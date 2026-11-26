using UnityEngine;
#if !UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public sealed class KameraOrbit : MonoBehaviour
{
    [SerializeField] private Transform pemain;
    [SerializeField, Min(0f)] private float tinggiFokus = 0.5f;
    [SerializeField, Min(0.01f)] private float sensitivitas = 0.65f;
    [SerializeField, Min(0.5f)] private float jarakAwal = 8.5f;
    [SerializeField, Min(0.5f)] private float jarakMinimum = 3f;
    [SerializeField, Min(0.5f)] private float jarakMaksimum = 15f;

    private const float PitchMinimum = 8f;
    private const float PitchMaksimum = 75f;
    private const float RadiusKamera = 0.18f;

    private readonly RaycastHit[] benturan = new RaycastHit[16];
    private bool sedangDrag;
    private float yaw;
    private float pitch;
    private float jarakTarget;
    private float jarakZoom;
    private float jarakAktual;

    private void Awake()
    {
        Vector3 rotasi = transform.eulerAngles;
        yaw = rotasi.y;
        pitch = Mathf.Clamp(Mathf.DeltaAngle(0f, rotasi.x), PitchMinimum, PitchMaksimum);
        jarakTarget = jarakZoom = jarakAwal;
        TempatkanSegera();
    }

    private void LateUpdate()
    {
        if (Time.timeScale == 0f)
        {
            AturDrag(false);
            return;
        }

#if !UNITY_EDITOR
        BacaMouseBuild();
#endif
        jarakZoom = Mathf.MoveTowards(jarakZoom, jarakTarget, 18f * Time.unscaledDeltaTime);
        TempatkanKamera(false);
    }

#if UNITY_EDITOR
    // Game View meneruskan event mouse meskipun polling perangkat tidak selalu membacanya.
    private void OnGUI()
    {
        if (Time.timeScale == 0f)
        {
            return;
        }

        Event input = Event.current;
        switch (input.rawType)
        {
            case EventType.MouseDown when input.button == 1:
                AturDrag(true);
                break;
            case EventType.MouseDrag when input.button == 1 && sedangDrag:
                Putar(input.delta);
                break;
            case EventType.MouseUp when input.button == 1:
            case EventType.KeyDown when input.keyCode == KeyCode.Escape:
                AturDrag(false);
                break;
            case EventType.ScrollWheel:
                Zoom(-input.delta.y / 3f);
                break;
        }
    }
#else
    private void BacaMouseBuild()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || !Application.isFocused)
        {
            AturDrag(false);
            return;
        }

        if (mouse.rightButton.wasPressedThisFrame)
        {
            AturDrag(true);
        }

        if (!mouse.rightButton.isPressed || (Keyboard.current?.escapeKey.wasPressedThisFrame ?? false))
        {
            AturDrag(false);
        }

        if (sedangDrag)
        {
            Vector2 delta = mouse.delta.ReadValue();
            Putar(new Vector2(delta.x, -delta.y));
        }

        float gulir = mouse.scroll.ReadValue().y;
        Zoom(Mathf.Abs(gulir) > 10f ? gulir / 120f : gulir);
    }
#endif

    private void Putar(Vector2 delta)
    {
        yaw = Mathf.Repeat(yaw + delta.x * sensitivitas, 360f);
        pitch = Mathf.Clamp(pitch + delta.y * sensitivitas, PitchMinimum, PitchMaksimum);
    }

    private void Zoom(float langkah)
    {
        jarakTarget = Mathf.Clamp(jarakTarget - langkah * 0.85f, jarakMinimum, jarakMaksimum);
    }

    public void TempatkanSegera()
    {
        TempatkanKamera(true);
    }

    private void TempatkanKamera(bool langsung)
    {
        if (pemain == null)
        {
            return;
        }

        Vector3 fokus = pemain.position + Vector3.up * tinggiFokus;
        Quaternion rotasi = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 keBelakang = rotasi * Vector3.back;
        float jarakAman = jarakZoom;
        int jumlah = Physics.SphereCastNonAlloc(fokus, RadiusKamera, keBelakang, benturan, jarakZoom,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < jumlah; i++)
        {
            RaycastHit hit = benturan[i];
            if (!hit.collider.transform.IsChildOf(pemain))
            {
                jarakAman = Mathf.Min(jarakAman, Mathf.Max(0.12f, hit.distance - 0.08f));
            }
        }

        // Dekatkan kamera segera saat terhalang, lalu kembalikan jaraknya secara bertahap.
        jarakAktual = langsung || jarakAman < jarakAktual
            ? jarakAman
            : Mathf.MoveTowards(jarakAktual, jarakAman, 18f * Time.unscaledDeltaTime);
        transform.SetPositionAndRotation(fokus + keBelakang * jarakAktual, rotasi);
    }

    private void AturDrag(bool aktif)
    {
        if (sedangDrag == aktif)
        {
            return;
        }

        sedangDrag = aktif;
        Debug.Log(aktif ? "Pemain mulai memutar kamera." : "Pemain selesai memutar kamera.", this);
    }

    private void OnApplicationFocus(bool fokus)
    {
        if (!fokus)
        {
            AturDrag(false);
        }
    }

    private void OnDisable()
    {
        AturDrag(false);
    }
}
