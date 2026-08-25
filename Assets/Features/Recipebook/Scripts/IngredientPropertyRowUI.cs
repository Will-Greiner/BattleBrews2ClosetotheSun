using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientPropertyRowUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image propertyIcon;
    [SerializeField] private TMP_Text propertyNameText;
    [SerializeField] private TMP_Text stationText;

    [Header("Unknown")]
    [SerializeField] private Sprite unknownIcon;
    [SerializeField] private string unknownPropertyText = "???";

    public void Display(IngredientData ingredient, int propertyLevel, bool discovered)
    {
        if (ingredient == null)
        {
            gameObject.SetActive(false);
            return;
        }

        ItemPropertyData property = ingredient.GetPropertyAtLevel(propertyLevel);

        if (property == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (propertyNameText != null)
            propertyNameText.text = discovered ? property.DisplayName : unknownPropertyText;

        if (propertyIcon != null)
        {
            propertyIcon.sprite = discovered ? property.Icon : unknownIcon;
            propertyIcon.enabled = propertyIcon.sprite != null;
        }

        if (stationText != null)
            stationText.text = GetStationName(propertyLevel);
    }

    private string GetStationName(int propertyLevel)
    {
        switch (propertyLevel)
        {
            case 1:
                return "Mortar & Pestle";

            case 2:
                return "Bunsen Burner";

            case 3:
                return "Pulverizer";

            default:
                return string.Empty;
        }
    }
}