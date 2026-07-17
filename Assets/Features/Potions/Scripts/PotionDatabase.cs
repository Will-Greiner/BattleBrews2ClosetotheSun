using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PotionDatabase", menuName = "Scriptable Objects/PotionDatabase")]
public class PotionDatabase : ScriptableObject
{
    [SerializeField] private List<PotionData> potions = new();

    public IReadOnlyList<PotionData> Potions => potions;

    public PotionData GetRandomAvailablePotion(int round)
    {
        int availableCount = 0;

        foreach (PotionData potion in potions)
        {
            if (potion != null && potion.IsAvailableForRequest(round))
                availableCount++;
        }

        if (availableCount == 0)
            return null;

        int selectedIndex = Random.Range(0, availableCount);

        foreach (PotionData potion in potions)
        {
            if (potion == null || !potion.IsAvailableForRequest(round))
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