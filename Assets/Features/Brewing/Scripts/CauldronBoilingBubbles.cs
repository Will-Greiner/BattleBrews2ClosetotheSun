using UnityEngine;

[RequireComponent(typeof(CauldronLiquidController))]
public class CauldronBoilingBubbles : MonoBehaviour
{
    [SerializeField] private Material bubbleMaterial;
    [Min(0f)] [SerializeField] private float glowIntensity = 4f;

    private CauldronLiquidController liquid;

    private void Awake()
    {
        liquid = GetComponent<CauldronLiquidController>();
    }

    private void Update()
    {
        if (liquid == null || bubbleMaterial == null)
            return;

        Color liquidColor = liquid.CurrentLiquidColor;
        Color glowColor = liquidColor * glowIntensity;
        glowColor.a = liquidColor.a;

        bubbleMaterial.SetColor("_Color", liquidColor);
        bubbleMaterial.SetColor("_GlowColor", glowColor);
    }
}
