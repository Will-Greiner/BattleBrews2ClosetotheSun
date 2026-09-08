using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text slotNameText;
    [SerializeField] private TMP_Text detailsText;
    [SerializeField] private Button selectButton;
    [SerializeField] private TMP_Text selectButtonText;
    [SerializeField] private Button deleteButton;
    [SerializeField] private TMP_Text deleteButtonText;
    [SerializeField, Min(0.5f)] private float deleteConfirmationDuration = 3f;

    private int slot;
    private bool awaitingDeleteConfirmation;
    private bool awaitingOverwriteConfirmation;
    private bool hasSaveData;
    private float confirmationExpiresAt;
    private SaveSlotMenuMode currentMode;
    private Action<int> selectRequested;
    private Action<int> deleteRequested;

    private void Update()
    {
        if (!awaitingDeleteConfirmation && !awaitingOverwriteConfirmation || Time.unscaledTime < confirmationExpiresAt)
            return;

        ResetDeleteConfirmation();
        ResetOverwriteConfirmation();
    }

    private void OnDestroy()
    {
        if (selectButton != null) selectButton.onClick.RemoveListener(HandleSelect);
        if (deleteButton != null) deleteButton.onClick.RemoveListener(HandleDelete);
    }

    public void Display(int slotNumber, SaveGameData data, SaveSlotMenuMode mode, Action<int> onSelect, Action<int> onDelete)
    {
        slot = slotNumber;
        currentMode = mode;
        hasSaveData = data != null;
        selectRequested = onSelect;
        deleteRequested = onDelete;

        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(HandleSelect);
            selectButton.onClick.AddListener(HandleSelect);
            selectButton.interactable = mode == SaveSlotMenuMode.NewGame || hasSaveData;
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveListener(HandleDelete);
            deleteButton.onClick.AddListener(HandleDelete);
            deleteButton.gameObject.SetActive(data != null);
        }

        if (slotNameText != null) slotNameText.text = $"Save {slotNumber}";
        if (detailsText != null) detailsText.text = BuildDetails(data);
        if (selectButtonText != null) selectButtonText.text = GetDefaultSelectText();
        ResetDeleteConfirmation();
        ResetOverwriteConfirmation();
    }

    private string BuildDetails(SaveGameData data)
    {
        if (data == null)
            return "New Game";

        string lastPlayed = DateTime.TryParse(data.lastPlayedUtc, out DateTime parsed) ? parsed.ToLocalTime().ToString("MMM d, yyyy  h:mm tt") : "Unknown date";
        return $"Round {Mathf.Max(1, data.currentRound)}  |  {data.currency} coins\nLast played {lastPlayed}";
    }

    private void HandleSelect()
    {
        if (currentMode == SaveSlotMenuMode.NewGame && hasSaveData && !awaitingOverwriteConfirmation)
        {
            awaitingOverwriteConfirmation = true;
            confirmationExpiresAt = Time.unscaledTime + deleteConfirmationDuration;
            if (selectButtonText != null) selectButtonText.text = "Confirm Overwrite";
            return;
        }

        selectRequested?.Invoke(slot);
    }

    private void HandleDelete()
    {
        if (!awaitingDeleteConfirmation)
        {
            awaitingDeleteConfirmation = true;
            confirmationExpiresAt = Time.unscaledTime + deleteConfirmationDuration;
            if (deleteButtonText != null) deleteButtonText.text = "Confirm Delete";
            return;
        }

        deleteRequested?.Invoke(slot);
        ResetDeleteConfirmation();
    }

    private void ResetDeleteConfirmation()
    {
        awaitingDeleteConfirmation = false;
        if (deleteButtonText != null) deleteButtonText.text = "Delete";
    }

    private void ResetOverwriteConfirmation()
    {
        awaitingOverwriteConfirmation = false;
        if (selectButtonText != null) selectButtonText.text = GetDefaultSelectText();
    }

    private string GetDefaultSelectText()
    {
        if (currentMode == SaveSlotMenuMode.Load)
            return "Load";

        return hasSaveData ? "Overwrite" : "Start";
    }
}
