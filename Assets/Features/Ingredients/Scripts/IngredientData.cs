using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ingredient", menuName = "Scriptable Objects/Ingredient")]
public class IngredientData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string ingredientName;
    [SerializeField] private Sprite icon;
    [TextArea] [SerializeField] private string description;

    [Header("World Item")]
    [SerializeField] private GameObject prefab;

    [Header("Properties")]
    [SerializeField] private List<ItemPropertyData> properties = new();

    public string IngredientName => ingredientName;
    public Sprite Icon => icon;
    public string Description => description;
    public GameObject Prefab => prefab;
    public IReadOnlyList<ItemPropertyData> Properties => properties;

    public bool HasProperty(ItemPropertyData property)
    {
        return property != null && properties.Contains(property);
    }

    private void OnValidate()
    {
        RemoveDuplicateProperties();
    }

    private void RemoveDuplicateProperties()
    {
        for (int i = properties.Count - 1; i >= 0; i--)
        {
            if (properties[i] == null || properties.IndexOf(properties[i]) != i)
                properties.RemoveAt(i);
        }
    }
}