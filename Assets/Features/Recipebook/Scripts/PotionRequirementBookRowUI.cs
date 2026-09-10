using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PotionRequirementBookRowUI : MonoBehaviour
{
    [SerializeField] private Image requirementIcon;
    [SerializeField] private TMP_Text requirementNameText;

    public void Display(RecipeRequirement requirement)
    {
        if (requirement == null || !requirement.IsValid())
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (requirementIcon != null)
            requirementIcon.gameObject.SetActive(false);

        if (requirementNameText != null)
            requirementNameText.text = $"{requirement.DisplayName}  x{requirement.RequiredCount}";
    }
}
