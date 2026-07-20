using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PotionRequirementDisplay : MonoBehaviour
{
    private class RequirementRow
    {
        public PotionRequirementRowUI Row { get; }
        public int RequirementIndex { get; }
        public int UnitIndex { get; }

        public RequirementRow(PotionRequirementRowUI row, int requirementIndex, int unitIndex)
        {
            Row = row;
            RequirementIndex = requirementIndex;
            UnitIndex = unitIndex;
        }
    }

    [Header("References")]
    [SerializeField] private CauldronController cauldron;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text potionNameText;
    [SerializeField] private TMP_Text readyText;
    [SerializeField] private Transform requirementContainer;
    [SerializeField] private PotionRequirementRowUI requirementRowPrefab;

    [Header("Text")]
    [SerializeField] private string readyMessage = "READY TO STIR";

    private readonly List<RequirementRow> spawnedRows = new();
    private PotionData displayedPotion;
    private bool isSubscribedToGameManager;

    private void Awake()
    {
        Hide();
    }

    private void OnEnable()
    {
        if (cauldron != null)
            cauldron.ContributionsChanged += Refresh;

        SubscribeToGameManager();
    }

    private void Start()
    {
        SubscribeToGameManager();

        if (GameManager.Instance != null && GameManager.Instance.State == GameState.RoundActive)
            Show(GameManager.Instance.RequestedPotion);
    }

    private void OnDisable()
    {
        if (cauldron != null)
            cauldron.ContributionsChanged -= Refresh;

        UnsubscribeFromGameManager();
    }

    private void SubscribeToGameManager()
    {
        if (isSubscribedToGameManager || GameManager.Instance == null)
            return;

        GameManager.Instance.RoundActivated += HandleRoundActivated;
        GameManager.Instance.RoundResolved += HandleRoundResolved;
        GameManager.Instance.GameEnded += HandleGameEnded;
        isSubscribedToGameManager = true;
    }

    private void UnsubscribeFromGameManager()
    {
        if (!isSubscribedToGameManager || GameManager.Instance == null)
            return;

        GameManager.Instance.RoundActivated -= HandleRoundActivated;
        GameManager.Instance.RoundResolved -= HandleRoundResolved;
        GameManager.Instance.GameEnded -= HandleGameEnded;
        isSubscribedToGameManager = false;
    }

    private void HandleRoundActivated(EncounterData encounter, PotionData requestedPotion)
    {
        Show(requestedPotion);
    }

    private void HandleRoundResolved(BattleOutcome outcome, EncounterData encounter, PotionData requestedPotion, PotionData deliveredPotion)
    {
        Hide();
    }

    private void HandleGameEnded()
    {
        Hide();
    }

    public void Show(PotionData potion)
    {
        if (potion == null)
        {
            Hide();
            return;
        }

        displayedPotion = potion;
        BuildRows();
        SetVisible(true);
        Refresh();
    }

    public void Hide()
    {
        displayedPotion = null;
        ClearRows();

        if (potionNameText != null)
            potionNameText.text = string.Empty;

        if (readyText != null)
        {
            readyText.text = string.Empty;
            readyText.gameObject.SetActive(false);
        }

        SetVisible(false);
    }

    public void Refresh()
    {
        if (displayedPotion == null || cauldron == null)
            return;

        RecipeMatchResult result = cauldron.EvaluatePotion(displayedPotion);

        if (potionNameText != null)
            potionNameText.text = displayedPotion.PotionName;

        foreach (RequirementRow spawnedRow in spawnedRows)
        {
            if (spawnedRow.Row == null)
                continue;

            int completedCount = result.GetCompletedCount(spawnedRow.RequirementIndex);
            bool isComplete = spawnedRow.UnitIndex < completedCount;
            spawnedRow.Row.gameObject.SetActive(!isComplete);
        }

        if (readyText != null)
        {
            readyText.text = readyMessage;
            readyText.gameObject.SetActive(result.AllRequirementsMet);
        }
    }

    private void BuildRows()
    {
        ClearRows();

        if (displayedPotion == null || requirementContainer == null || requirementRowPrefab == null)
            return;

        for (int requirementIndex = 0; requirementIndex < displayedPotion.Requirements.Count; requirementIndex++)
        {
            RecipeRequirement requirement = displayedPotion.Requirements[requirementIndex];

            if (requirement == null)
                continue;

            for (int unitIndex = 0; unitIndex < requirement.RequiredCount; unitIndex++)
            {
                PotionRequirementRowUI row = Instantiate(requirementRowPrefab, requirementContainer);
                row.Display(requirement);
                spawnedRows.Add(new RequirementRow(row, requirementIndex, unitIndex));
            }
        }
    }

    private void ClearRows()
    {
        foreach (RequirementRow spawnedRow in spawnedRows)
        {
            if (spawnedRow.Row != null)
                Destroy(spawnedRow.Row.gameObject);
        }

        spawnedRows.Clear();
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