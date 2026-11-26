using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class NavigasiScene : MonoBehaviour
{
    public const string Menu = "Assets/ArenaKetangkasan/MenuUtama.unity";
    public const string Arena = "Assets/ArenaKetangkasan/ArenaKetangkasan.unity";

    private AsyncOperation pemuatan;
    internal bool SedangMemuat => pemuatan != null;

    public void Mulai() => Buka(Arena);

    public void KembaliKeMenu() => Buka(Menu);

    private void Buka(string scene)
    {
        if (SedangMemuat)
        {
            return;
        }

        pemuatan = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
    }
}
