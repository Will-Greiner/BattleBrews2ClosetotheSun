using System.Collections.Generic;
using UnityEngine;

public class ObjectHighlight : MonoBehaviour
{
    [Header("Highlight")]
    [SerializeField] private Material highlightMaterial;
    [ColorUsage(true, true)] [SerializeField] private Color highlightColor = Color.white;
    [SerializeField] private string colorPropertyName = "_HighlightColor";
    [SerializeField] private bool includeInactiveRenderers = true;

    private readonly Dictionary<Renderer, Material[]> originalMaterials = new();
    private MaterialPropertyBlock propertyBlock;
    private int colorPropertyID;
    private bool isHighlighted;

    public bool IsHighlighted => isHighlighted;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        colorPropertyID = Shader.PropertyToID(colorPropertyName);
        CacheRenderers();
    }

    public void Show()
    {
        if (isHighlighted || highlightMaterial == null)
            return;

        if (originalMaterials.Count == 0)
            CacheRenderers();

        foreach (KeyValuePair<Renderer, Material[]> entry in originalMaterials)
        {
            Renderer targetRenderer = entry.Key;

            if (targetRenderer == null)
                continue;

            Material[] original = entry.Value;
            Material[] highlighted = new Material[original.Length + 1];

            for (int i = 0; i < original.Length; i++)
                highlighted[i] = original[i];

            int highlightMaterialIndex = highlighted.Length - 1;
            highlighted[highlightMaterialIndex] = highlightMaterial;
            targetRenderer.sharedMaterials = highlighted;

            propertyBlock.Clear();
            propertyBlock.SetColor(colorPropertyID, highlightColor);
            targetRenderer.SetPropertyBlock(propertyBlock, highlightMaterialIndex);
        }

        isHighlighted = true;
    }

    public void Hide()
    {
        if (!isHighlighted)
            return;

        foreach (KeyValuePair<Renderer, Material[]> entry in originalMaterials)
        {
            if (entry.Key != null)
                entry.Key.sharedMaterials = entry.Value;
        }

        isHighlighted = false;
    }

    public void SetHighlightColor(Color color)
    {
        highlightColor = color;

        if (!isHighlighted)
            return;

        Hide();
        Show();
    }

    private void CacheRenderers()
    {
        originalMaterials.Clear();

        Renderer[] renderers = GetComponentsInChildren<Renderer>(includeInactiveRenderers);

        foreach (Renderer targetRenderer in renderers)
        {
            if (targetRenderer == null)
                continue;

            if (targetRenderer is not MeshRenderer && targetRenderer is not SkinnedMeshRenderer)
                continue;

            originalMaterials.Add(targetRenderer, targetRenderer.sharedMaterials);
        }
    }

    private void OnDisable()
    {
        Hide();
    }

    private void OnDestroy()
    {
        Hide();
    }
}