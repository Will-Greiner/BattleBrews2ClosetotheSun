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

    private bool isSubscribed;

    private void Awake()
    {
        Hide();
    }

    private void OnEnable()
    {
        SubscribeToGameManager();
    }

    private void Start()
    {
        SubscribeToGameManager();

        if (GameManager.Instance != null && GameManager.Instance.State == GameState.RoundActive)
            ShowRequest(GameManager.Instance.CurrentEncounter, GameManager.Instance.RequestedPotion);
    }

    private void OnDisable()
    {
        UnsubscribeFromGameManager();
    }

    private void SubscribeToGameManager()
    {
        if (isSubscribed || GameManager.Instance == null)
            return;

        GameManager.Instance.RoundStarted += HandleRoundStarted;
        GameManager.Instance.RoundResolved += HandleRoundResolved;
        GameManager.Instance.GameEnded += HandleGameEnded;
        isSubscribed = true;
    }

    private void UnsubscribeFromGameManager()
    {
        if (!isSubscribed || GameManager.Instance == null)
            return;

        GameManager.Instance.RoundStarted -= HandleRoundStarted;
        GameManager.Instance.RoundResolved -= HandleRoundResolved;
        GameManager.Instance.GameEnded -= HandleGameEnded;
        isSubscribed = false;
    }

    private void HandleRoundStarted(EncounterData encounter, PotionData requestedPotion)
    {
        ShowRequest(encounter, requestedPotion);
    }

    private void HandleRoundResolved(BattleOutcome outcome, EncounterData encounter, PotionData requestedPotion, PotionData deliveredPotion)
    {
        Hide();
    }

    private void HandleGameEnded()
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