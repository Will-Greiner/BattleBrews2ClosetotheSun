using TMPro;
using UnityEngine;

public class GameHUDUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text timerText;

    [Header("Timer Color")]
    [SerializeField] private Color normalTimerColor = Color.white;
    [SerializeField] private Color warningTimerColor = Color.red;

    [Min(0f)]
    [SerializeField] private float warningTime = 10f;

    private bool isSubscribedToGameManager;
    private bool isSubscribedToTimeManager;

    private void OnEnable()
    {
        SubscribeToManagers();
    }

    private void Start()
    {
        SubscribeToManagers();
        RefreshDisplay();
    }

    private void OnDisable()
    {
        UnsubscribeFromManagers();
    }

    private void SubscribeToManagers()
    {
        if (!isSubscribedToGameManager && GameManager.Instance != null)
        {
            GameManager.Instance.RoundChanged += HandleRoundChanged;
            GameManager.Instance.GameEnded += HandleGameEnded;
            isSubscribedToGameManager = true;
        }

        if (!isSubscribedToTimeManager && TimeManager.Instance != null)
        {
            TimeManager.Instance.TimeChanged += HandleTimeChanged;
            isSubscribedToTimeManager = true;
        }
    }

    private void UnsubscribeFromManagers()
    {
        if (isSubscribedToGameManager && GameManager.Instance != null)
        {
            GameManager.Instance.RoundChanged -= HandleRoundChanged;
            GameManager.Instance.GameEnded -= HandleGameEnded;
        }

        if (isSubscribedToTimeManager && TimeManager.Instance != null)
            TimeManager.Instance.TimeChanged -= HandleTimeChanged;

        isSubscribedToGameManager = false;
        isSubscribedToTimeManager = false;
    }

    private void HandleRoundChanged(int currentRound, int totalRounds)
    {
        SetRoundText(currentRound, totalRounds);
    }

    private void HandleTimeChanged(float timeRemaining)
    {
        SetTimerText(timeRemaining);
    }

    private void HandleGameEnded()
    {
        roundText.text = "GAME OVER";
        SetTimerText(0f);
    }

    private void RefreshDisplay()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.NotStarted)
            SetRoundText(GameManager.Instance.CurrentRound, GameManager.Instance.TotalRounds);
        else
            roundText.text = string.Empty;

        if (TimeManager.Instance != null)
            SetTimerText(TimeManager.Instance.TimeRemaining);
        else
            SetTimerText(0f);
    }

    private void SetRoundText(int currentRound, int totalRounds)
    {
        roundText.text = $"ROUND {currentRound} / {totalRounds}";
    }

    private void SetTimerText(float timeRemaining)
    {
        int totalSeconds = Mathf.CeilToInt(timeRemaining);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
        timerText.color = timeRemaining > 0f && timeRemaining <= warningTime ? warningTimerColor : normalTimerColor;
    }
}