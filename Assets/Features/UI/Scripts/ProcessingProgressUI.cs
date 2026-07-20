using UnityEngine;
using UnityEngine.UI;

public class ProcessingProgressUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private IngredientProcessingStation station;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image progressFill;
    [SerializeField] private Camera playerCamera;

    [Header("Display")]
    [SerializeField] private bool hideWithoutIngredient = true;
    [SerializeField] private bool faceCamera = true;

    private void Awake()
    {
        if (progressFill != null)
            progressFill.fillAmount = 0f;

        SetVisible(false);
    }

    private void OnEnable()
    {
        if (station != null)
        {
            station.IngredientLoaded += HandleIngredientLoaded;
            station.ProgressChanged += HandleProgressChanged;
            station.ProcessingCompleted += HandleProcessingCompleted;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (station != null)
        {
            station.IngredientLoaded -= HandleIngredientLoaded;
            station.ProgressChanged -= HandleProgressChanged;
            station.ProcessingCompleted -= HandleProcessingCompleted;
        }
    }

    private void LateUpdate()
    {
        if (!faceCamera || playerCamera == null)
            return;

        Transform cameraTransform = playerCamera.transform;
        transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position, cameraTransform.up);
    }

    public void Refresh()
    {
        if (station == null)
        {
            SetProgress(0f);
            SetVisible(false);
            return;
        }

        SetProgress(station.NormalizedProgress);
        SetVisible(!hideWithoutIngredient || station.HasIngredient);
    }

    private void HandleIngredientLoaded(IngredientData ingredient)
    {
        SetProgress(0f);
        SetVisible(true);
    }

    private void HandleProgressChanged(float currentProgress, float requiredProgress)
    {
        float normalizedProgress = requiredProgress > 0f ? currentProgress / requiredProgress : 0f;
        SetProgress(normalizedProgress);

        if (station != null)
            SetVisible(!hideWithoutIngredient || station.HasIngredient);
    }

    private void HandleProcessingCompleted(IngredientData ingredient, int propertyLevel, ProcessedIngredientItem output)
    {
        SetProgress(0f);
        SetVisible(!hideWithoutIngredient);
    }

    private void SetProgress(float normalizedProgress)
    {
        if (progressFill != null)
            progressFill.fillAmount = Mathf.Clamp01(normalizedProgress);
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