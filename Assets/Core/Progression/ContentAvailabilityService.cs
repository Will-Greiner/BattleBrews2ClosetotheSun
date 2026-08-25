using UnityEngine;

public enum PotionRequestAvailabilityMode
{
    AllConfiguredPotions,
    CurrentlyCraftableOnly
}

public class ContentAvailabilityService : MonoBehaviour
{
    public static ContentAvailabilityService Instance { get; private set; }

    [SerializeField] private PotionRequestAvailabilityMode potionRequestMode = PotionRequestAvailabilityMode.AllConfiguredPotions;

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

        return potionRequestMode == PotionRequestAvailabilityMode.AllConfiguredPotions || CanEventuallyCraftPotion(potion);
    }

    public bool CanEventuallyCraftPotion(PotionData potion)
    {
        if (potion == null || !potion.HasValidRecipe())
            return false;

        foreach (RecipeRequirement requirement in potion.Requirements)
        {
            if (requirement == null)
                return false;

            if (requirement.RequirementType == RecipeRequirementType.Ingredient)
            {
                if (!IsIngredientUnlocked(requirement.Ingredient))
                    return false;

                continue;
            }

            if (!CanAccessProperty(requirement.Property))
                return false;
        }

        return true;
    }

    private bool CanAccessProperty(ItemPropertyData property)
    {
        if (property == null)
            return false;

        IngredientDatabase ingredientDatabase = GameContentCatalog.Instance != null ? GameContentCatalog.Instance.IngredientDatabase : null;

        if (ingredientDatabase == null)
            return true;

        foreach (IngredientData ingredient in ingredientDatabase.Ingredients)
        {
            int level = ingredient != null ? ingredient.GetPropertyLevel(property) : 0;

            if (level > 0 && IsIngredientUnlocked(ingredient) && IsContentUnlocked($"processing.level.{level}"))
                return true;
        }

        return false;
    }
}
