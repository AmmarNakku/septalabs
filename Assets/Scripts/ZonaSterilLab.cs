// Ammar Mohamad Qatar - 2491345011 - GJ24B
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider))]
public sealed class ZonaSterilLab : MonoBehaviour
{
    [SerializeField] private ProgresSampelLab progres;
    [SerializeField] private Text notifikasi;
    [SerializeField] private GameObject panelSelesai;
    [SerializeField] private Text teksHasil;
    [SerializeField] private GameObject crosshair;
    [SerializeField] private GameObject petunjukKontrol;

    private bool selesai;
    private bool memuatUlang;
    private BoxCollider batasZona;
    private Collider pemainDiZona;

    private void Awake()
    {
        batasZona = GetComponent<BoxCollider>();
    }

    private void LateUpdate()
    {
        // CharacterController can leave without an exit callback when repositioned.
        if (pemainDiZona != null && (!pemainDiZona.enabled || !batasZona.bounds.Intersects(pemainDiZona.bounds)))
            HapusNotifikasi();
    }

    private void OnTriggerEnter(Collider objek)
    {
        if (selesai || !objek.CompareTag("Player"))
            return;

        if (progres.JumlahSampel < 3)
        {
            pemainDiZona = objek;
            notifikasi.text = "Zona Steril Aktif — Kumpulkan 3 Sampel Dahulu";
            return;
        }

        selesai = true;
        HapusNotifikasi();
        progres.HentikanTimer();
        teksHasil.text = $"Sampel: {progres.JumlahSampel} / 3   |   Total poin: {progres.TotalPoin}\nWaktu misi: {progres.WaktuMisi}";
        crosshair.SetActive(false);
        petunjukKontrol.SetActive(false);
        panelSelesai.SetActive(true);
        if (objek.TryGetComponent(out KontrolFpsLab kontrol))
            kontrol.enabled = false;
        if (objek.TryGetComponent(out RaycastCollector raycast))
            raycast.enabled = false;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnTriggerExit(Collider objek)
    {
        if (objek == pemainDiZona)
            HapusNotifikasi();
    }

    private void HapusNotifikasi()
    {
        pemainDiZona = null;
        notifikasi.text = "";
    }

    public void Restart()
    {
        if (!selesai || memuatUlang)
            return;

        memuatUlang = true;
        SceneManager.LoadScene(gameObject.scene.path);
    }
}
