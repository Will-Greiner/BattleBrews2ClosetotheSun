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
    [SerializeField] private ContributionType contributionType;
    [SerializeField] private ItemPropertyData selectedProperty;

    public IngredientData SourceIngredient => sourceIngredient;
    public ContributionType Type => contributionType;
    public ItemPropertyData SelectedProperty => selectedProperty;

    public string DisplayName
    {
        get
        {
            if (contributionType == ContributionType.Ingredient)
                return sourceIngredient != null ? sourceIngredient.IngredientName : "Missing Ingredient";

            return selectedProperty != null ? selectedProperty.DisplayName : "Missing Property";
        }
    }

    public CauldronContribution(IngredientData ingredient)
    {
        sourceIngredient = ingredient;
        contributionType = ContributionType.Ingredient;
        selectedProperty = null;
    }

    public CauldronContribution(IngredientData ingredient, ItemPropertyData property)
    {
        sourceIngredient = ingredient;
        contributionType = ContributionType.Property;
        selectedProperty = property;
    }

    public bool MatchesIngredient(IngredientData ingredient)
    {
        return contributionType == ContributionType.Ingredient && sourceIngredient == ingredient;
    }

    public bool MatchesProperty(ItemPropertyData property)
    {
        return contributionType == ContributionType.Property && selectedProperty == property;
    }
}