using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Potion", menuName = "Scriptable Objects/Potion")]
public class PotionData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string potionName;
    [SerializeField] private Sprite icon;
    [TextArea]
    [SerializeField] private string description;

    [Header("World Item")]
    [SerializeField] private GameObject prefab;

    [Header("Recipe")]
    [Min(1)]
    [SerializeField] private int requiredTotalIngredients = 3;
    [SerializeField] private List<PropertyRequirement> propertyRequirements = new();
    [SerializeField] private List<SpecificIngredientRequirement> specificIngredientRequirements = new();

    [Header("Request Availability")]
    [Min(1)]
    [SerializeField] private int firstRequestRound = 1;

    [Min(1)]
    [SerializeField] private int lastRequestRound = 99;

    [Header("Discovery")]
    [SerializeField] private bool isDiscovered;

    public string PotionName => potionName;
    public Sprite Icon => icon;
    public string Description => description;
    public GameObject Prefab => prefab;
    public int RequiredTotalIngredients => requiredTotalIngredients;
    public IReadOnlyList<PropertyRequirement> PropertyRequirements => propertyRequirements;
    public IReadOnlyList<SpecificIngredientRequirement> SpecificIngredientRequirements => specificIngredientRequirements;
    public int FirstRequestRound => firstRequestRound;
    public int LastRequestRound => lastRequestRound;
    public bool IsDiscovered => isDiscovered;

    public bool IsAvailableForRequest(int round)
    {
        return round >= firstRequestRound && round <= lastRequestRound;
    }

    public void SetDiscovered(bool discovered)
    {
        isDiscovered = discovered;
    }

    private void OnValidate()
    {
        firstRequestRound = Mathf.Max(1, firstRequestRound);
        lastRequestRound = Mathf.Max(firstRequestRound, lastRequestRound);
    }
}