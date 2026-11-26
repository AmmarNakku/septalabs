using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public sealed class AudioArena : MonoBehaviour
{
    [SerializeField] private AudioClip tembakan;
    [SerializeField] private AudioClip musuhTerkena;
    [SerializeField] private AudioClip koin;
    [SerializeField] private AudioClip pemainTerkena;

    private AudioSource sumber;

    private void Awake()
    {
        sumber = GetComponent<AudioSource>();
    }

    public void PutarTembakan() => Putar(tembakan, 0.68f);
    public void PutarMusuhTerkena() => Putar(musuhTerkena, 0.77f);
    public void PutarKoin() => Putar(koin, 0.74f);
    public void PutarPemainTerkena() => Putar(pemainTerkena, 0.8f);

    private void Putar(AudioClip klip, float volume)
    {
        if (klip != null)
        {
            sumber.PlayOneShot(klip, volume);
        }
    }
}
