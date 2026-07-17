using System.Collections.Generic;

public static class ContributionRecipeMatcher
{
    public static PotionData FindExactMatch(PotionDatabase database, IReadOnlyList<CauldronContribution> contributions)
    {
        if (database == null || contributions == null)
            return null;

        foreach (PotionData potion in database.Potions)
        {
            if (IsExactMatch(potion, contributions))
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
            if (IsOverfilledMatch(potion, contributions))
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
            if (CouldStillMatch(potion, contributions))
                return true;
        }

        return false;
    }

    public static bool IsExactMatch(PotionData potion, IReadOnlyList<CauldronContribution> contributions)
    {
        if (potion == null || contributions == null)
            return false;

        if (contributions.Count != potion.RequiredTotalIngredients)
            return false;

        return MeetsAllRequirements(potion, contributions);
    }

    public static bool IsOverfilledMatch(PotionData potion, IReadOnlyList<CauldronContribution> contributions)
    {
        if (potion == null || contributions == null)
            return false;

        if (contributions.Count <= potion.RequiredTotalIngredients)
            return false;

        if (!ContainsOnlyCompatibleContributions(potion, contributions))
            return false;

        return MeetsAllRequirements(potion, contributions);
    }

    public static bool CouldStillMatch(PotionData potion, IReadOnlyList<CauldronContribution> contributions)
    {
        if (potion == null || contributions == null)
            return false;

        if (contributions.Count >= potion.RequiredTotalIngredients)
            return false;

        return ContainsOnlyCompatibleContributions(potion, contributions);
    }

    private static bool MeetsAllRequirements(PotionData potion, IReadOnlyList<CauldronContribution> contributions)
    {
        foreach (PropertyRequirement requirement in potion.PropertyRequirements)
        {
            if (requirement == null || requirement.Property == null)
                return false;

            int matchingCount = 0;

            foreach (CauldronContribution contribution in contributions)
            {
                if (contribution != null && contribution.MatchesProperty(requirement.Property))
                    matchingCount++;
            }

            if (matchingCount < requirement.RequiredCount)
                return false;
        }

        foreach (SpecificIngredientRequirement requirement in potion.SpecificIngredientRequirements)
        {
            if (requirement == null || requirement.Ingredient == null)
                return false;

            int matchingCount = 0;

            foreach (CauldronContribution contribution in contributions)
            {
                if (contribution != null && contribution.MatchesIngredient(requirement.Ingredient))
                    matchingCount++;
            }

            if (matchingCount < requirement.RequiredCount)
                return false;
        }

        return true;
    }

    private static bool ContainsOnlyCompatibleContributions(PotionData potion, IReadOnlyList<CauldronContribution> contributions)
    {
        foreach (CauldronContribution contribution in contributions)
        {
            if (contribution == null || !IsContributionCompatible(contribution, potion))
                return false;
        }

        return true;
    }

    private static bool IsContributionCompatible(CauldronContribution contribution, PotionData potion)
    {
        if (contribution.Type == ContributionType.Property)
        {
            foreach (PropertyRequirement requirement in potion.PropertyRequirements)
            {
                if (requirement != null && requirement.Property == contribution.SelectedProperty)
                    return true;
            }
        }

        if (contribution.Type == ContributionType.Ingredient)
        {
            foreach (SpecificIngredientRequirement requirement in potion.SpecificIngredientRequirements)
            {
                if (requirement != null && requirement.Ingredient == contribution.SourceIngredient)
                    return true;
            }
        }

        return false;
    }
}