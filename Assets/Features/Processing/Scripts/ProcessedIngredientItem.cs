using UnityEngine;

[RequireComponent(typeof(GrabbableItem))]
public class ProcessedIngredientItem : MonoBehaviour
{
    private IngredientData sourceIngredient;
    private int propertyLevel;

    public IngredientData SourceIngredient => sourceIngredient;
    public int PropertyLevel => propertyLevel;
    public ItemPropertyData Property => sourceIngredient != null ? sourceIngredient.GetPropertyAtLevel(propertyLevel) : null;
    public bool IsInitialized => sourceIngredient != null && Property != null;

    public void Initialize(IngredientData ingredient, int level)
    {
        sourceIngredient = ingredient;
        propertyLevel = level;

        if (!IsInitialized)
            Debug.LogError($"{name}: Processed ingredient was initialized with invalid data.", this);
    }
}