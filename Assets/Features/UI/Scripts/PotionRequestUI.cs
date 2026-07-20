using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PotionRequestUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text encounterNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image potionIcon;

    private void Awake()
    {
        Hide();
    }

    public void ShowRequest(EncounterData encounter, PotionData requestedPotion)
    {
        if (encounter == null || requestedPotion == null)
        {
            Hide();
            return;
        }

        encounterNameText.text = encounter.EncounterName;
        dialogueText.text = encounter.BuildRequestDialogue(requestedPotion);
        potionIcon.sprite = requestedPotion.Icon;
        potionIcon.enabled = requestedPotion.Icon != null;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void Hide()
    {
        if (encounterNameText != null)
            encounterNameText.text = string.Empty;

        if (dialogueText != null)
            dialogueText.text = string.Empty;

        if (potionIcon != null)
        {
            potionIcon.sprite = null;
            potionIcon.enabled = false;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}