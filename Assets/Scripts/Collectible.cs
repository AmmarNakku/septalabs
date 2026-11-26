// Ammar Mohamad Qatar - 2491345011 - GJ24B
using UnityEngine;

public sealed class Collectible : MonoBehaviour
{
    [SerializeField] private string namaSampel;
    [SerializeField, Min(0)] private int poin;
    [SerializeField] private ProgresSampelLab progres;
    [SerializeField] private AudioClip suaraAmbil;

    private bool sudahDiambil;

    public void Ambil()
    {
        if (sudahDiambil || progres == null)
            return;

        sudahDiambil = true;
        Debug.Log($"[COLLECTIBLE] {namaSampel} (+{poin} poin)", this);
        progres.CatatSampel(poin, suaraAmbil);
        Destroy(gameObject);
    }
}
