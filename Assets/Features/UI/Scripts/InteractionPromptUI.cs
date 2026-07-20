using System.Collections;
using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private Transform handTarget;
    [SerializeField] private Camera playerCamera;

    [Header("Position")]
    [Tooltip("Position relative to the camera's right, up, and forward directions.")]
    [SerializeField] private Vector3 handOffset = new Vector3(0f, 0.3f, 0f);

    private Coroutine temporaryPromptRoutine;

    public bool IsShowingTemporaryPrompt => temporaryPromptRoutine != null;

    private void Awake()
    {
        ForceHide();
    }

    private void LateUpdate()
    {
        if (handTarget == null || playerCamera == null)
            return;

        Transform cameraTransform = playerCamera.transform;

        transform.position = handTarget.position + cameraTransform.right * handOffset.x + cameraTransform.up * handOffset.y + cameraTransform.forward * handOffset.z;
        transform.rotation = Quaternion.LookRotation(cameraTransform.forward, cameraTransform.up);
    }

    private void OnDisable()
    {
        if (temporaryPromptRoutine != null)
        {
            StopCoroutine(temporaryPromptRoutine);
            temporaryPromptRoutine = null;
        }
    }

    public void Show(string prompt)
    {
        if (IsShowingTemporaryPrompt)
            return;

        ForceShow(prompt);
    }

    public void Hide()
    {
        if (IsShowingTemporaryPrompt)
            return;

        ForceHide();
    }

    public void ShowTemporary(string message, float duration = 1.5f)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (temporaryPromptRoutine != null)
            StopCoroutine(temporaryPromptRoutine);

        temporaryPromptRoutine = StartCoroutine(TemporaryPromptRoutine(message, duration));
    }

    private IEnumerator TemporaryPromptRoutine(string message, float duration)
    {
        ForceShow(message);

        if (duration > 0f)
            yield return new WaitForSecondsRealtime(duration);

        temporaryPromptRoutine = null;
        ForceHide();
    }

    private void ForceShow(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            ForceHide();
            return;
        }

        promptText.text = prompt;
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void ForceHide()
    {
        if (promptText != null)
            promptText.text = string.Empty;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}