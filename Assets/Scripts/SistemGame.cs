using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public sealed class SistemGame : MonoBehaviour
{
    [SerializeField] private KarakterJalan pemain;
    [SerializeField] private Transform lingkungan;
    [SerializeField] private float batasJatuh = -2f;
    [SerializeField, Min(0f)] private float jedaRespawn = 1.25f;
    [SerializeField] private Text hud;
    [FormerlySerializedAs("layarMenang")]
    [SerializeField] private LayarHasilRonde layarHasil;
    [SerializeField] private MusuhArena musuh;
    [SerializeField, Min(1)] private int hpMaksimum = 3;
    [SerializeField, Min(0.02f)] private float jedaDamage = 1f;

    private TembakanPemain tembakan;
    private AudioArena audioArena;
    private Koin[] koinArena;
    private ZonaRintangan[] zona;
    private KameraOrbit kameraArena;
    private Vector3 posisiAwal;
    private Quaternion rotasiAwal;
    private enum StatusRonde { Bermain, Kalah, Menang }
    private StatusRonde status;
    private int ronde;
    private int skor;
    private int detik;
    private int hp;
    private float waktuMulai;
    private float waktuRespawn;
    private float waktuDamageBerikutnya;

    private int DetikRonde => Mathf.FloorToInt(Time.time - waktuMulai);
    internal bool RondeBerjalan => ronde > 0 && status == StatusRonde.Bermain;

    private void Awake()
    {
        TryGetComponent(out audioArena);
        Debug.Log("Sistem Game Disiapkan", this);
    }

    private void Start()
    {
        posisiAwal = pemain.transform.position;
        pemain.TryGetComponent(out tembakan);
        rotasiAwal = pemain.transform.rotation;
        koinArena = FindObjectsByType<Koin>(FindObjectsSortMode.None);
        zona = lingkungan.GetComponentsInChildren<ZonaRintangan>();
        kameraArena = FindFirstObjectByType<KameraOrbit>();

        foreach (ZonaRintangan rintangan in zona)
        {
            Debug.Log($"Zona {rintangan.name} siap (jarak {rintangan.JarakDeteksi:0.##} m)", rintangan);
        }

        if (musuh != null)
        {
            var navigasi = lingkungan.GetComponent<NavMeshSurface>();
            navigasi.BuildNavMesh();
            musuh.Siapkan(this, pemain);
        }

        MulaiRonde();
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            return;
        }

        if (status != StatusRonde.Bermain)
        {
            if (status == StatusRonde.Kalah && layarHasil == null && Time.time >= waktuRespawn)
            {
                Respawn();
            }

            return;
        }

        Vector3 posisi = pemain.transform.position;
        if (posisi.y < batasJatuh)
        {
            AkhiriRonde("Pemain jatuh.");
            return;
        }

        foreach (ZonaRintangan rintangan in zona)
        {
            rintangan.Periksa(posisi);
        }

        int detikSekarang = DetikRonde;
        if (detikSekarang != detik)
        {
            detik = detikSekarang;
            PerbaruiHud();
        }
    }

    public bool CobaTambahSkor(KarakterJalan pengambil, Koin koin)
    {
        if (!RondeBerjalan || Time.timeScale == 0f || pengambil != pemain || pemain.transform.position.y < batasJatuh)
        {
            return false;
        }

        skor++;
        audioArena?.PutarKoin();
        Debug.Log($"{koin.name} diambil. Skor: {skor}.", koin);
        PeriksaMenang();
        PerbaruiHud();
        return true;
    }

    public void CobaBeriDamage(MusuhArena penyerang)
    {
        if (!RondeBerjalan || Time.timeScale == 0f || penyerang != musuh || Time.time < waktuDamageBerikutnya
            || pemain.transform.position.y < batasJatuh)
        {
            return;
        }

        hp--;
        audioArena?.PutarPemainTerkena();
        waktuDamageBerikutnya = Time.time + jedaDamage;
        Debug.Log($"Pemain terkena {penyerang.name}. HP: {hp}/{hpMaksimum}.", pemain);

        if (hp == 0)
        {
            AkhiriRonde("HP pemain habis.");
            return;
        }

        PerbaruiHud();
    }

    private void AkhiriRonde(string penyebab)
    {
        HentikanRonde(StatusRonde.Kalah);
        waktuRespawn = Time.time + jedaRespawn;
        Debug.Log($"{penyebab} Kamu kalah. Ronde {ronde}, skor {skor}, waktu {detik} detik.", this);
        PerbaruiHud();
    }

    private void PeriksaMenang()
    {
        if (status != StatusRonde.Bermain || pemain.transform.position.y < batasJatuh
            || layarHasil == null || skor != koinArena.Length || musuh == null || musuh.HP != 0)
        {
            return;
        }

        HentikanRonde(StatusRonde.Menang);
        Debug.Log($"Kamu menang. Ronde {ronde}, skor {skor}, waktu {detik} detik.", this);
    }

    public void MainLagi()
    {
        if (status != StatusRonde.Bermain)
        {
            Respawn();
        }
    }

    private void HentikanRonde(StatusRonde hasil)
    {
        detik = DetikRonde;
        status = hasil;
        pemain.enabled = false;
        if (tembakan != null)
        {
            tembakan.enabled = false;
        }

        musuh?.Hentikan();

        foreach (ZonaRintangan rintangan in zona)
        {
            rintangan.HapusDeteksi();
        }

        layarHasil?.Tampilkan(hasil == StatusRonde.Menang, skor, koinArena.Length, FormatWaktu(detik));
    }

    private void Respawn()
    {
        pemain.KembaliKeAwal(posisiAwal, rotasiAwal);
        if (tembakan != null)
        {
            tembakan.enabled = true;
        }

        kameraArena?.TempatkanSegera();

        foreach (Koin koin in koinArena)
        {
            koin.Pulihkan();
        }

        musuh?.KembaliKeAwal();
        Debug.Log($"Pemain respawn di {posisiAwal}. Koin dipulihkan.", pemain);
        MulaiRonde();
    }

    private void MulaiRonde()
    {
        ronde++;
        status = StatusRonde.Bermain;
        layarHasil?.Sembunyikan();
        skor = 0;
        detik = 0;
        hp = hpMaksimum;
        waktuMulai = Time.time;
        waktuDamageBerikutnya = waktuMulai;
        Debug.Log($"Ronde {ronde} dimulai. Skor, timer, dan HP direset.", this);
        PerbaruiHud();
    }

    internal void CatatMusuhTerkena()
    {
        audioArena?.PutarMusuhTerkena();
        PeriksaMenang();
        PerbaruiHud();
    }

    private static string FormatWaktu(int durasi)
    {
        return $"{durasi / 60:00}:{durasi % 60:00}";
    }

    private void PerbaruiHud()
    {
        if (hud != null)
        {
            string baris = System.Environment.NewLine;
            hud.text = $"HP  {hp}/{hpMaksimum}{baris}Skor  {skor}{baris}Waktu  {FormatWaktu(detik)}";
            if (musuh != null)
            {
                hud.text += $"{baris}Musuh  {musuh.HP}/{musuh.HPMaksimum}";
            }

            if (status == StatusRonde.Kalah && layarHasil == null)
            {
                hud.text += baris + "Kamu kalah";
            }
        }
    }
}
