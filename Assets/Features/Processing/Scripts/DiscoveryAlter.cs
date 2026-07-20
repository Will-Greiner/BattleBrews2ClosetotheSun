using UnityEngine;

public class DiscoveryAltar : MonoBehaviour, IItemReceiver, IItemRejectionFeedback
{
    [Header("References")]
    [SerializeField] private Transform sampleSnapPoint;
    [SerializeField] private RuneConstellationMinigame constellationMinigame;
    [SerializeField] private GrabController grabController;

    [Header("Prompt")]
    [SerializeField] private string receivePrompt = "[LMB] Place Sample";

    [Header("Completion Effects")]
    [SerializeField] private ParticleSystem discoveryParticles;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip discoveryClip;

    private GrabbableItem currentItem;
    private ProcessedIngredientItem currentSample;
    private bool minigameActive;

    public bool CanReceiveItem(GrabbableItem item)
    {
        if (currentItem != null || minigameActive || item == null)
            return false;

        ProcessedIngredientItem sample = item.GetComponent<ProcessedIngredientItem>();

        if (sample == null || !sample.IsInitialized)
            return false;

        IngredientDiscoveryManager discoveryManager = IngredientDiscoveryManager.Instance;

        if (discoveryManager == null)
            return false;

        return !discoveryManager.IsPropertyDiscovered(sample.SourceIngredient, sample.PropertyLevel);
    }

    public void ReceiveItem(GrabbableItem item)
    {
        if (!CanReceiveItem(item))
            return;

        currentItem = item;
        currentSample = item.GetComponent<ProcessedIngredientItem>();

        SnapSample();
        BeginMinigame();
    }

    public string GetReceivePrompt(GrabbableItem item)
    {
        return receivePrompt;
    }

    private void OnDisable()
    {
        if (minigameActive)
        {
            constellationMinigame?.Cancel();
            minigameActive = false;

            if (grabController != null)
                grabController.SetInputEnabled(true);
        }
    }

    private void SnapSample()
    {
        if (currentItem == null)
            return;

        Rigidbody sampleRigidbody = currentItem.Rigidbody;

        if (sampleRigidbody != null)
        {
            sampleRigidbody.linearVelocity = Vector3.zero;
            sampleRigidbody.angularVelocity = Vector3.zero;
            sampleRigidbody.useGravity = false;
            sampleRigidbody.isKinematic = true;
        }

        if (sampleSnapPoint != null)
        {
            currentItem.transform.SetParent(sampleSnapPoint);
            currentItem.transform.SetPositionAndRotation(sampleSnapPoint.position, sampleSnapPoint.rotation);
        }

        currentItem.enabled = false;
    }

    private void BeginMinigame()
    {
        if (constellationMinigame == null)
        {
            Debug.LogError($"{name}: No constellation minigame has been assigned.", this);
            return;
        }

        minigameActive = true;

        if (grabController != null)
            grabController.SetInputEnabled(false);

        constellationMinigame.Begin(currentSample.PropertyLevel, CompleteDiscovery);
    }

    private void CompleteDiscovery()
    {
        if (currentSample == null)
        {
            FinishMinigame();
            return;
        }

        IngredientDiscoveryManager discoveryManager = IngredientDiscoveryManager.Instance;

        if (discoveryManager == null)
        {
            Debug.LogError($"{name}: No IngredientDiscoveryManager exists in the scene.", this);
            FinishMinigame();
            return;
        }

        IngredientData ingredient = currentSample.SourceIngredient;
        ItemPropertyData property = currentSample.Property;
        int propertyLevel = currentSample.PropertyLevel;

        bool discovered = discoveryManager.DiscoverProperty(ingredient, propertyLevel);

        if (discovered)
            PlayDiscoveryEffects();

        if (currentItem != null)
            Destroy(currentItem.gameObject);

        currentItem = null;
        currentSample = null;
        FinishMinigame();

        if (discovered && grabController != null)
            grabController.ShowTemporaryPrompt($"{property.DisplayName} discovered for {ingredient.IngredientName}!", 2.5f);
    }

    private void FinishMinigame()
    {
        minigameActive = false;

        if (grabController != null)
            grabController.SetInputEnabled(true);
    }

    private void PlayDiscoveryEffects()
    {
        if (discoveryParticles != null)
            discoveryParticles.Play();

        if (audioSource != null && discoveryClip != null)
            audioSource.PlayOneShot(discoveryClip);
    }

    public void ShowRejectionFeedback(GrabbableItem item)
    {
        if (grabController == null)
            return;

        if (currentItem != null || minigameActive)
        {
            grabController.ShowTemporaryPrompt("Altar already occupied");
            return;
        }

        ProcessedIngredientItem sample = item != null ? item.GetComponent<ProcessedIngredientItem>() : null;

        if (sample == null)
        {
            grabController.ShowTemporaryPrompt("The altar requires a processed sample");
            return;
        }

        if (!sample.IsInitialized)
        {
            grabController.ShowTemporaryPrompt("This sample is invalid");
            return;
        }

        IngredientDiscoveryManager discoveryManager = IngredientDiscoveryManager.Instance;

        if (discoveryManager != null && discoveryManager.IsPropertyDiscovered(sample.SourceIngredient, sample.PropertyLevel))
        {
            grabController.ShowTemporaryPrompt($"{sample.Property.DisplayName} already discovered");
            return;
        }

        grabController.ShowTemporaryPrompt("The altar rejected this sample");
    }
}