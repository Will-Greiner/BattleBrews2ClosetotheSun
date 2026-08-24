using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IngredientDatabase", menuName = "Scriptable Objects/IngredientDatabase")]
public class IngredientDatabase : ScriptableObject
{
    [SerializeField] private List<IngredientData> ingredients = new();

    public IReadOnlyList<IngredientData> Ingredients => ingredients;

    private void OnValidate()
    {
        RemoveInvalidEntries();
    }

    private void RemoveInvalidEntries()
    {
        for (int i = ingredients.Count - 1; i >= 0; i--)
        {
            if (ingredients[i] == null || ingredients.IndexOf(ingredients[i]) != i)
                ingredients.RemoveAt(i);
        }
    }
}
