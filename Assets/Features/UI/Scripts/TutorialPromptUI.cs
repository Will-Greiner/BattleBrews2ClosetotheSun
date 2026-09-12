using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPromptUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private Button skipButton;

    public System.Action SkipRequested;

    private void Awake()
    {
        if (skipButton != null)
            skipButton.onClick.AddListener(HandleSkip);

        Hide();
    }

    private void OnDestroy()
    {
        if (skipButton != null)
            skipButton.onClick.RemoveListener(HandleSkip);
    }

    public void Show(string instruction)
    {
        if (instructionText != null)
            instructionText.text = instruction;

        SetVisible(true);
    }

    public void Hide()
    {
        if (instructionText != null)
            instructionText.text = string.Empty;

        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    private void HandleSkip()
    {
        SkipRequested?.Invoke();
    }
}
