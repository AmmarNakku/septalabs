using UnityEngine;

public sealed class ProyektilPemain : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float kecepatan = 22f;
    [SerializeField, Min(0.1f)] private float masaAktif = 1.5f;

    private readonly RaycastHit[] hasil = new RaycastHit[16];
    private Transform pemilik;
    private Vector3 arah;
    private float sisaWaktu;

    private void Awake()
    {
        var objekJejak = new GameObject("Jejak");
        objekJejak.transform.SetParent(transform, false);
        var jejak = objekJejak.AddComponent<TrailRenderer>();
        jejak.sharedMaterial = GetComponent<MeshRenderer>().sharedMaterial;
        jejak.time = 0.14f;
        jejak.startWidth = 0.15f;
        jejak.endWidth = 0f;
        jejak.minVertexDistance = 0.05f;
        jejak.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    public void Luncurkan(Transform penembak, Vector3 arahTembak)
    {
        pemilik = penembak;
        arah = arahTembak;
        sisaWaktu = masaAktif;
    }

    private void FixedUpdate()
    {
        float langkah = Mathf.Min(sisaWaktu, Time.fixedDeltaTime);
        float jarak = kecepatan * langkah;
        if (CariBenturan(transform.position, arah, jarak, pemilik, hasil, out RaycastHit kena, out MusuhArena musuh))
        {
            if (musuh != null)
            {
                musuh.TerimaTembakan();
            }
            else
            {
                Debug.Log($"Proyektil terhalang oleh {kena.collider.name}.", kena.collider);
            }

            Batalkan();
            return;
        }

        transform.position += arah * jarak;
        sisaWaktu -= langkah;
        if (sisaWaktu <= 0f)
        {
            Batalkan();
        }
    }

    internal static bool CariBenturan(Vector3 asal, Vector3 arah, float jarak, Transform pemilik,
        RaycastHit[] hasil, out RaycastHit terdekat, out MusuhArena musuh)
    {
        int jumlah = Physics.RaycastNonAlloc(asal, arah, hasil, jarak,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
        terdekat = default;
        musuh = null;

        for (int i = 0; i < jumlah; i++)
        {
            RaycastHit kena = hasil[i];
            if (kena.transform.IsChildOf(pemilik) || (terdekat.collider != null && kena.distance >= terdekat.distance))
            {
                continue;
            }

            MusuhArena sasaran = kena.collider.GetComponentInParent<MusuhArena>();
            if (kena.collider.isTrigger && sasaran == null)
            {
                continue;
            }

            terdekat = kena;
            musuh = sasaran;
        }

        return terdekat.collider != null;
    }

    public void Batalkan()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
