using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoundObjectiveHUD : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text encounterText;
    [SerializeField] private TMP_Text opponentText;
    [SerializeField] private TMP_Text potionText;
    [SerializeField] private Image potionIcon;

    private void Awake()
    {
        Hide();
    }

    public void Show(EncounterData encounter, PotionData potion)
    {
        if (encounter == null || potion == null)
        {
            Hide();
            return;
        }

        if (encounterText != null)
            encounterText.text = encounter.EncounterName;

        if (opponentText != null)
            opponentText.text = encounter.OpponentName;

        if (potionText != null)
            potionText.text = $"Brew a {potion.PotionName}";

        if (potionIcon != null)
        {
            potionIcon.sprite = potion.Icon;
            potionIcon.enabled = potion.Icon != null;
        }

        SetVisible(true);
    }

    public void Hide()
    {
        if (encounterText != null)
            encounterText.text = string.Empty;

        if (opponentText != null)
            opponentText.text = string.Empty;

        if (potionText != null)
            potionText.text = string.Empty;

        if (potionIcon != null)
        {
            potionIcon.sprite = null;
            potionIcon.enabled = false;
        }

        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}