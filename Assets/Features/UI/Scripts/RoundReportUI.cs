using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoundReportUI : MonoBehaviour
{
    [Header("Visibility")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Text")]
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text gradeText;
    [SerializeField] private TMP_Text encounterText;
    [SerializeField] private TMP_Text outcomeText;
    [SerializeField] private TMP_Text commentText;
    [SerializeField] private TMP_Text requestedPotionLabel;
    [SerializeField] private TMP_Text deliveredPotionLabel;
    [SerializeField] private TMP_Text livesText;

    [Header("Icons")]
    [SerializeField] private Image requestedPotionIcon;
    [SerializeField] private Image deliveredPotionIcon;

    [Header("Button")]
    [SerializeField] private Button continueButton;

    [Header("Colors")]
    [SerializeField] private Color winColor = new Color(0.35f, 0.85f, 0.4f);
    [SerializeField] private Color loseColor = new Color(0.9f, 0.25f, 0.25f);

    [Header("Win Grades")]
    [SerializeField] private string[] winGrades = { "A+", "A", "A-" };

    [Header("Lose Grades")]
    [SerializeField] private string[] loseGrades = { "F", "F-", "F--", "F---" };

    [Header("Win Comments")]
    [SerializeField] private string[] winComments =
    {
        "Excellent brewing!",
        "Exactly what the fighter needed.",
        "A decisive success.",
        "Perfectly prepared for battle."
    };

    [Header("Lose Comments")]
    [SerializeField] private string[] loseComments =
    {
        "The requested potion was not delivered.",
        "The fighter was left unprepared.",
        "This brew needs some work.",
        "Better luck next round."
    };

    private bool isSubscribed;

    private void Awake()
    {
        continueButton.onClick.AddListener(HandleContinueClicked);
        Hide();
    }

    private void OnEnable()
    {
        SubscribeToGameManager();
    }

    private void Start()
    {
        SubscribeToGameManager();

        if (GameManager.Instance != null && GameManager.Instance.State == GameState.RoundResolving)
            ShowReport(GameManager.Instance.CurrentOutcome, GameManager.Instance.CurrentEncounter, GameManager.Instance.RequestedPotion, GameManager.Instance.DeliveredPotion);
    }

    private void OnDisable()
    {
        UnsubscribeFromGameManager();
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(HandleContinueClicked);
    }

    private void SubscribeToGameManager()
    {
        if (isSubscribed || GameManager.Instance == null)
            return;

        GameManager.Instance.RoundResolved += HandleRoundResolved;
        GameManager.Instance.GameEnded += HandleGameEnded;
        isSubscribed = true;
    }

    private void UnsubscribeFromGameManager()
    {
        if (!isSubscribed || GameManager.Instance == null)
            return;

        GameManager.Instance.RoundResolved -= HandleRoundResolved;
        GameManager.Instance.GameEnded -= HandleGameEnded;
        isSubscribed = false;
    }

    private void HandleRoundResolved(BattleOutcome outcome, EncounterData encounter, PotionData requestedPotion, PotionData deliveredPotion)
    {
        ShowReport(outcome, encounter, requestedPotion, deliveredPotion);
    }

    private void HandleGameEnded()
    {
        Hide();
    }

    private void HandleContinueClicked()
    {
        Hide();

        if (GameManager.Instance != null)
            GameManager.Instance.ContinueAfterRound();
    }

    public void ShowReport(BattleOutcome outcome, EncounterData encounter, PotionData requestedPotion, PotionData deliveredPotion)
    {
        if (encounter == null || requestedPotion == null)
        {
            Hide();
            return;
        }

        bool didWin = outcome == BattleOutcome.Win;
        Color outcomeColor = didWin ? winColor : loseColor;

        resultText.text = didWin ? "VICTORY" : "DEFEAT";
        resultText.color = outcomeColor;

        gradeText.text = GetRandomEntry(didWin ? winGrades : loseGrades);
        gradeText.color = outcomeColor;

        encounterText.text = encounter.EncounterName;
        outcomeText.text = didWin ? encounter.WinOutcomeText : encounter.LoseOutcomeText;
        commentText.text = GetRandomEntry(didWin ? winComments : loseComments);

        requestedPotionLabel.text = requestedPotion.PotionName;
        requestedPotionIcon.sprite = requestedPotion.Icon;
        requestedPotionIcon.enabled = requestedPotion.Icon != null;

        if (deliveredPotion != null)
        {
            deliveredPotionLabel.text = deliveredPotion.PotionName;
            deliveredPotionIcon.sprite = deliveredPotion.Icon;
            deliveredPotionIcon.enabled = deliveredPotion.Icon != null;
        }
        else
        {
            deliveredPotionLabel.text = "No Potion Delivered";
            deliveredPotionIcon.sprite = null;
            deliveredPotionIcon.enabled = false;
        }

        int remainingLives = GameManager.Instance != null ? GameManager.Instance.Lives : 0;
        livesText.text = $"Lives Remaining: {remainingLives}";

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        continueButton.interactable = true;
    }

    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (continueButton != null)
            continueButton.interactable = false;
    }

    private string GetRandomEntry(string[] entries)
    {
        if (entries == null || entries.Length == 0)
            return string.Empty;

        return entries[Random.Range(0, entries.Length)];
    }
}