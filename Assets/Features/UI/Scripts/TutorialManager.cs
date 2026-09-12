using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private TutorialSequenceData sequence;
    [SerializeField] private TutorialPromptUI promptUI;
    [SerializeField] private bool useBuiltInSequenceWhenUnassigned = true;
    [SerializeField] private bool onlyBeginOnRoundOne = true;

    private readonly List<TutorialStep> builtInSteps = new();
    private IReadOnlyList<TutorialStep> activeSteps;
    private TutorialTarget highlightedTarget;
    private int stepIndex;
    private bool active;
    private bool subscribedToGameManager;

    public bool IsActive => active;
    public int CurrentStepIndex => stepIndex;

    private void Awake()
    {
        BuildDefaultSequence();
        activeSteps = sequence != null && sequence.Steps.Count > 0 ? sequence.Steps : builtInSteps;
    }

    private void OnEnable()
    {
        TutorialEvents.Triggered += HandleTrigger;

        if (promptUI != null)
            promptUI.SkipRequested += Skip;
    }

    private void Start()
    {
        SubscribeToGameManager();

        if (GameManager.Instance != null && GameManager.Instance.State == GameState.RoundActive)
            TryBegin(GameManager.Instance.CurrentRound);
    }

    private void LateUpdate()
    {
        if (!subscribedToGameManager)
            SubscribeToGameManager();

        if (active && highlightedTarget == null)
            RefreshHighlight();
    }

    private void OnDisable()
    {
        TutorialEvents.Triggered -= HandleTrigger;

        if (promptUI != null)
            promptUI.SkipRequested -= Skip;

        if (subscribedToGameManager && GameManager.Instance != null)
            GameManager.Instance.RoundActivated -= HandleRoundActivated;

        subscribedToGameManager = false;
        ClearHighlight();
    }

    public void Begin()
    {
        if (activeSteps == null || activeSteps.Count == 0 || ProgressionManager.Instance == null || ProgressionManager.Instance.TutorialCompleted)
            return;

        stepIndex = 0;
        active = true;
        ShowCurrentStep();
    }

    public void Skip()
    {
        if (!active)
            return;

        Complete();
    }

    private void SubscribeToGameManager()
    {
        if (subscribedToGameManager || GameManager.Instance == null)
            return;

        GameManager.Instance.RoundActivated += HandleRoundActivated;
        subscribedToGameManager = true;
    }

    private void HandleRoundActivated(EncounterData encounter, PotionData potion)
    {
        TutorialEvents.Report(TutorialTrigger.RoundActivated, potion != null ? potion.PotionName : string.Empty);
        TryBegin(GameManager.Instance != null ? GameManager.Instance.CurrentRound : 0);
    }

    private void TryBegin(int round)
    {
        if (active || ProgressionManager.Instance == null || ProgressionManager.Instance.TutorialCompleted)
            return;

        if (!onlyBeginOnRoundOne || round == 1)
            Begin();
    }

    private void HandleTrigger(TutorialTrigger trigger, string id)
    {
        if (!active || stepIndex < 0 || stepIndex >= activeSteps.Count)
            return;

        TutorialStep step = activeSteps[stepIndex];

        if (step.CompletionTrigger != trigger)
            return;

        if (!string.IsNullOrWhiteSpace(step.RequiredId) && !string.Equals(step.RequiredId, id, StringComparison.OrdinalIgnoreCase))
            return;

        ClearHighlight();
        stepIndex++;

        if (stepIndex >= activeSteps.Count)
        {
            Complete();
            return;
        }

        ShowCurrentStep();
    }

    private void ShowCurrentStep()
    {
        TutorialStep step = activeSteps[stepIndex];
        promptUI?.Show(step.Instruction);
        RefreshHighlight();
    }

    private void RefreshHighlight()
    {
        if (!active || stepIndex < 0 || stepIndex >= activeSteps.Count)
            return;

        TutorialTarget target = TutorialTarget.Find(activeSteps[stepIndex].HighlightTargetId);

        if (target == highlightedTarget)
            return;

        ClearHighlight();
        highlightedTarget = target;
        highlightedTarget?.Show();
    }

    private void ClearHighlight()
    {
        highlightedTarget?.Hide();
        highlightedTarget = null;
    }

    private void Complete()
    {
        active = false;
        ClearHighlight();
        promptUI?.Hide();
        ProgressionManager.Instance?.CompleteTutorial();
    }

    private void BuildDefaultSequence()
    {
        builtInSteps.Clear();

        if (!useBuiltInSequenceWhenUnassigned)
            return;

        builtInSteps.Add(new TutorialStep("Take an apple from the dispenser.", TutorialTrigger.IngredientSpawned, "apple", "apple-dispenser"));
        builtInSteps.Add(new TutorialStep("Drop the apple into the cauldron.", TutorialTrigger.IngredientAdded, "apple", "cauldron"));
        builtInSteps.Add(new TutorialStep("Grab the stirring stick and stir the mixture.", TutorialTrigger.StirringStarted, string.Empty, "stir-stick"));
        builtInSteps.Add(new TutorialStep("Keep stirring until the potion is ready.", TutorialTrigger.PotionCreated, "Potion of Crunch", "cauldron"));
        builtInSteps.Add(new TutorialStep("Pick up the finished potion.", TutorialTrigger.ItemGrabbed, "Potion of Crunch", "potion-spawn"));
        builtInSteps.Add(new TutorialStep("Deliver the potion through the hatch.", TutorialTrigger.PotionDelivered, "Potion of Crunch", "delivery-hatch"));
        builtInSteps.Add(new TutorialStep(
            "You earned some coin. Open the shop.",
            TutorialTrigger.ShopOpened,
            string.Empty,
            "shop"));

        builtInSteps.Add(new TutorialStep(
            "Buy the Mortar and Pestle to begin processing ingredients.",
            TutorialTrigger.OfferPurchased,
            "processing.level.1",
            "shop-processing.level.1"));

        builtInSteps.Add(new TutorialStep(
            "Take another apple.",
            TutorialTrigger.IngredientSpawned,
            "apple",
            "apple-dispenser"));

        builtInSteps.Add(new TutorialStep(
            "Place the apple into the Mortar and Pestle.",
            TutorialTrigger.ProcessingIngredientInserted,
            "apple",
            "mortar"));

        builtInSteps.Add(new TutorialStep(
            "Use the pestle to crush the apple.",
            TutorialTrigger.ProcessingStarted,
            "processing.level.1",
            "pestle"));

        builtInSteps.Add(new TutorialStep(
            "Processing created a sample containing one of the apple's hidden properties.",
            TutorialTrigger.ProcessingCompleted,
            "apple",
            "mortar-output"));

        builtInSteps.Add(new TutorialStep(
            "Pick up the processed sample and place it on the altar.",
            TutorialTrigger.AltarSampleInserted,
            "apple",
            "altar"));

        builtInSteps.Add(new TutorialStep(
            "Complete the altar analysis to discover the property.",
            TutorialTrigger.PropertyDiscovered,
            "apple",
            "altar"));

        builtInSteps.Add(new TutorialStep(
            "Open your recipe book to review what you discovered.",
            TutorialTrigger.RecipeBookOpened,
            string.Empty,
            "recipe-book"));

        builtInSteps.Add(new TutorialStep(
            "Ingredient pages show the properties you have discovered. Potion pages show recipes that can use ingredients or properties.",
            TutorialTrigger.RecipeBookClosed,
            string.Empty,
            "recipe-book"));
            }
}
