using System;
using UnityEngine;
using UnityEngine.UI;

public enum SaveSlotMenuMode
{
    Load,
    NewGame
}

public class SaveSlotMenuUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private SaveSlotRowUI[] slotRows = new SaveSlotRowUI[SaveManager.SlotCount];
    [SerializeField] private Button backButton;

    private Action<int> slotSelected;
    private SaveSlotMenuMode mode;

    public event Action SlotsChanged;
    public event Action Closed;

    public bool IsOpen => canvasGroup != null && canvasGroup.alpha > 0.99f;

    private void Awake()
    {
        if (backButton != null) backButton.onClick.AddListener(Hide);
        Hide();
    }

    private void OnDestroy()
    {
        if (backButton != null) backButton.onClick.RemoveListener(Hide);
    }

    public void Show(SaveSlotMenuMode menuMode, Action<int> onSlotSelected)
    {
        mode = menuMode;
        slotSelected = onSlotSelected;
        Refresh();
        SetVisible(true);

        CursorManager.Instance?.ShowForUI(this);
    }

    public void Hide()
    {
        Hide(true);
        CursorManager.Instance?.HideForUI(this);
    }

    public void HideWithoutNotification()
    {
        Hide(false);
        CursorManager.Instance?.HideForUI(this);
    }

    public void Refresh()
    {
        for (int index = 0; index < slotRows.Length; index++)
        {
            if (slotRows[index] == null)
                continue;

            int slot = index + 1;
            slotRows[index].Display(slot, SaveManager.Load(slot), mode, HandleSlotSelected, HandleDeleteRequested);
        }
    }

    private void HandleSlotSelected(int slot)
    {
        slotSelected?.Invoke(slot);
    }

    private void HandleDeleteRequested(int slot)
    {
        SaveManager.Delete(slot);
        Refresh();
        SlotsChanged?.Invoke();
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    private void Hide(bool notify)
    {
        bool wasOpen = IsOpen;
        SetVisible(false);

        if (notify && wasOpen)
            Closed?.Invoke();
    }
}
