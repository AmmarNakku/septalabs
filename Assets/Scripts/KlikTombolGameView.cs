using UnityEngine;
using UnityEngine.UI;

public sealed class KlikTombolGameView : MonoBehaviour
{
    [SerializeField] private Button[] tombol;

#if UNITY_EDITOR
    private Button tombolDitekan;

    private void OnGUI()
    {
        // Game View dapat meneruskan klik lewat IMGUI saat Input System tidak membacanya.
        Event input = Event.current;
        if (input.button != 0 || (input.rawType != EventType.MouseDown && input.rawType != EventType.MouseUp))
        {
            return;
        }

        var posisi = new Vector2(input.mousePosition.x, Screen.height - input.mousePosition.y);
        if (input.rawType == EventType.MouseDown)
        {
            tombolDitekan = System.Array.Find(tombol, item => AdaDiTombol(item, posisi));
            return;
        }

        Button target = tombolDitekan;
        tombolDitekan = null;
        if (AdaDiTombol(target, posisi))
        {
            target.onClick.Invoke();
        }
    }

    private static bool AdaDiTombol(Button target, Vector2 posisi)
    {
        return target != null && target.isActiveAndEnabled && target.IsInteractable()
            && RectTransformUtility.RectangleContainsScreenPoint((RectTransform)target.transform, posisi);
    }

    private void OnDisable()
    {
        tombolDitekan = null;
    }
#endif
}
