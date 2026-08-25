using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientBookSpreadUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private IngredientDatabase ingredientDatabase;

    [Header("Illustration Page")]
    [SerializeField] private Image entryImage;

    [Header("Information Page")]
    [SerializeField] private TMP_Text ingredientNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private List<IngredientPropertyRowUI> propertyRows = new();

    [Header("Starting Entry")]
    [Min(0)] [SerializeField] private int startingIngredientIndex;

    private int currentIngredientIndex;

    public int CurrentIndex => currentIngredientIndex;
    public int EntryCount => ingredientDatabase != null && ingredientDatabase.Ingredients != null ? ingredientDatabase.Ingredients.Count : 0;
    public bool HasPrevious => currentIngredientIndex > 0;
    public bool HasNext => currentIngredientIndex < EntryCount - 1;
    public IngredientData CurrentIngredient => GetIngredient(currentIngredientIndex);

    private void Awake()
    {
        currentIngredientIndex = ClampIngredientIndex(startingIngredientIndex);
    }

    private void OnEnable()
    {
        SubscribeToDiscovery();
    }

    private void Start()
    {
        SubscribeToDiscovery();
    }

    private void OnDisable()
    {
        if (IngredientDiscoveryManager.Instance == null)
            return;

        IngredientDiscoveryManager.Instance.PropertyDiscovered -= HandlePropertyDiscovered;
        IngredientDiscoveryManager.Instance.IngredientDiscoveryReset -= HandleIngredientReset;
        IngredientDiscoveryManager.Instance.AllDiscoveryReset -= HandleAllDiscoveryReset;
    }

    public void Refresh()
    {
        currentIngredientIndex = ClampIngredientIndex(currentIngredientIndex);
        DisplayIngredient(CurrentIngredient);
    }

    public void DisplayPrevious()
    {
        if (!HasPrevious)
            return;

        DisplayIngredientAtIndex(currentIngredientIndex - 1);
    }

    public void DisplayNext()
    {
        if (!HasNext)
            return;

        DisplayIngredientAtIndex(currentIngredientIndex + 1);
    }

    public void DisplayIngredientAtIndex(int index)
    {
        currentIngredientIndex = ClampIngredientIndex(index);
        DisplayIngredient(CurrentIngredient);
    }

    private void DisplayIngredient(IngredientData ingredient)
    {
        if (ingredient == null)
        {
            ClearDisplay();
            return;
        }

        if (entryImage != null)
        {
            entryImage.sprite = ingredient.Icon;
            entryImage.enabled = ingredient.Icon != null;
        }

        if (ingredientNameText != null)
            ingredientNameText.text = ingredient.IngredientName;

        if (descriptionText != null)
            descriptionText.text = ingredient.Description;

        IngredientDiscoveryManager discoveryManager = IngredientDiscoveryManager.Instance;

        for (int i = 0; i < propertyRows.Count; i++)
        {
            IngredientPropertyRowUI row = propertyRows[i];

            if (row == null)
                continue;

            int propertyLevel = i + 1;
            bool discovered = discoveryManager != null && discoveryManager.IsPropertyDiscovered(ingredient, propertyLevel);
            row.Display(ingredient, propertyLevel, discovered);
        }
    }

    private IngredientData GetIngredient(int index)
    {
        if (ingredientDatabase == null || ingredientDatabase.Ingredients == null || ingredientDatabase.Ingredients.Count == 0)
            return null;

        if (index < 0 || index >= ingredientDatabase.Ingredients.Count)
            return null;

        return ingredientDatabase.Ingredients[index];
    }

    private int ClampIngredientIndex(int index)
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

        if (ingredientNameText != null)
            ingredientNameText.text = string.Empty;

        if (descriptionText != null)
            descriptionText.text = string.Empty;

        foreach (IngredientPropertyRowUI row in propertyRows)
        {
            if (row != null)
                row.gameObject.SetActive(false);
        }
    }

    private void SubscribeToDiscovery()
    {
        if (IngredientDiscoveryManager.Instance == null)
            return;

        IngredientDiscoveryManager.Instance.PropertyDiscovered -= HandlePropertyDiscovered;
        IngredientDiscoveryManager.Instance.IngredientDiscoveryReset -= HandleIngredientReset;
        IngredientDiscoveryManager.Instance.AllDiscoveryReset -= HandleAllDiscoveryReset;
        IngredientDiscoveryManager.Instance.PropertyDiscovered += HandlePropertyDiscovered;
        IngredientDiscoveryManager.Instance.IngredientDiscoveryReset += HandleIngredientReset;
        IngredientDiscoveryManager.Instance.AllDiscoveryReset += HandleAllDiscoveryReset;
    }

    private void HandlePropertyDiscovered(IngredientData ingredient, int level, ItemPropertyData property)
    {
        if (ingredient == CurrentIngredient)
            Refresh();
    }

    private void HandleIngredientReset(IngredientData ingredient)
    {
        if (ingredient == CurrentIngredient)
            Refresh();
    }

    private void HandleAllDiscoveryReset() => Refresh();
}
