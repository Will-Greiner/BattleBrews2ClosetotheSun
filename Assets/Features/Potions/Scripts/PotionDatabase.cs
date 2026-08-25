using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PotionDatabase", menuName = "Scriptable Objects/PotionDatabase")]
public class PotionDatabase : ScriptableObject
{
    [SerializeField] private List<PotionData> potions = new();

    public IReadOnlyList<PotionData> Potions => potions;

    public PotionData GetRandomAvailablePotion(int round)
    {
        return GetRandomAvailablePotion(round, null);
    }

    public PotionData GetRandomAvailablePotion(int round, Func<PotionData, bool> additionalFilter)
    {
        int availableCount = 0;

        foreach (PotionData potion in potions)
        {
            if (potion != null && potion.IsAvailableForRequest(round) && (additionalFilter == null || additionalFilter(potion)))
                availableCount++;
        }

        if (availableCount == 0)
            return null;

        int selectedIndex = UnityEngine.Random.Range(0, availableCount);

        foreach (PotionData potion in potions)
        {
            if (potion == null || !potion.IsAvailableForRequest(round) || (additionalFilter != null && !additionalFilter(potion)))
                continue;

            if (selectedIndex == 0)
                return potion;

            selectedIndex--;
        }

        return null;
    }

    private void OnValidate()
    {
        RemoveInvalidEntries();
    }

    private void RemoveInvalidEntries()
    {
        for (int i = potions.Count - 1; i >= 0; i--)
        {
            if (potions[i] == null || potions.IndexOf(potions[i]) != i)
                potions.RemoveAt(i);
        }
    }
}
