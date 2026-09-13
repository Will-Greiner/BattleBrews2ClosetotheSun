using System.Collections.Generic;
using UnityEngine;

public enum PotionRequestAvailabilityMode
{
    CurrentlyCraftableOnly,
    AllConfiguredPotions
}

public class ContentAvailabilityService : MonoBehaviour
{
    private const int MaximumCauldronIngredientTypes = 3;
    public static ContentAvailabilityService Instance { get; private set; }

    [SerializeField] private PotionRequestAvailabilityMode potionRequestMode = PotionRequestAvailabilityMode.CurrentlyCraftableOnly;

    public PotionRequestAvailabilityMode PotionRequestMode => potionRequestMode;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool IsContentUnlocked(string unlockId)
    {
        return string.IsNullOrWhiteSpace(unlockId) || (ProgressionManager.Instance != null && ProgressionManager.Instance.IsContentUnlocked(unlockId));
    }

    public bool IsIngredientUnlocked(IngredientData ingredient)
    {
        return ingredient != null && IsContentUnlocked(ingredient.UnlockId);
    }

    public bool IsPropertyDiscovered(IngredientData ingredient, int level)
    {
        return ProgressionManager.Instance != null && ProgressionManager.Instance.IsPropertyDiscovered(ingredient, level);
    }

    public bool CanRequestPotion(PotionData potion, int round)
    {
        if (potion == null || !potion.IsAvailableForRequest(round))
            return false;

        return potionRequestMode == PotionRequestAvailabilityMode.AllConfiguredPotions || CanCurrentlyCraftPotion(potion);
    }

    public bool CanCurrentlyCraftPotion(PotionData potion)
    {
        if (potion == null || !potion.HasValidRecipe())
            return false;

        HashSet<IngredientData> requiredIngredientTypes = new();
        List<ItemPropertyData> propertyRequirements = new();

        foreach (RecipeRequirement requirement in potion.Requirements)
        {
            if (requirement == null)
                return false;

            if (requirement.RequirementType == RecipeRequirementType.Ingredient)
            {
                if (!IsIngredientUnlocked(requirement.Ingredient))
                    return false;

                requiredIngredientTypes.Add(requirement.Ingredient);
                continue;
            }

            propertyRequirements.Add(requirement.Property);
        }

        if (requiredIngredientTypes.Count > MaximumCauldronIngredientTypes)
            return false;

        IngredientDatabase ingredientDatabase = GameContentCatalog.Instance != null ? GameContentCatalog.Instance.IngredientDatabase : null;

        if (propertyRequirements.Count > 0 && ingredientDatabase == null)
            return false;

        return CanAssignPropertyIngredients(propertyRequirements, 0, requiredIngredientTypes, ingredientDatabase);
    }

    public bool CanEventuallyCraftPotion(PotionData potion)
    {
        return CanCurrentlyCraftPotion(potion);
    }

    private bool CanAssignPropertyIngredients(IReadOnlyList<ItemPropertyData> properties, int propertyIndex, HashSet<IngredientData> selectedTypes, IngredientDatabase ingredientDatabase)
    {
        if (propertyIndex >= properties.Count)
            return true;

        ItemPropertyData property = properties[propertyIndex];

        if (property == null || ingredientDatabase == null)
            return false;

        foreach (IngredientData ingredient in ingredientDatabase.Ingredients)
        {
            int level = ingredient != null ? ingredient.GetPropertyLevel(property) : 0;

            if (level <= 0 || !IsIngredientUnlocked(ingredient) || !IsPropertyDiscovered(ingredient, level))
                continue;

            bool alreadySelected = selectedTypes.Contains(ingredient);

            if (!alreadySelected && selectedTypes.Count >= MaximumCauldronIngredientTypes)
                continue;

            if (!alreadySelected)
                selectedTypes.Add(ingredient);

            if (CanAssignPropertyIngredients(properties, propertyIndex + 1, selectedTypes, ingredientDatabase))
                return true;

            if (!alreadySelected)
                selectedTypes.Remove(ingredient);
        }

        return false;
    }
}
