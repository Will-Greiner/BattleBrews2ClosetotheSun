using System.Collections.Generic;

public sealed class RecipeMatchResult
{
    private readonly int[] completedCounts;

    public PotionData Potion { get; }
    public int AssignedContributionCount { get; }
    public int CompatibleExtraCount { get; }
    public int IncompatibleContributionCount { get; }
    public bool AllRequirementsMet { get; }
    public bool IsExactMatch { get; }
    public bool IsOverfilledMatch { get; }

    public RecipeMatchResult(PotionData potion, int[] completedCounts, int assignedContributionCount, int compatibleExtraCount, int incompatibleContributionCount, bool allRequirementsMet, bool isExactMatch, bool isOverfilledMatch)
    {
        Potion = potion;
        this.completedCounts = completedCounts;
        AssignedContributionCount = assignedContributionCount;
        CompatibleExtraCount = compatibleExtraCount;
        IncompatibleContributionCount = incompatibleContributionCount;
        AllRequirementsMet = allRequirementsMet;
        IsExactMatch = isExactMatch;
        IsOverfilledMatch = isOverfilledMatch;
    }

    public int GetCompletedCount(int requirementIndex)
    {
        if (completedCounts == null || requirementIndex < 0 || requirementIndex >= completedCounts.Length)
            return 0;

        return completedCounts[requirementIndex];
    }
}

public static class ContributionRecipeMatcher
{
    public static PotionData FindExactMatch(PotionDatabase database, IReadOnlyList<CauldronContribution> contributions)
    {
        if (database == null || contributions == null)
            return null;

        foreach (PotionData potion in database.Potions)
        {
            if (Evaluate(potion, contributions).IsExactMatch)
                return potion;
        }

        return null;
    }

    public static PotionData FindOverfilledMatch(PotionDatabase database, IReadOnlyList<CauldronContribution> contributions)
    {
        if (database == null || contributions == null)
            return null;

        foreach (PotionData potion in database.Potions)
        {
            if (Evaluate(potion, contributions).IsOverfilledMatch)
                return potion;
        }

        return null;
    }

    public static bool CouldStillMatchAnyPotion(PotionDatabase database, IReadOnlyList<CauldronContribution> contributions)
    {
        if (database == null || contributions == null)
            return false;

        foreach (PotionData potion in database.Potions)
        {
            RecipeMatchResult result = Evaluate(potion, contributions);

            if (result.IncompatibleContributionCount == 0 && contributions.Count < potion.RequiredTotalIngredients)
                return true;
        }

        return false;
    }

    public static bool IsExactMatch(PotionData potion, IReadOnlyList<CauldronContribution> contributions)
    {
        return Evaluate(potion, contributions).IsExactMatch;
    }

    public static bool IsOverfilledMatch(PotionData potion, IReadOnlyList<CauldronContribution> contributions)
    {
        return Evaluate(potion, contributions).IsOverfilledMatch;
    }

    public static RecipeMatchResult Evaluate(PotionData potion, IReadOnlyList<CauldronContribution> contributions)
    {
        int requirementCount = potion != null ? potion.Requirements.Count : 0;
        int[] completedCounts = new int[requirementCount];

        if (potion == null || contributions == null || !potion.HasValidRecipe())
            return new RecipeMatchResult(potion, completedCounts, 0, 0, contributions != null ? contributions.Count : 0, false, false, false);

        int assignedCount = 0;
        int compatibleExtraCount = 0;
        int incompatibleCount = 0;

        foreach (CauldronContribution contribution in contributions)
        {
            IngredientData ingredient = contribution != null ? contribution.SourceIngredient : null;

            if (ingredient == null)
            {
                incompatibleCount++;
                continue;
            }

            int matchedRequirementIndex = FindFirstIncompleteMatchingRequirement(potion, ingredient, completedCounts);

            if (matchedRequirementIndex >= 0)
            {
                completedCounts[matchedRequirementIndex]++;
                assignedCount++;
                continue;
            }

            if (CanMatchAnyRequirement(potion, ingredient))
                compatibleExtraCount++;
            else
                incompatibleCount++;
        }

        bool allRequirementsMet = AreAllRequirementsMet(potion, completedCounts);
        bool exactMatch = allRequirementsMet && contributions.Count == potion.RequiredTotalIngredients && incompatibleCount == 0;
        bool overfilledMatch = allRequirementsMet && contributions.Count > potion.RequiredTotalIngredients && incompatibleCount == 0;

        return new RecipeMatchResult(potion, completedCounts, assignedCount, compatibleExtraCount, incompatibleCount, allRequirementsMet, exactMatch, overfilledMatch);
    }

    public static bool CanIngredientSatisfyRequirement(IngredientData ingredient, RecipeRequirement requirement)
    {
        if (ingredient == null || requirement == null || !requirement.IsValid())
            return false;

        if (requirement.RequirementType == RecipeRequirementType.Ingredient)
            return requirement.MatchesIngredientIdentity(ingredient);

        if (!ingredient.HasProperty(requirement.Property))
            return false;

        IngredientDiscoveryManager discoveryManager = IngredientDiscoveryManager.Instance;
        return discoveryManager != null && discoveryManager.IsPropertyDiscovered(ingredient, requirement.Property);
    }

    private static int FindFirstIncompleteMatchingRequirement(PotionData potion, IngredientData ingredient, int[] completedCounts)
    {
        for (int i = 0; i < potion.Requirements.Count; i++)
        {
            RecipeRequirement requirement = potion.Requirements[i];

            if (requirement == null || completedCounts[i] >= requirement.RequiredCount)
                continue;

            if (CanIngredientSatisfyRequirement(ingredient, requirement))
                return i;
        }

        return -1;
    }

    private static bool CanMatchAnyRequirement(PotionData potion, IngredientData ingredient)
    {
        foreach (RecipeRequirement requirement in potion.Requirements)
        {
            if (CanIngredientSatisfyRequirement(ingredient, requirement))
                return true;
        }

        return false;
    }

    private static bool AreAllRequirementsMet(PotionData potion, int[] completedCounts)
    {
        for (int i = 0; i < potion.Requirements.Count; i++)
        {
            RecipeRequirement requirement = potion.Requirements[i];

            if (requirement == null || completedCounts[i] < requirement.RequiredCount)
                return false;
        }

        return true;
    }
}