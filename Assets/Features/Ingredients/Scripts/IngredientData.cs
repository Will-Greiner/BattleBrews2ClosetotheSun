using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ingredient", menuName = "Scriptable Objects/Ingredient")]
public class IngredientData : ScriptableObject
{
    private const int MaximumPropertyLevels = 3;

    [Header("Identity")]
    [SerializeField] private string ingredientName;
    [SerializeField] private Sprite icon;
    [TextArea] [SerializeField] private string description;
    [HideInInspector] [SerializeField] private string persistentId;

    [Header("World Item")]
    [SerializeField] private GameObject prefab;

    [Header("Properties")]
    [Tooltip("Property order determines discovery level. Element 0 is Level 1, Element 1 is Level 2, and Element 2 is Level 3.")]
    [SerializeField] private List<ItemPropertyData> properties = new();

    public string IngredientName => ingredientName;
    public Sprite Icon => icon;
    public string Description => description;
    public string PersistentId => persistentId;
    public GameObject Prefab => prefab;
    public IReadOnlyList<ItemPropertyData> Properties => properties;
    public int PropertyCount => properties.Count;

    public bool HasProperty(ItemPropertyData property)
    {
        return property != null && properties.Contains(property);
    }

    public int GetPropertyLevel(ItemPropertyData property)
    {
        if (property == null)
            return 0;

        int index = properties.IndexOf(property);
        return index >= 0 ? index + 1 : 0;
    }

    public ItemPropertyData GetPropertyAtLevel(int level)
    {
        int index = level - 1;

        if (index < 0 || index >= properties.Count)
            return null;

        return properties[index];
    }

    private void OnValidate()
    {
        EnsurePersistentId();
        RemoveInvalidProperties();
    }

    private void EnsurePersistentId()
    {
        if (string.IsNullOrWhiteSpace(persistentId))
            persistentId = Guid.NewGuid().ToString("N");
    }

    private void RemoveInvalidProperties()
    {
        if (properties == null)
            properties = new List<ItemPropertyData>();

        for (int i = properties.Count - 1; i >= 0; i--)
        {
            if (properties[i] == null || properties.IndexOf(properties[i]) != i || i >= MaximumPropertyLevels)
                properties.RemoveAt(i);
        }
    }
}