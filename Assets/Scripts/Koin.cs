using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public sealed class Koin : MonoBehaviour
{
    private SistemGame sistem;
    private bool diambil;

    private void Awake()
    {
        sistem = FindFirstObjectByType<SistemGame>();
    }

    public void Pulihkan()
    {
        diambil = false;
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider lainnya)
    {
        if (diambil || sistem == null || lainnya.attachedRigidbody == null)
        {
            return;
        }

        Rigidbody badan = lainnya.attachedRigidbody;
        if (badan.CompareTag("Player") && badan.TryGetComponent(out KarakterJalan pemain)
            && sistem.CobaTambahSkor(pemain, this))
        {
            diambil = true;
            gameObject.SetActive(false);
        }
    }
}
