using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(CauldronLiquidController))]
public class CauldronBoilingBubbles : MonoBehaviour
{
    [Header("Boiling")]
    [Min(0f)] [SerializeField] private float idleBubblesPerSecond = 0.7f;
    [Min(0f)] [SerializeField] private float activeBubblesPerSecond = 8f;
    [Min(0.01f)] [SerializeField] private float minimumBubbleSize = 0.025f;
    [Min(0.01f)] [SerializeField] private float maximumBubbleSize = 0.075f;
    [Min(0.05f)] [SerializeField] private float minimumLifetime = 0.45f;
    [Min(0.05f)] [SerializeField] private float maximumLifetime = 0.9f;
    [Range(0.1f, 1f)] [SerializeField] private float surfaceRadius = 0.78f;

    [Header("Appearance")]
    [SerializeField] private Color bubbleColor = new(0.55f, 0.9f, 0.35f, 0.32f);
    [Range(0f, 1f)] [SerializeField] private float smoothness = 0.95f;
    [Min(0f)] [SerializeField] private float riseDistance = 0.018f;

    private sealed class Bubble
    {
        public Transform Transform;
        public float Age;
        public float Lifetime;
        public float Size;
        public Vector3 StartPosition;
    }

    private readonly List<Bubble> activeBubbles = new();
    private readonly Stack<Bubble> pooledBubbles = new();
    private CauldronLiquidController liquid;
    private Renderer liquidRenderer;
    private Material bubbleMaterial;
    private float emissionAccumulator;

    private void Awake()
    {
        liquid = GetComponent<CauldronLiquidController>();
        liquidRenderer = FindLiquidRenderer();
        bubbleMaterial = CreateBubbleMaterial();
    }

    private void OnDestroy()
    {
        if (bubbleMaterial != null)
            Destroy(bubbleMaterial);
    }

    private void Update()
    {
        UpdateBubbleColor();
        UpdateBubbles();

        if (liquidRenderer == null || liquid == null || liquid.DrainAmount > 0.05f)
            return;

        float rate = Mathf.Lerp(idleBubblesPerSecond, activeBubblesPerSecond, liquid.VisualActivity);
        emissionAccumulator += rate * Time.deltaTime;

        while (emissionAccumulator >= 1f)
        {
            emissionAccumulator -= 1f;
            SpawnBubble();
        }
    }

    private void UpdateBubbleColor()
    {
        if (bubbleMaterial == null || liquid == null)
            return;

        Color tintedColor = Color.Lerp(bubbleColor, liquid.CurrentMixtureColor, liquid.MixtureAmount);
        tintedColor.a = bubbleColor.a;
        bubbleMaterial.SetColor("_BaseColor", tintedColor);
    }

    private Renderer FindLiquidRenderer()
    {
        foreach (Renderer candidate in GetComponentsInChildren<Renderer>(true))
        {
            if (candidate.sharedMaterial != null && candidate.sharedMaterial.HasProperty("_Drain"))
                return candidate;
        }

        return null;
    }

    private Material CreateBubbleMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
            return null;

        Material material = new(shader)
        {
            name = "Cauldron Bubble (Runtime)",
            renderQueue = (int)RenderQueue.Transparent
        };
        material.SetColor("_BaseColor", bubbleColor);
        material.SetFloat("_Smoothness", smoothness);
        material.SetFloat("_Metallic", 0f);
        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", 0f);
        material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_ZWrite", 0f);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.SetShaderPassEnabled("ShadowCaster", false);
        return material;
    }

    private void SpawnBubble()
    {
        Bounds bounds = liquidRenderer.bounds;
        Vector2 point = Random.insideUnitCircle * surfaceRadius;
        float radiusX = bounds.extents.x;
        float radiusZ = bounds.extents.z;
        Vector3 position = new(
            bounds.center.x + point.x * radiusX,
            bounds.max.y,
            bounds.center.z + point.y * radiusZ);

        Bubble bubble = pooledBubbles.Count > 0 ? pooledBubbles.Pop() : CreateBubble();
        bubble.Age = 0f;
        bubble.Lifetime = Random.Range(minimumLifetime, maximumLifetime);
        bubble.Size = Random.Range(minimumBubbleSize, maximumBubbleSize)
            * Mathf.Lerp(0.8f, 1.35f, liquid.VisualActivity);
        bubble.StartPosition = position;
        bubble.Transform.position = position;
        bubble.Transform.localScale = Vector3.zero;
        bubble.Transform.gameObject.SetActive(true);
        activeBubbles.Add(bubble);
    }

    private Bubble CreateBubble()
    {
        GameObject bubbleObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bubbleObject.name = "Boiling Bubble";
        bubbleObject.transform.SetParent(transform, true);

        Collider collider = bubbleObject.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);

        MeshRenderer renderer = bubbleObject.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = bubbleMaterial;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        return new Bubble { Transform = bubbleObject.transform };
    }

    private void UpdateBubbles()
    {
        for (int i = activeBubbles.Count - 1; i >= 0; i--)
        {
            Bubble bubble = activeBubbles[i];
            bubble.Age += Time.deltaTime;
            float t = Mathf.Clamp01(bubble.Age / bubble.Lifetime);

            // A quick emergence, a rounded dome, then a sharp collapse reads as a surface pop.
            float scale = t < 0.72f
                ? Mathf.Sin((t / 0.72f) * Mathf.PI * 0.5f)
                : 1f - Mathf.SmoothStep(0f, 1f, (t - 0.72f) / 0.28f);
            float horizontalSize = bubble.Size * scale;
            float verticalSize = horizontalSize * Mathf.Lerp(0.45f, 0.8f, t);
            bubble.Transform.localScale = new Vector3(horizontalSize, verticalSize, horizontalSize);
            bubble.Transform.position = bubble.StartPosition + Vector3.up * (riseDistance * t);

            if (t < 1f)
                continue;

            bubble.Transform.gameObject.SetActive(false);
            activeBubbles.RemoveAt(i);
            pooledBubbles.Push(bubble);
        }
    }
}
