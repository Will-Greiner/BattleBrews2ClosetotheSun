using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CauldronGhostDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CauldronController cauldron;
    [SerializeField] private Transform orbitCenter;
    [SerializeField] private Transform ghostContainer;
    [SerializeField] private Material ghostMaterial;
    [SerializeField] private TMP_Text countLabelPrefab;

    [Header("Layout")]
    [SerializeField, Min(0f)] private float orbitRadius = 0.55f;
    [SerializeField] private float heightAboveCenter = 0.25f;
    [SerializeField, Min(0.01f)] private float ghostScale = 0.35f;
    [SerializeField] private Vector3 countLabelOffset = new(0.18f, 0.18f, 0f);

    [Header("Motion")]
    [SerializeField] private float orbitSpeed = 25f;
    [SerializeField, Min(0f)] private float bobHeight = 0.07f;
    [SerializeField, Min(0f)] private float bobSpeed = 2f;
    [SerializeField] private bool faceCamera = true;

    private readonly List<GhostEntry> ghosts = new();
    private readonly Dictionary<Material, Material> runtimeMaterials = new();

    private sealed class GhostEntry
    {
        public Transform Root;
        public TMP_Text CountLabel;
        public float AngleOffset;
        public float BobOffset;
    }

    private void Awake()
    {
        if (cauldron == null)
            cauldron = GetComponentInParent<CauldronController>();

        if (orbitCenter == null)
            orbitCenter = transform;

        if (ghostContainer == null)
            ghostContainer = transform;
    }

    private void OnEnable()
    {
        if (cauldron != null)
            cauldron.ContributionsChanged += Rebuild;

        Rebuild();
    }

    private void OnDisable()
    {
        if (cauldron != null)
            cauldron.ContributionsChanged -= Rebuild;
    }

    private void OnDestroy()
    {
        foreach (Material material in runtimeMaterials.Values)
            Destroy(material);

        runtimeMaterials.Clear();
    }

    private void LateUpdate()
    {
        int count = ghosts.Count;

        if (count == 0 || orbitCenter == null)
            return;

        Camera cameraToFace = Camera.main;
        float currentOrbit = Time.time * orbitSpeed;

        for (int i = 0; i < count; i++)
        {
            GhostEntry ghost = ghosts[i];
            float angle = (currentOrbit + ghost.AngleOffset) * Mathf.Deg2Rad;
            float bob = Mathf.Sin(Time.time * bobSpeed + ghost.BobOffset) * bobHeight;
            Vector3 localOffset = new(
                Mathf.Cos(angle) * orbitRadius,
                heightAboveCenter + bob,
                Mathf.Sin(angle) * orbitRadius);

            ghost.Root.position = orbitCenter.TransformPoint(localOffset);
            ghost.Root.rotation = Quaternion.Euler(0f, -currentOrbit - ghost.AngleOffset + 90f, 0f);

            if (faceCamera && ghost.CountLabel != null && cameraToFace != null)
                ghost.CountLabel.transform.rotation = cameraToFace.transform.rotation;
        }
    }

    [ContextMenu("Refresh Ghosts")]
    public void Rebuild()
    {
        ClearGhosts();

        if (cauldron == null || ghostContainer == null || ghostMaterial == null)
            return;

        Dictionary<IngredientData, int> quantities = new();
        List<IngredientData> ingredientOrder = new();

        foreach (CauldronContribution contribution in cauldron.Contributions)
        {
            IngredientData ingredient = contribution?.SourceIngredient;

            if (ingredient == null)
                continue;

            if (quantities.TryGetValue(ingredient, out int quantity))
                quantities[ingredient] = quantity + 1;
            else
            {
                quantities.Add(ingredient, 1);
                ingredientOrder.Add(ingredient);
            }
        }

        int typeCount = ingredientOrder.Count;

        for (int i = 0; i < typeCount; i++)
        {
            IngredientData ingredient = ingredientOrder[i];
            GhostEntry entry = CreateGhost(ingredient, quantities[ingredient]);

            if (entry == null)
                continue;

            entry.AngleOffset = 360f * i / Mathf.Max(1, typeCount);
            entry.BobOffset = i * 2.1f;
            ghosts.Add(entry);
        }
    }

    private GhostEntry CreateGhost(IngredientData ingredient, int quantity)
    {
        if (ingredient.Prefab == null)
        {
            Debug.LogWarning($"{name}: {ingredient.IngredientName} has no prefab for its cauldron ghost.", ingredient);
            return null;
        }

        GameObject rootObject = new($"{ingredient.IngredientName} Ghost x{quantity}");
        Transform root = rootObject.transform;
        root.SetParent(ghostContainer, false);
        root.localScale = Vector3.one * ghostScale;

        CopyVisualHierarchy(ingredient.Prefab.transform, root, true);

        TMP_Text label = null;

        if (countLabelPrefab != null)
        {
            label = Instantiate(countLabelPrefab, root);
            label.name = "Quantity";
            label.text = $"×{quantity}";
            label.transform.localPosition = countLabelOffset / ghostScale;
            label.gameObject.SetActive(true);
        }

        return new GhostEntry { Root = root, CountLabel = label };
    }

    private void CopyVisualHierarchy(Transform source, Transform destinationParent, bool isPrefabRoot)
    {
        Transform destination = destinationParent;

        if (!isPrefabRoot)
        {
            GameObject copy = new(source.name);
            destination = copy.transform;
            destination.SetParent(destinationParent, false);
            destination.localPosition = source.localPosition;
            destination.localRotation = source.localRotation;
            destination.localScale = source.localScale;
        }

        MeshFilter sourceFilter = source.GetComponent<MeshFilter>();
        MeshRenderer sourceRenderer = source.GetComponent<MeshRenderer>();

        if (sourceFilter != null && sourceRenderer != null)
        {
            destination.gameObject.AddComponent<MeshFilter>().sharedMesh = sourceFilter.sharedMesh;
            MeshRenderer renderer = destination.gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = CreateGhostMaterials(sourceRenderer.sharedMaterials);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        foreach (Transform child in source)
            CopyVisualHierarchy(child, destination, false);
    }

    private Material[] CreateGhostMaterials(Material[] sourceMaterials)
    {
        int materialCount = Mathf.Max(1, sourceMaterials.Length);
        Material[] materials = new Material[materialCount];

        for (int i = 0; i < materialCount; i++)
        {
            Material source = sourceMaterials.Length > 0 ? sourceMaterials[i] : null;
            materials[i] = GetGhostMaterial(source);
        }

        return materials;
    }

    private Material GetGhostMaterial(Material source)
    {
        if (source == null)
            return ghostMaterial;

        if (runtimeMaterials.TryGetValue(source, out Material existing))
            return existing;

        Material material = new(ghostMaterial) { name = $"{source.name} Ghost (Runtime)" };
        Texture texture = null;

        if (source.HasProperty("_BaseMap"))
            texture = source.GetTexture("_BaseMap");
        else if (source.HasProperty("_MainTex"))
            texture = source.GetTexture("_MainTex");

        if (texture != null)
        {
            if (material.HasProperty("_BaseMap"))
                material.SetTexture("_BaseMap", texture);
            else if (material.HasProperty("_MainTex"))
                material.SetTexture("_MainTex", texture);
        }

        runtimeMaterials.Add(source, material);
        return material;
    }

    private void ClearGhosts()
    {
        foreach (GhostEntry ghost in ghosts)
        {
            if (ghost.Root != null)
                Destroy(ghost.Root.gameObject);
        }

        ghosts.Clear();
    }
}
