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

    private void Awake()
    {
        Hide();
    }

    private void LateUpdate()
    {
        if (handTarget == null || playerCamera == null)
            return;

        Transform cameraTransform = playerCamera.transform;

        transform.position =
            handTarget.position +
            cameraTransform.right * handOffset.x +
            cameraTransform.up * handOffset.y +
            cameraTransform.forward * handOffset.z;

        transform.rotation = Quaternion.LookRotation(
            cameraTransform.forward,
            cameraTransform.up
        );
    }

    public void Show(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            Hide();
            return;
        }

        promptText.text = prompt;
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void Hide()
    {
        promptText.text = string.Empty;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}