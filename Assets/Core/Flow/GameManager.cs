using System;
using System.Collections.Generic;
using UnityEngine;

public enum BattleOutcome
{
    Win,
    Lose
}

public enum GameState
{
    NotStarted,
    RoundStarting,
    RoundActive,
    RoundResolving,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Content")]
    [SerializeField] private EncounterData[] encounters;
    [SerializeField] private PotionDatabase potionDatabase;

    [Header("Game")]
    [Min(1)]
    [SerializeField] private int startingLives = 3;

    [Min(1)]
    [SerializeField] private int totalRounds = 5;

    [SerializeField] private bool startAutomatically = true;

    private readonly List<EncounterData> availableEncounters = new();

    public event Action GameStarted;
    public event Action<int, int> LivesChanged;
    public event Action<int, int> RoundChanged;
    public event Action<EncounterData, PotionData> RoundStarted;
    public event Action<EncounterData, PotionData> RoundActivated;
    public event Action<BattleOutcome, EncounterData, PotionData, PotionData> RoundResolved;
    public event Action GameEnded;

    public EncounterData CurrentEncounter { get; private set; }
    public PotionData RequestedPotion { get; private set; }
    public PotionData DeliveredPotion { get; private set; }
    public BattleOutcome CurrentOutcome { get; private set; }
    public GameState State { get; private set; } = GameState.NotStarted;
    public int CurrentRound { get; private set; }
    public int Lives { get; private set; }
    public int TotalRounds => totalRounds;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (startAutomatically)
            StartGame();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void StartGame()
    {
        if (State != GameState.NotStarted && State != GameState.GameOver)
            return;

        CurrentRound = 0;
        Lives = startingLives;
        CurrentEncounter = null;
        RequestedPotion = null;
        DeliveredPotion = null;
        State = GameState.NotStarted;

        GameStarted?.Invoke();
        LivesChanged?.Invoke(Lives, startingLives);

        StartNextRound();
    }

    public void StartNextRound()
    {
        if (State == GameState.RoundStarting || State == GameState.RoundActive)
            return;

        if (Lives <= 0 || CurrentRound >= totalRounds)
        {
            EndGame();
            return;
        }

        CurrentRound++;
        State = GameState.RoundStarting;

        CurrentEncounter = SelectEncounter(CurrentRound);

        if (CurrentEncounter == null)
        {
            Debug.LogError($"No encounter is available for round {CurrentRound}.", this);
            EndGame();
            return;
        }

        RequestedPotion = CurrentEncounter.SelectRequestedPotion(CurrentRound, potionDatabase);

        if (RequestedPotion == null)
        {
            Debug.LogError($"{CurrentEncounter.name} could not select a requested potion for round {CurrentRound}.", CurrentEncounter);
            EndGame();
            return;
        }

        DeliveredPotion = null;

        RoundChanged?.Invoke(CurrentRound, totalRounds);
        RoundStarted?.Invoke(CurrentEncounter, RequestedPotion);
    }

    public bool ActivateCurrentRound()
    {
        if (State != GameState.RoundStarting || CurrentEncounter == null || RequestedPotion == null)
            return false;

        State = GameState.RoundActive;
        RoundActivated?.Invoke(CurrentEncounter, RequestedPotion);
        return true;
    }

    public bool CanDeliverPotion(PotionData potion)
    {
        return State == GameState.RoundActive && potion != null;
    }

    public bool DeliverPotion(PotionData potion)
    {
        if (!CanDeliverPotion(potion))
            return false;

        DeliveredPotion = potion;
        BattleOutcome outcome = DeliveredPotion == RequestedPotion ? BattleOutcome.Win : BattleOutcome.Lose;
        ResolveRound(outcome);
        return true;
    }

    public bool FailCurrentRound()
    {
        if (State != GameState.RoundActive)
            return false;

        DeliveredPotion = null;
        ResolveRound(BattleOutcome.Lose);
        return true;
    }

    public void ContinueAfterRound()
    {
        if (State != GameState.RoundResolving)
            return;

        if (Lives <= 0 || CurrentRound >= totalRounds)
        {
            EndGame();
            return;
        }

        StartNextRound();
    }

    private void ResolveRound(BattleOutcome outcome)
    {
        CurrentOutcome = outcome;
        State = GameState.RoundResolving;

        if (CurrentOutcome == BattleOutcome.Lose)
        {
            Lives = Mathf.Max(0, Lives - 1);
            LivesChanged?.Invoke(Lives, startingLives);
        }

        RoundResolved?.Invoke(CurrentOutcome, CurrentEncounter, RequestedPotion, DeliveredPotion);
    }

    private EncounterData SelectEncounter(int round)
    {
        availableEncounters.Clear();

        if (encounters == null)
            return null;

        foreach (EncounterData encounter in encounters)
        {
            if (encounter != null && encounter.IsAvailableInRound(round))
                availableEncounters.Add(encounter);
        }

        if (availableEncounters.Count == 0)
            return null;

        return availableEncounters[UnityEngine.Random.Range(0, availableEncounters.Count)];
    }

    private void EndGame()
    {
        State = GameState.GameOver;
        GameEnded?.Invoke();
    }
}