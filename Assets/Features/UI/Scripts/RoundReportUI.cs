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

    private void Awake()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(HandleContinueClicked);

        Hide();
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(HandleContinueClicked);
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

        SetText(resultText, didWin ? "VICTORY" : "DEFEAT", outcomeColor);
        SetText(gradeText, GetRandomEntry(didWin ? winGrades : loseGrades), outcomeColor);
        SetText(encounterText, encounter.EncounterName);
        SetText(outcomeText, didWin ? encounter.WinOutcomeText : encounter.LoseOutcomeText);
        SetText(commentText, GetRandomEntry(didWin ? winComments : loseComments));

        SetText(requestedPotionLabel, requestedPotion.PotionName);
        SetIcon(requestedPotionIcon, requestedPotion.Icon);

        if (deliveredPotion != null)
        {
            SetText(deliveredPotionLabel, deliveredPotion.PotionName);
            SetIcon(deliveredPotionIcon, deliveredPotion.Icon);
        }
        else
        {
            SetText(deliveredPotionLabel, "No Potion Delivered");
            SetIcon(deliveredPotionIcon, null);
        }

        int remainingLives = GameManager.Instance != null ? GameManager.Instance.Lives : 0;
        SetText(livesText, $"Lives Remaining: {remainingLives}");

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (continueButton != null)
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

    private static void SetText(TMP_Text target, string value, Color? color = null)
    {
        if (target == null)
            return;

        target.text = value;

        if (color.HasValue)
            target.color = color.Value;
    }

    private static void SetIcon(Image target, Sprite sprite)
    {
        if (target == null)
            return;

        target.sprite = sprite;
        target.enabled = sprite != null;
    }
}
