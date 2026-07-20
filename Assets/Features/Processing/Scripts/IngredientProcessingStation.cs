using System;
using UnityEngine;

public class IngredientProcessingStation : MonoBehaviour
{
    [Header("Processing")]
    [Range(1, 3)] [SerializeField] private int propertyLevel = 1;
    [Min(0.01f)] [SerializeField] private float requiredProgress = 5f;

    [Header("Item Placement")]
    [SerializeField] private Transform ingredientSnapPoint;
    [SerializeField] private Transform outputPoint;
    [SerializeField] private GameObject processedIngredientPrefab;

    [Header("Effects")]
    [SerializeField] private ParticleSystem completionParticles;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip completionClip;

    private GrabbableItem currentItem;
    private IngredientData currentIngredient;
    private float currentProgress;

    public event Action<IngredientData> IngredientLoaded;
    public event Action<float, float> ProgressChanged;
    public event Action<IngredientData, int, ProcessedIngredientItem> ProcessingCompleted;

    public bool HasIngredient => currentItem != null;
    public IngredientData CurrentIngredient => currentIngredient;
    public int PropertyLevel => propertyLevel;
    public float CurrentProgress => currentProgress;
    public float RequiredProgress => requiredProgress;
    public float NormalizedProgress => requiredProgress > 0f ? Mathf.Clamp01(currentProgress / requiredProgress) : 0f;

    public bool CanAcceptIngredient(GrabbableItem item)
    {
        if (currentItem != null || item == null || item.IsHeld)
            return false;

        IngredientItem ingredientItem = item.GetComponent<IngredientItem>();

        if (ingredientItem == null || ingredientItem.Data == null)
            return false;

        IngredientData ingredient = ingredientItem.Data;

        if (ingredient.GetPropertyAtLevel(propertyLevel) == null)
            return false;

        IngredientDiscoveryManager discoveryManager = IngredientDiscoveryManager.Instance;

        // if (discoveryManager != null && discoveryManager.IsPropertyDiscovered(ingredient, propertyLevel))
        //     return false;

        return true;
    }

    public bool TryAcceptIngredient(GrabbableItem item)
    {
        if (!CanAcceptIngredient(item))
            return false;

        IngredientItem ingredientItem = item.GetComponent<IngredientItem>();
        currentItem = item;
        currentIngredient = ingredientItem.Data;
        currentProgress = 0f;

        SnapCurrentIngredient();
        IngredientLoaded?.Invoke(currentIngredient);
        ProgressChanged?.Invoke(currentProgress, requiredProgress);
        return true;
    }

    public bool AddProgress(float amount)
    {
        if (currentItem == null || amount <= 0f)
            return false;

        currentProgress = Mathf.Min(requiredProgress, currentProgress + amount);
        ProgressChanged?.Invoke(currentProgress, requiredProgress);

        if (currentProgress >= requiredProgress)
            CompleteProcessing();

        return true;
    }

    public void ClearStation()
    {
        if (currentItem != null)
            Destroy(currentItem.gameObject);

        currentItem = null;
        currentIngredient = null;
        currentProgress = 0f;
        ProgressChanged?.Invoke(currentProgress, requiredProgress);
    }

    [ContextMenu("Debug Complete Current Ingredient")]
    private void DebugCompleteCurrentIngredient()
    {
        if (currentItem != null)
            AddProgress(requiredProgress - currentProgress);
    }

    private void SnapCurrentIngredient()
    {
        if (currentItem == null)
            return;

        Rigidbody itemRigidbody = currentItem.Rigidbody;

        if (itemRigidbody != null)
        {
            itemRigidbody.linearVelocity = Vector3.zero;
            itemRigidbody.angularVelocity = Vector3.zero;
            itemRigidbody.useGravity = false;
            itemRigidbody.isKinematic = true;
        }

        if (ingredientSnapPoint != null)
        {
            currentItem.transform.SetParent(ingredientSnapPoint);
            currentItem.transform.SetPositionAndRotation(ingredientSnapPoint.position, ingredientSnapPoint.rotation);
        }

        currentItem.enabled = false;
    }

    private void CompleteProcessing()
    {
        IngredientData completedIngredient = currentIngredient;

        if (currentItem != null)
            Destroy(currentItem.gameObject);

        currentItem = null;
        currentIngredient = null;
        currentProgress = 0f;

        ProcessedIngredientItem processedItem = SpawnProcessedIngredient(completedIngredient);
        PlayCompletionEffects();

        ProgressChanged?.Invoke(currentProgress, requiredProgress);
        ProcessingCompleted?.Invoke(completedIngredient, propertyLevel, processedItem);
    }

    private ProcessedIngredientItem SpawnProcessedIngredient(IngredientData ingredient)
    {
        if (processedIngredientPrefab == null)
        {
            Debug.LogError($"{name}: No processed ingredient prefab has been assigned.", this);
            return null;
        }

        if (outputPoint == null)
        {
            Debug.LogError($"{name}: No output point has been assigned.", this);
            return null;
        }

        GameObject output = Instantiate(processedIngredientPrefab, outputPoint.position, outputPoint.rotation);

        Rigidbody outputRigidbody = output.GetComponent<Rigidbody>();

        if (outputRigidbody != null)
        {
            outputRigidbody.linearVelocity = Vector3.zero;
            outputRigidbody.angularVelocity = Vector3.zero;
            outputRigidbody.useGravity = false;
        }

        ProcessedIngredientItem processedItem = output.GetComponent<ProcessedIngredientItem>();

        if (processedItem == null)
        {
            Debug.LogError($"{output.name} requires a ProcessedIngredientItem component.", output);
            Destroy(output);
            return null;
        }

        processedItem.Initialize(ingredient, propertyLevel);
        return processedItem;
    }

    private void PlayCompletionEffects()
    {
        if (completionParticles != null)
            completionParticles.Play();

        if (audioSource != null && completionClip != null)
            audioSource.PlayOneShot(completionClip);
    }

    public string GetRejectionMessage(GrabbableItem item)
    {
        if (currentItem != null)
            return "Station already occupied";

        IngredientItem ingredientItem = item != null ? item.GetComponent<IngredientItem>() : null;

        if (ingredientItem == null || ingredientItem.Data == null)
            return "Requires a raw ingredient";

        IngredientData ingredient = ingredientItem.Data;

        if (ingredient.GetPropertyAtLevel(propertyLevel) == null)
            return $"No Level {propertyLevel} property to extract";

        IngredientDiscoveryManager discoveryManager = IngredientDiscoveryManager.Instance;

        if (discoveryManager != null && discoveryManager.IsPropertyDiscovered(ingredient, propertyLevel))
            return $"{ingredient.GetPropertyAtLevel(propertyLevel).DisplayName} already discovered";

        return "Ingredient rejected";
    }
}