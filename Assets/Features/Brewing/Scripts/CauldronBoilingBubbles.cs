using UnityEngine;

[RequireComponent(typeof(CauldronLiquidController))]
public class CauldronBoilingBubbles : MonoBehaviour
{
    [SerializeField] private Material bubbleMaterial;

    private CauldronLiquidController liquid;
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private void Awake()
    {
        liquid = GetComponent<CauldronLiquidController>();
    }

    private void Update()
    {
        if (liquid == null || bubbleMaterial == null)
            return;

        Color liquidColor = liquid.CurrentLiquidColor;
        bubbleMaterial.SetColor(ColorId, liquidColor);
    }
}
