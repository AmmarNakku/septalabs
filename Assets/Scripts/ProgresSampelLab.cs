// Ammar Mohamad Qatar - 2491345011 - GJ24B
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public sealed class ProgresSampelLab : MonoBehaviour
{
    [SerializeField] private Text teksProgres;

    private double waktuMulai;
    private int detikTampil;
    private bool waktuBerhenti;

    public int JumlahSampel { get; private set; }
    public int TotalPoin { get; private set; }
    public string WaktuMisi => $"{detikTampil / 60:00}:{detikTampil % 60:00}";

    private void Start()
    {
        waktuMulai = Time.timeAsDouble;
        PerbaruiHud();
    }

    private void Update()
    {
        if (!waktuBerhenti)
            PerbaruiWaktu();
    }

    public void HentikanTimer()
    {
        if (waktuBerhenti)
            return;

        PerbaruiWaktu();
        waktuBerhenti = true;
    }

    public void CatatSampel(int poin, AudioClip suara)
    {
        JumlahSampel++;
        TotalPoin += poin;
        PerbaruiHud();

        if (suara != null)
            GetComponent<AudioSource>().PlayOneShot(suara);
    }

    private void PerbaruiWaktu()
    {
        int detik = (int)(Time.timeAsDouble - waktuMulai);
        if (detik == detikTampil)
            return;

        detikTampil = detik;
        PerbaruiHud();
    }

    private void PerbaruiHud()
    {
        teksProgres.text = $"Sampel: {JumlahSampel} / 3 | Poin: {TotalPoin}\nWaktu: {WaktuMisi}";
    }
}
