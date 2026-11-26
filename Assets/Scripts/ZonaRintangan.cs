using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public sealed class ZonaRintangan : MonoBehaviour
{
    [SerializeField, Min(0f)] private float jarakDeteksi = 2f;

    private BoxCollider batas;
    public float JarakDeteksi => jarakDeteksi;
    public bool PemainTerdeteksi { get; private set; }

    private void Awake()
    {
        batas = GetComponent<BoxCollider>();
    }

    public void Periksa(Vector3 posisiPemain)
    {
        Vector3 titikTerdekat = batas.ClosestPoint(posisiPemain);
        AturDeteksi((posisiPemain - titikTerdekat).sqrMagnitude <= jarakDeteksi * jarakDeteksi);
    }

    public void HapusDeteksi()
    {
        AturDeteksi(false);
    }

    private void AturDeteksi(bool terdeteksi)
    {
        if (PemainTerdeteksi == terdeteksi)
        {
            return;
        }

        PemainTerdeteksi = terdeteksi;
        Debug.Log(terdeteksi ? $"Pemain masuk zona {name}." : $"Pemain keluar zona {name}.", this);
    }
}
