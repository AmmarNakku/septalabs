// Ammar Mohamad Qatar - 2491345011 - GJ24B
using UnityEngine;

public sealed class RaycastCollector : MonoBehaviour
{
    public Camera kameraPemain;
    [Min(0f)] public float jarakAmbil = 4f;
    public bool tampilkanGarisDebug = true;

    private void Start()
    {
        if (kameraPemain == null)
            kameraPemain = Camera.main;
    }

    private void Update()
    {
        if (kameraPemain == null)
            return;

        if (tampilkanGarisDebug)
        {
            Ray garis = kameraPemain.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Debug.DrawRay(garis.origin, garis.direction * jarakAmbil, Color.cyan);
        }

        if (!Input.GetKeyDown(KeyCode.E))
            return;

        Interaksi();
    }

    private void Interaksi()
    {
        var pusat = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        Ray ray = kameraPemain.ScreenPointToRay(pusat);
        if (!Physics.Raycast(ray, out RaycastHit hit, jarakAmbil, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            return;

        Debug.Log($"[RAYCAST HIT] {hit.collider.name} ({hit.distance:F2} m)", this);
        if (hit.collider.CompareTag("Collectible"))
        {
            var sampel = hit.collider.GetComponent<Collectible>();
            if (sampel != null)
                sampel.Ambil();
        }
        else if (hit.collider.CompareTag("Interactable"))
        {
            var panel = hit.collider.GetComponent<LightSwitch>();
            if (panel != null)
                panel.ToggleLampu();
        }
    }
}
