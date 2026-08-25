using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PotionBookSpreadUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PotionDatabase potionDatabase;

    [Header("Illustration Page")]
    [SerializeField] private Image entryImage;

    [Header("Information Page")]
    [SerializeField] private TMP_Text potionNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Transform requirementContainer;
    [SerializeField] private PotionRequirementBookRowUI requirementRowPrefab;

    [Header("Starting Entry")]
    [Min(0)] [SerializeField] private int startingPotionIndex;

    private readonly List<PotionRequirementBookRowUI> spawnedRows = new();
    private int currentPotionIndex;

    public int CurrentIndex => currentPotionIndex;
    public int EntryCount => potionDatabase != null && potionDatabase.Potions != null ? potionDatabase.Potions.Count : 0;
    public bool HasPrevious => currentPotionIndex > 0;
    public bool HasNext => currentPotionIndex < EntryCount - 1;
    public PotionData CurrentPotion => GetPotion(currentPotionIndex);

    private void Awake()
    {
        currentPotionIndex = ClampPotionIndex(startingPotionIndex);
    }

    public void Refresh()
    {
        currentPotionIndex = ClampPotionIndex(currentPotionIndex);
        DisplayPotion(CurrentPotion);
    }

    public void DisplayPrevious()
    {
        if (!HasPrevious)
            return;

        DisplayPotionAtIndex(currentPotionIndex - 1);
    }

    public void DisplayNext()
    {
        if (!HasNext)
            return;

        DisplayPotionAtIndex(currentPotionIndex + 1);
    }

    public void DisplayPotionAtIndex(int index)
    {
        currentPotionIndex = ClampPotionIndex(index);
        DisplayPotion(CurrentPotion);
    }

    private void DisplayPotion(PotionData potion)
    {
        ClearRequirementRows();

        if (potion == null)
        {
            ClearDisplay();
            return;
        }

        if (entryImage != null)
        {
            entryImage.sprite = potion.Icon;
            entryImage.enabled = potion.Icon != null;
        }

        if (potionNameText != null)
            potionNameText.text = potion.PotionName;

        if (descriptionText != null)
            descriptionText.text = potion.Description;

        if (requirementContainer == null || requirementRowPrefab == null)
            return;

        foreach (RecipeRequirement requirement in potion.Requirements)
        {
            if (requirement == null)
                continue;

            for (int unitIndex = 0; unitIndex < requirement.RequiredCount; unitIndex++)
            {
                PotionRequirementBookRowUI row = Instantiate(requirementRowPrefab, requirementContainer);
                row.Display(requirement);
                spawnedRows.Add(row);
            }
        }
    }

    private PotionData GetPotion(int index)
    {
        if (potionDatabase == null || potionDatabase.Potions == null || potionDatabase.Potions.Count == 0)
            return null;

        if (index < 0 || index >= potionDatabase.Potions.Count)
            return null;

        return potionDatabase.Potions[index];
    }

    private int ClampPotionIndex(int index)
    {
        if (EntryCount == 0)
            return 0;

        return Mathf.Clamp(index, 0, EntryCount - 1);
    }

    private void ClearDisplay()
    {
        if (entryImage != null)
        {
            entryImage.sprite = null;
            entryImage.enabled = false;
        }

        if (potionNameText != null)
            potionNameText.text = string.Empty;

        if (descriptionText != null)
            descriptionText.text = string.Empty;

        ClearRequirementRows();
    }

    private void ClearRequirementRows()
    {
        foreach (PotionRequirementBookRowUI row in spawnedRows)
        {
            if (row != null)
                Destroy(row.gameObject);
        }

        spawnedRows.Clear();
    }
}