using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PotionRequirementRowUI : MonoBehaviour
{
    [SerializeField] private Image requirementIcon;
    [SerializeField] private TMP_Text requirementNameText;

    public void Display(RecipeRequirement requirement)
    {
        if (requirement == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (requirementIcon != null)
        {
            requirementIcon.sprite = requirement.Icon;
            requirementIcon.enabled = requirement.Icon != null;
        }

        if (requirementNameText != null)
            requirementNameText.text = requirement.DisplayName;
    }
}