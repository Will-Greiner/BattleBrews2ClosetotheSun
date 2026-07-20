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
        HashSet<IngredientData> displayedIngredients = new();
        builder.AppendLine(title);

        foreach (CauldronContribution contribution in cauldron.Contributions)
        {
            IngredientData ingredient = contribution != null ? contribution.SourceIngredient : null;

            if (ingredient == null || !displayedIngredients.Add(ingredient))
                continue;

            int quantity = CountIngredient(ingredient);
            builder.AppendLine(quantity > 1 ? $"{ingredient.IngredientName}  x{quantity}" : ingredient.IngredientName);
        }

        contentsText.text = builder.ToString().TrimEnd();
    }

    private int CountIngredient(IngredientData ingredient)
    {
        int count = 0;

        foreach (CauldronContribution contribution in cauldron.Contributions)
        {
            if (contribution != null && contribution.SourceIngredient == ingredient)
                count++;
        }

        return count;
    }
}