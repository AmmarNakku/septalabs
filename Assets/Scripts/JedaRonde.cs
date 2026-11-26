using UnityEngine;
using UnityEngine.UI;
#if !UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
[RequireComponent(typeof(SistemGame), typeof(NavigasiScene))]
public sealed class JedaRonde : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button tombolLanjutkan;

    private SistemGame sistem;
    private NavigasiScene navigasi;
    private bool jedaAktif;
    private float skalaWaktuSebelumnya;
    private bool audioDijedaSebelumnya;

    private void Awake()
    {
        sistem = GetComponent<SistemGame>();
        navigasi = GetComponent<NavigasiScene>();
    }

#if UNITY_EDITOR
    private bool escDitahan;

    private void OnGUI()
    {
        Event input = Event.current;
        if (input.keyCode != KeyCode.Escape)
        {
            return;
        }

        if (input.rawType == EventType.KeyUp)
        {
            escDitahan = false;
        }
        else if (input.rawType == EventType.KeyDown && !escDitahan)
        {
            escDitahan = true;
            UbahJeda();
        }
    }

    private void OnApplicationFocus(bool fokus)
    {
        if (!fokus)
        {
            escDitahan = false;
        }
    }
#else
    private void Update()
    {
        if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
        {
            UbahJeda();
        }
    }
#endif

    private void UbahJeda()
    {
        if (!sistem.RondeBerjalan || navigasi.SedangMemuat)
        {
            return;
        }

        if (jedaAktif)
        {
            Lanjutkan();
            return;
        }

        skalaWaktuSebelumnya = Time.timeScale;
        audioDijedaSebelumnya = AudioListener.pause;
        jedaAktif = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        panel.SetActive(true);
        tombolLanjutkan.Select();
        Debug.Log("Ronde dijeda.", this);
    }

    public void Lanjutkan()
    {
        if (!jedaAktif || navigasi.SedangMemuat)
        {
            return;
        }

        panel.SetActive(false);
        PulihkanWaktuDanAudio();
        Debug.Log("Ronde dilanjutkan.", this);
    }

    private void OnDisable()
    {
        if (jedaAktif)
        {
            PulihkanWaktuDanAudio();
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }

    private void PulihkanWaktuDanAudio()
    {
        Time.timeScale = skalaWaktuSebelumnya;
        AudioListener.pause = audioDijedaSebelumnya;
        jedaAktif = false;
    }
}
