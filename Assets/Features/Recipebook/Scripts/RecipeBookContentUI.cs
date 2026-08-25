using UnityEngine;
using UnityEngine.UI;

public enum RecipeBookSection
{
    Ingredients,
    Potions
}

public class RecipeBookContentUI : MonoBehaviour
{
    [Header("Content Roots")]
    [SerializeField] private GameObject ingredientContentRoot;
    [SerializeField] private GameObject potionContentRoot;

    [Header("Spread Controllers")]
    [SerializeField] private IngredientBookSpreadUI ingredientSpread;
    [SerializeField] private PotionBookSpreadUI potionSpread;

    [Header("Navigation")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;

    [Header("Tabs")]
    [SerializeField] private Button ingredientTabButton;
    [SerializeField] private Button potionTabButton;
    [SerializeField] private RecipeBookSection startingSection = RecipeBookSection.Ingredients;

    private RecipeBookSection currentSection;

    public RecipeBookSection CurrentSection => currentSection;

    private void Awake()
    {
        currentSection = startingSection;
    }

    public void OpenContent()
    {
        ShowSection(currentSection);
    }

    public void ShowIngredients()
    {
        ShowSection(RecipeBookSection.Ingredients);
    }

    public void ShowPotions()
    {
        ShowSection(RecipeBookSection.Potions);
    }

    public void ShowPreviousEntry()
    {
        if (currentSection == RecipeBookSection.Ingredients)
            ingredientSpread?.DisplayPrevious();
        else
            potionSpread?.DisplayPrevious();

        UpdateNavigationButtons();
    }

    public void ShowNextEntry()
    {
        if (currentSection == RecipeBookSection.Ingredients)
            ingredientSpread?.DisplayNext();
        else
            potionSpread?.DisplayNext();

        UpdateNavigationButtons();
    }

    public void RefreshCurrentEntry()
    {
        if (currentSection == RecipeBookSection.Ingredients)
            ingredientSpread?.Refresh();
        else
            potionSpread?.Refresh();

        UpdateNavigationButtons();
    }

    private void ShowSection(RecipeBookSection section)
    {
        currentSection = section;

        if (ingredientContentRoot != null)
            ingredientContentRoot.SetActive(currentSection == RecipeBookSection.Ingredients);

        if (potionContentRoot != null)
            potionContentRoot.SetActive(currentSection == RecipeBookSection.Potions);

        if (currentSection == RecipeBookSection.Ingredients)
            ingredientSpread?.Refresh();
        else
            potionSpread?.Refresh();

        if (ingredientTabButton != null)
            ingredientTabButton.interactable = currentSection != RecipeBookSection.Ingredients;

        if (potionTabButton != null)
            potionTabButton.interactable = currentSection != RecipeBookSection.Potions;

        UpdateNavigationButtons();
    }

    private void UpdateNavigationButtons()
    {
        bool hasPrevious = currentSection == RecipeBookSection.Ingredients ? ingredientSpread != null && ingredientSpread.HasPrevious : potionSpread != null && potionSpread.HasPrevious;
        bool hasNext = currentSection == RecipeBookSection.Ingredients ? ingredientSpread != null && ingredientSpread.HasNext : potionSpread != null && potionSpread.HasNext;

        if (previousButton != null)
            previousButton.interactable = hasPrevious;

        if (nextButton != null)
            nextButton.interactable = hasNext;
    }
}