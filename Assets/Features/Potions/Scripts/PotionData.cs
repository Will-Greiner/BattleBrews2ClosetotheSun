using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Potion", menuName = "Scriptable Objects/Potion")]
public class PotionData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string potionName;
    [SerializeField] private Sprite icon;
    [TextArea] [SerializeField] private string description;

    [Header("World Item")]
    [SerializeField] private GameObject prefab;

    [Header("Recipe")]
    [Tooltip("Requirements are evaluated from top to bottom. An ingredient satisfies the first incomplete requirement it can match.")]
    [SerializeField] private List<RecipeRequirement> requirements = new();

    [Header("Request Availability")]
    [Min(1)] [SerializeField] private int firstRequestRound = 1;
    [Min(1)] [SerializeField] private int lastRequestRound = 99;

    [Header("Discovery")]
    [SerializeField] private bool isDiscovered;

    public string PotionName => potionName;
    public Sprite Icon => icon;
    public string Description => description;
    public GameObject Prefab => prefab;
    public IReadOnlyList<RecipeRequirement> Requirements => requirements;
    public int RequiredTotalIngredients => CalculateRequiredTotalIngredients();
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

    public bool HasValidRecipe()
    {
        if (requirements == null || requirements.Count == 0)
            return false;

        foreach (RecipeRequirement requirement in requirements)
        {
            if (requirement == null || !requirement.IsValid())
                return false;
        }

        return true;
    }

    private int CalculateRequiredTotalIngredients()
    {
        int total = 0;

        if (requirements == null)
            return total;

        foreach (RecipeRequirement requirement in requirements)
        {
            if (requirement != null)
                total += Mathf.Max(1, requirement.RequiredCount);
        }

        return total;
    }

    private void OnValidate()
    {
        firstRequestRound = Mathf.Max(1, firstRequestRound);
        lastRequestRound = Mathf.Max(firstRequestRound, lastRequestRound);
        RemoveNullRequirements();
    }

    private void RemoveNullRequirements()
    {
        if (requirements == null)
            requirements = new List<RecipeRequirement>();

        for (int i = requirements.Count - 1; i >= 0; i--)
        {
            if (requirements[i] == null)
                requirements.RemoveAt(i);
        }
    }
}