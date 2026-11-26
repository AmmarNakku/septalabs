using UnityEngine;
using UnityEngine.UI;

public sealed class LayarHasilRonde : MonoBehaviour
{
    [SerializeField] private Text judul;
    [SerializeField] private Text ringkasan;
    [SerializeField] private Button tombolMainLagi;

    public void Tampilkan(bool menang, int skor, int jumlahKoin, string waktu)
    {
        judul.text = menang ? "Kamu Menang" : "Kamu Kalah";
        ringkasan.text = $"Koin  {skor}/{jumlahKoin}\nWaktu  {waktu}";
        gameObject.SetActive(true);
        tombolMainLagi.Select();
    }

    public void Sembunyikan()
    {
        gameObject.SetActive(false);
    }
}
