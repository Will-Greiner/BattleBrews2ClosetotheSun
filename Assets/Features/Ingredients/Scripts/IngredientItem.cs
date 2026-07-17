using UnityEngine;

[RequireComponent(typeof(GrabbableItem))]
public class IngredientItem : MonoBehaviour
{
    [SerializeField] private IngredientData ingredientData;

    public IngredientData Data => ingredientData;

    private void Awake()
    {
        if (ingredientData == null)
            Debug.LogError($"{name} does not have Ingredient Data assigned.", this);
    }
}