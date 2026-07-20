using System;
using UnityEngine;

public enum ContributionType
{
    Ingredient,
    Property
}

[Serializable]
public class CauldronContribution
{
    [SerializeField] private IngredientData sourceIngredient;

    public IngredientData SourceIngredient => sourceIngredient;
    public ContributionType Type => ContributionType.Ingredient;
    public ItemPropertyData SelectedProperty => null;
    public string DisplayName => sourceIngredient != null ? sourceIngredient.IngredientName : "Missing Ingredient";

    public CauldronContribution(IngredientData ingredient)
    {
        sourceIngredient = ingredient;
    }

    [Obsolete("The cauldron no longer accepts manually selected properties.")]
    public CauldronContribution(IngredientData ingredient, ItemPropertyData property)
    {
        sourceIngredient = ingredient;
    }

    public bool MatchesIngredient(IngredientData ingredient)
    {
        return sourceIngredient != null && sourceIngredient == ingredient;
    }

    [Obsolete("Property matching is now handled automatically by ContributionRecipeMatcher.")]
    public bool MatchesProperty(ItemPropertyData property)
    {
        return false;
    }
}