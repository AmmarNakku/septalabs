using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class TembakanPemain : MonoBehaviour
{
    [SerializeField] private Transform kamera;
    [SerializeField] private ProyektilPemain prefab;
    [SerializeField] private AudioArena audioArena;
    [SerializeField, Min(0f)] private float jedaTembak = 0.25f;

    private readonly List<ProyektilPemain> tembakan = new();
    private readonly RaycastHit[] bidikan = new RaycastHit[32];
    private Camera kameraGame;
    private float waktuTembakBerikutnya;

    private void Awake()
    {
        kameraGame = kamera != null ? kamera.GetComponent<Camera>() : Camera.main;
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        Event input = Event.current;
        if (input.rawType == EventType.MouseDown && input.button == 0)
        {
            Tembak(new Vector2(input.mousePosition.x, Screen.height - input.mousePosition.y));
        }
    }
#else
    private void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            Tembak(mouse.position.ReadValue());
        }
    }
#endif

    private void Tembak(Vector2 posisiMouse)
    {
        if (Time.timeScale == 0f || Time.time < waktuTembakBerikutnya || prefab == null || kameraGame == null)
        {
            return;
        }

        Vector3 asal = transform.position + Vector3.up * 0.15f;
        Vector3 arah = ArahTembak(asal, posisiMouse);
        if (arah == Vector3.zero)
        {
            return;
        }

        waktuTembakBerikutnya = Time.time + jedaTembak;
        var proyektil = Instantiate(prefab, asal, Quaternion.LookRotation(arah));
        proyektil.Luncurkan(transform, arah);
        tembakan.RemoveAll(item => item == null);
        tembakan.Add(proyektil);
        audioArena?.PutarTembakan();
        Debug.Log("Pemain menembakkan proyektil.", this);
    }

    private Vector3 ArahTembak(Vector3 asal, Vector2 posisiMouse)
    {
        Vector3 depan = Vector3.ProjectOnPlane(kameraGame.transform.forward, Vector3.up).normalized;
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            return depan;
        }

        Ray garis = kameraGame.ScreenPointToRay(posisiMouse);
        float jarak = kameraGame.farClipPlane;
        if (ProyektilPemain.CariBenturan(garis.origin, garis.direction, jarak, transform, bidikan,
                out RaycastHit kena, out MusuhArena musuh) && musuh != null)
        {
            return (kena.collider.bounds.center - asal).normalized;
        }

        var bidangBidik = new Plane(Vector3.up, asal);
        if (bidangBidik.Raycast(garis, out float jarakBidik))
        {
            Vector3 arah = garis.GetPoint(jarakBidik) - asal;
            if (arah.sqrMagnitude > 0.0025f)
            {
                return arah.normalized;
            }
        }

        return depan;
    }

    private void OnDisable()
    {
        foreach (ProyektilPemain proyektil in tembakan)
        {
            if (proyektil != null)
            {
                proyektil.Batalkan();
            }
        }

        tembakan.Clear();
        waktuTembakBerikutnya = 0f;
    }
}
