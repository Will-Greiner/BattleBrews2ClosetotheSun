using System;
using UnityEngine;

public enum RecipeRequirementType
{
    Ingredient,
    Property
}

[Serializable]
public class RecipeRequirement
{
    [SerializeField] private RecipeRequirementType requirementType;
    [SerializeField] private IngredientData ingredient;
    [SerializeField] private ItemPropertyData property;
    [Min(1)] [SerializeField] private int requiredCount = 1;

    public RecipeRequirementType RequirementType => requirementType;
    public IngredientData Ingredient => ingredient;
    public ItemPropertyData Property => property;
    public int RequiredCount => requiredCount;

    public string DisplayName
    {
        get
        {
            if (requirementType == RecipeRequirementType.Ingredient)
                return ingredient != null ? ingredient.IngredientName : "Missing Ingredient";

            return property != null ? property.DisplayName : "Missing Property";
        }
    }

    public Sprite Icon
    {
        get
        {
            if (requirementType == RecipeRequirementType.Ingredient)
                return ingredient != null ? ingredient.Icon : null;

            return property != null ? property.Icon : null;
        }
    }

    public bool IsValid()
    {
        if (requiredCount < 1)
            return false;

        if (requirementType == RecipeRequirementType.Ingredient)
            return ingredient != null;

        return property != null;
    }

    public bool MatchesIngredientIdentity(IngredientData candidate)
    {
        return requirementType == RecipeRequirementType.Ingredient && ingredient != null && ingredient == candidate;
    }

    public bool MatchesProperty(ItemPropertyData candidate)
    {
        return requirementType == RecipeRequirementType.Property && property != null && property == candidate;
    }
}