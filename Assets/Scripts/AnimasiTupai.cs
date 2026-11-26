using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[DisallowMultipleComponent]
public sealed class AnimasiTupai : MonoBehaviour
{
    [SerializeField] private Transform model;

    private enum Gerakan { Diam, Jalan, Lompat, Jatuh, Mendarat }
    private static readonly string[] NamaKlip = { "Idle", "Walk_Loop", "Lompat", "Jatuh", "Mendarat" };

    private readonly AnimationClipPlayable[] klip = new AnimationClipPlayable[NamaKlip.Length];
    private readonly float[] durasi = new float[NamaKlip.Length];
    private PlayableGraph grafik;
    private AnimationMixerPlayable mixer;
    private Gerakan gerakan;
    private float laju;
    private float waktuKlip;
    private bool menapak = true;
    private bool naik;
    private bool beku;

    private void Awake()
    {
        var sumber = new AnimationClip[NamaKlip.Length];
        foreach (AnimationClip clip in Resources.LoadAll<AnimationClip>("Tupai"))
        {
            string nama = clip.name.Substring(clip.name.LastIndexOf('|') + 1);
            int index = System.Array.IndexOf(NamaKlip, nama);
            if (index >= 0)
            {
                sumber[index] = clip;
            }
        }

        if (model == null || System.Array.Exists(sumber, item => item == null || item.length <= 0f))
        {
            Debug.LogError("Model atau klip animasi tupai belum lengkap.", this);
            return;
        }

        if (!model.TryGetComponent(out Animator animator))
        {
            animator = model.gameObject.AddComponent<Animator>();
        }

        animator.applyRootMotion = false;
        grafik = PlayableGraph.Create("Animasi Tupai");
        grafik.SetTimeUpdateMode(DirectorUpdateMode.Manual);
        mixer = AnimationMixerPlayable.Create(grafik, klip.Length);
        for (int i = 0; i < klip.Length; i++)
        {
            durasi[i] = sumber[i].length;
            klip[i] = AnimationClipPlayable.Create(grafik, sumber[i]);
            klip[i].SetSpeed(0);
            grafik.Connect(klip[i], 0, mixer, i);
        }

        var output = AnimationPlayableOutput.Create(grafik, "Rig Tupai", animator);
        output.SetSourcePlayable(mixer);
        grafik.Play();
        SiapkanAwal();
    }

    internal void Perbarui(float lajuGerak, bool diTanah, bool bergerakNaik)
    {
        laju = lajuGerak;
        menapak = diTanah;
        naik = bergerakNaik;
    }

    internal void Bekukan() => beku = true;

    internal void SiapkanAwal()
    {
        Perbarui(0f, true, false);
        beku = false;
        gerakan = Gerakan.Diam;
        waktuKlip = 0f;
        if (!grafik.IsValid())
        {
            return;
        }

        for (int i = 0; i < klip.Length; i++)
        {
            klip[i].SetTime(0);
            mixer.SetInputWeight(i, i == (int)Gerakan.Diam ? 1f : 0f);
        }

        grafik.Evaluate(0f);
    }

    private void LateUpdate()
    {
        float delta = Time.deltaTime;
        if (beku || delta <= 0f || !grafik.IsValid())
        {
            return;
        }

        Gerakan berikutnya = PilihGerakan();
        if (gerakan != berikutnya)
        {
            gerakan = berikutnya;
            waktuKlip = 0f;
        }

        int aktif = (int)gerakan;
        float tempo = gerakan == Gerakan.Jalan ? Mathf.Lerp(0.55f, 1.6f, laju) : 1f;
        waktuKlip += delta * tempo;
        bool ulang = gerakan == Gerakan.Diam || gerakan == Gerakan.Jalan;
        klip[aktif].SetTime(ulang ? waktuKlip % durasi[aktif] : Mathf.Min(waktuKlip, durasi[aktif]));

        float transisi = 1f - Mathf.Exp(-32f * delta);
        for (int i = 0; i < klip.Length; i++)
        {
            mixer.SetInputWeight(i, Mathf.Lerp(mixer.GetInputWeight(i), i == aktif ? 1f : 0f, transisi));
        }

        grafik.Evaluate(0f);
    }

    private Gerakan PilihGerakan()
    {
        if (!menapak)
        {
            return naik ? Gerakan.Lompat : Gerakan.Jatuh;
        }

        if (gerakan == Gerakan.Lompat || gerakan == Gerakan.Jatuh
            || (gerakan == Gerakan.Mendarat && waktuKlip < durasi[(int)Gerakan.Mendarat]))
        {
            return Gerakan.Mendarat;
        }

        return laju > 0.045f ? Gerakan.Jalan : Gerakan.Diam;
    }

    private void OnDestroy()
    {
        if (grafik.IsValid())
        {
            grafik.Destroy();
        }
    }
}
