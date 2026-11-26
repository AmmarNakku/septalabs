// Ammar Mohamad Qatar - 2491345011 - GJ24B
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public sealed class LightSwitch : MonoBehaviour
{
    public Light lampuRuangan;
    [SerializeField] private Color warnaMenyala = new Color(0.14f, 0.92f, 0.34f);
    [SerializeField] private Color warnaPadam = new Color(0.95f, 0.16f, 0.14f);

    private Renderer indikator;
    private MaterialPropertyBlock properti;

    private void Awake()
    {
        indikator = GetComponent<Renderer>();
        properti = new MaterialPropertyBlock();
        if (lampuRuangan != null)
            PerbaruiIndikator();
    }

    public void ToggleLampu()
    {
        if (lampuRuangan == null)
            return;

        lampuRuangan.enabled = !lampuRuangan.enabled;
        PerbaruiIndikator();
        Debug.Log(lampuRuangan.enabled ? "[LIGHT SWITCH] MENYALA" : "[LIGHT SWITCH] PADAM", this);
    }

    private void PerbaruiIndikator()
    {
        Color warna = lampuRuangan.enabled ? warnaMenyala : warnaPadam;
        indikator.GetPropertyBlock(properti);
        properti.SetColor("_Color", warna);
        properti.SetColor("_BaseColor", warna);
        properti.SetColor("_EmissionColor", warna * 0.22f);
        indikator.SetPropertyBlock(properti);
    }
}
