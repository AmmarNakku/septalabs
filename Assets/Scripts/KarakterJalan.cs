using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public sealed class KarakterJalan : MonoBehaviour
{
    [SerializeField, Min(0f)] private float kecepatan = 5f;
    [SerializeField, Min(0f)] private float kecepatanLompat = 6f;
    [SerializeField] private Transform kamera;
    [SerializeField] private Transform visual;

    private const float KecepatanPutarVisual = 720f;

    private Rigidbody badan;
    private BoxCollider bentuk;
    private AnimasiTupai animasi;
    private bool mintaLompat;
    private bool sedangBergerak;
    private bool sedangMelompat;
    private bool spasiSebelumnya;
    private Vector2 input;

    private void Awake()
    {
        badan = GetComponent<Rigidbody>();
        bentuk = GetComponent<BoxCollider>();
        if (visual != null)
        {
            visual.TryGetComponent(out animasi);
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            spasiSebelumnya = Keyboard.current?.spaceKey.isPressed ?? false;
            return;
        }

        var arah = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            bool kanan = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
            bool kiri = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
            bool maju = keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed;
            bool mundur = keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed;

            if (kanan || kiri)
            {
                arah.x = (kanan ? 1f : 0f) - (kiri ? 1f : 0f);
            }

            if (maju || mundur)
            {
                arah.y = (maju ? 1f : 0f) - (mundur ? 1f : 0f);
            }
        }

        input = Vector2.ClampMagnitude(arah, 1f);
        bool spasiDitekan = keyboard?.spaceKey.isPressed ?? false;
        mintaLompat |= Input.GetKeyDown(KeyCode.Space) || (spasiDitekan && !spasiSebelumnya)
            || (keyboard?.spaceKey.wasPressedThisFrame ?? false);
        spasiSebelumnya = spasiDitekan;
    }

    private void FixedUpdate()
    {
        Vector3 depan = Vector3.ProjectOnPlane(kamera.forward, Vector3.up).normalized;
        Vector3 kanan = Vector3.Cross(Vector3.up, depan);
        Vector3 target = (depan * input.y + kanan * input.x) * kecepatan;
        Vector3 saatIni = Vector3.ProjectOnPlane(badan.linearVelocity, Vector3.up);
        badan.AddForce(target - saatIni, ForceMode.VelocityChange);

        bool bergerak = input.sqrMagnitude > 0.01f && kecepatan > 0f;
        if (visual != null && bergerak)
        {
            Quaternion arahVisual = Quaternion.LookRotation(target, Vector3.up);
            visual.rotation = Quaternion.RotateTowards(visual.rotation, arahVisual, KecepatanPutarVisual * Time.fixedDeltaTime);
        }

        if (bergerak != sedangBergerak)
        {
            sedangBergerak = bergerak;
            Debug.Log(bergerak ? "Pemain mulai bergerak." : "Pemain berhenti bergerak.", this);
        }

        bool turun = badan.linearVelocity.y <= 0.1f;
        bool menapak = turun && (mintaLompat || sedangMelompat || animasi != null) && Menapak();
        if (sedangMelompat && menapak)
        {
            sedangMelompat = false;
            Debug.Log("Pemain mendarat.", this);
        }

        bool mulaiLompat = mintaLompat && menapak && kecepatanLompat > 0f;
        if (mulaiLompat)
        {
            sedangMelompat = true;
            badan.AddForce(Vector3.up * kecepatanLompat, ForceMode.VelocityChange);
            Debug.Log("Pemain melompat.", this);
        }

        if (animasi != null)
        {
            float laju = kecepatan > 0f ? saatIni.magnitude / kecepatan : 0f;
            animasi.Perbarui(laju, menapak && !mulaiLompat, mulaiLompat || !turun);
        }

        mintaLompat = false;
    }

    public void KembaliKeAwal(Vector3 posisi, Quaternion rotasi)
    {
        badan.linearVelocity = Vector3.zero;
        badan.angularVelocity = Vector3.zero;
        transform.SetPositionAndRotation(posisi, rotasi);
        badan.position = posisi;
        badan.rotation = rotasi;
        if (visual != null)
        {
            visual.localRotation = Quaternion.identity;
        }

        input = Vector2.zero;
        mintaLompat = false;
        sedangBergerak = false;
        sedangMelompat = false;
        animasi?.SiapkanAwal();
        enabled = true;
    }

    private void OnDisable()
    {
        animasi?.Bekukan();
    }

    private void OnCollisionEnter(Collision benturan)
    {
        if (enabled && benturan.collider.TryGetComponent<ZonaRintangan>(out var rintangan))
        {
            Debug.Log($"Pemain bertabrakan dengan {rintangan.name}.", rintangan);
        }
    }


    private bool Menapak()
    {
        Bounds batas = bentuk.bounds;
        float jarak = batas.extents.y + 0.07f;
        Vector3 pusat = batas.center;
        float sisiX = batas.extents.x * 0.85f;
        float sisiZ = batas.extents.z * 0.85f;

        return AdaPijakan(pusat, jarak)
            || AdaPijakan(pusat + new Vector3(sisiX, 0f, sisiZ), jarak)
            || AdaPijakan(pusat + new Vector3(-sisiX, 0f, sisiZ), jarak)
            || AdaPijakan(pusat + new Vector3(sisiX, 0f, -sisiZ), jarak)
            || AdaPijakan(pusat + new Vector3(-sisiX, 0f, -sisiZ), jarak);
    }

    private static bool AdaPijakan(Vector3 asal, float jarak)
    {
        return Physics.Raycast(asal, Vector3.down, out RaycastHit hit, jarak,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
            && hit.normal.y > 0.5f;
    }
}

