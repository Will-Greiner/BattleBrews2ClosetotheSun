using System;
using UnityEngine;

[Serializable]
public class PropertyRequirement
{
    [SerializeField] private ItemPropertyData property;
    [Min(1)] [SerializeField] private int requiredCount = 1;

    public ItemPropertyData Property => property;
    public int RequiredCount => requiredCount;
}

[Serializable]
public class SpecificIngredientRequirement
{
    [SerializeField] private IngredientData ingredient;
    [Min(1)] [SerializeField] private int requiredCount = 1;

    public IngredientData Ingredient => ingredient;
    public int RequiredCount => requiredCount;
}