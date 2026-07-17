using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class CauldronContributionDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CauldronController cauldron;
    [SerializeField] private GameObject displayRoot;
    [SerializeField] private TMP_Text contentsText;
    [SerializeField] private Camera playerCamera;

    [Header("Display")]
    [SerializeField] private string title = "CAULDRON";
    [SerializeField] private string emptyText = "Empty";
    [SerializeField] private bool hideWhenEmpty;
    [SerializeField] private bool faceCamera = true;

    private void OnEnable()
    {
        if (cauldron != null)
            cauldron.ContributionsChanged += RefreshDisplay;

        RefreshDisplay();
    }

    private void OnDisable()
    {
        if (cauldron != null)
            cauldron.ContributionsChanged -= RefreshDisplay;
    }

    private void LateUpdate()
    {
        if (!faceCamera || displayRoot == null || playerCamera == null)
            return;

        displayRoot.transform.rotation = Quaternion.LookRotation(displayRoot.transform.position - playerCamera.transform.position, playerCamera.transform.up);
    }

    public void RefreshDisplay()
    {
        if (cauldron == null || contentsText == null)
            return;

        bool isEmpty = cauldron.Contributions.Count == 0;

        if (displayRoot != null)
            displayRoot.SetActive(!hideWhenEmpty || !isEmpty);

        if (isEmpty)
        {
            contentsText.text = $"{title}\n{emptyText}";
            return;
        }

        StringBuilder builder = new();
        HashSet<ItemPropertyData> displayedProperties = new();
        HashSet<IngredientData> displayedIngredients = new();

        builder.AppendLine(title);

        foreach (CauldronContribution contribution in cauldron.Contributions)
        {
            if (contribution == null)
                continue;

            if (contribution.Type == ContributionType.Property)
            {
                ItemPropertyData property = contribution.SelectedProperty;

                if (property == null || displayedProperties.Contains(property))
                    continue;

                displayedProperties.Add(property);
                int quantity = CountProperty(property);
                AppendContribution(builder, property.DisplayName, quantity);
            }
            else
            {
                IngredientData ingredient = contribution.SourceIngredient;

                if (ingredient == null || displayedIngredients.Contains(ingredient))
                    continue;

                displayedIngredients.Add(ingredient);
                int quantity = CountIngredient(ingredient);
                AppendContribution(builder, ingredient.IngredientName, quantity);
            }
        }

        contentsText.text = builder.ToString().TrimEnd();
    }

    private int CountProperty(ItemPropertyData property)
    {
        int count = 0;

        foreach (CauldronContribution contribution in cauldron.Contributions)
        {
            if (contribution != null && contribution.MatchesProperty(property))
                count++;
        }

        return count;
    }

    private int CountIngredient(IngredientData ingredient)
    {
        int count = 0;

        foreach (CauldronContribution contribution in cauldron.Contributions)
        {
            if (contribution != null && contribution.MatchesIngredient(ingredient))
                count++;
        }

        return count;
    }

    private void AppendContribution(StringBuilder builder, string contributionName, int quantity)
    {
        if (quantity > 1)
            builder.AppendLine($"{contributionName}  x{quantity}");
        else
            builder.AppendLine(contributionName);
    }
}