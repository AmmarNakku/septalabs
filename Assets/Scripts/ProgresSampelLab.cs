// Ammar Mohamad Qatar - 2491345011 - GJ24B
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public sealed class ProgresSampelLab : MonoBehaviour
{
    [SerializeField] private Text teksProgres;

    public int JumlahSampel { get; private set; }
    public int TotalPoin { get; private set; }

    public void CatatSampel(int poin, AudioClip suara)
    {
        JumlahSampel++;
        TotalPoin += poin;
        teksProgres.text = $"Sampel: {JumlahSampel} / 3 | Poin: {TotalPoin}";

        if (suara != null)
            GetComponent<AudioSource>().PlayOneShot(suara);
    }
}
