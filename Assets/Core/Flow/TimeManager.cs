using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Round Time")]
    [Min(1f)]
    [SerializeField] private float startingTime = 120f;

    [Min(0f)]
    [SerializeField] private float secondsLostPerRound = 15f;

    [Min(1f)]
    [SerializeField] private float minimumRoundTime = 30f;

    public event Action<float> TimeChanged;
    public event Action TimerExpired;

    public float TimeRemaining { get; private set; }
    public bool IsRunning { get; private set; }

    private bool isSubscribed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        SubscribeToGameManager();
    }

    private void Start()
    {
        SubscribeToGameManager();
    }

    private void Update()
    {
        if (!IsRunning)
            return;

        TimeRemaining = Mathf.Max(0f, TimeRemaining - Time.deltaTime);
        TimeChanged?.Invoke(TimeRemaining);

        if (TimeRemaining <= 0f)
        {
            IsRunning = false;
            TimerExpired?.Invoke();

            if (GameManager.Instance != null)
                GameManager.Instance.FailCurrentRound();
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromGameManager();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void SubscribeToGameManager()
    {
        if (isSubscribed || GameManager.Instance == null)
            return;

        GameManager.Instance.RoundActivated += HandleRoundActivated;
        GameManager.Instance.RoundResolved += HandleRoundResolved;
        GameManager.Instance.GameEnded += HandleGameEnded;
        isSubscribed = true;
    }

    private void UnsubscribeFromGameManager()
    {
        if (!isSubscribed || GameManager.Instance == null)
            return;

        GameManager.Instance.RoundActivated -= HandleRoundActivated;
        GameManager.Instance.RoundResolved -= HandleRoundResolved;
        GameManager.Instance.GameEnded -= HandleGameEnded;
        isSubscribed = false;
    }

    private void HandleRoundActivated(EncounterData encounter, PotionData requestedPotion)
    {
        StartRoundTimer(GameManager.Instance.CurrentRound);
    }

    private void HandleRoundResolved(BattleOutcome outcome, EncounterData encounter, PotionData requestedPotion, PotionData deliveredPotion)
    {
        PauseTimer();
    }

    private void HandleGameEnded()
    {
        StopTimer();
    }

    private void StartRoundTimer(int round)
    {
        float roundTime = startingTime - secondsLostPerRound * (round - 1);
        TimeRemaining = Mathf.Max(minimumRoundTime, roundTime);
        IsRunning = true;
        TimeChanged?.Invoke(TimeRemaining);
    }

    public void PauseTimer()
    {
        IsRunning = false;
    }

    public void ResumeTimer()
    {
        if (TimeRemaining > 0f)
            IsRunning = true;
    }

    public void StopTimer()
    {
        IsRunning = false;
        TimeRemaining = 0f;
        TimeChanged?.Invoke(TimeRemaining);
    }
}