using UnityEngine;
using System.Collections;

public class DiscoveryAltar : MonoBehaviour, IItemReceiver, IItemRejectionFeedback, IItemHoverFeedback
{
    [Header("References")]
    [SerializeField] private Transform sampleSnapPoint;
    [SerializeField] private RuneConstellationMinigame constellationMinigame;
    [SerializeField] private GrabController grabController;
    [SerializeField] private ObjectHighlight altarHighlight;

    [Header("Prompt")]
    [SerializeField] private string receivePrompt = "[LMB] Place Sample";

    [Header("Camera Presentation")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform altarCameraPoint;
    [SerializeField, Min(0.01f)] private float cameraMoveDuration = 1f;
    [SerializeField] private AnimationCurve cameraMoveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Analyzer Presentation")]
    [SerializeField] private Animator analyzerAnimator;
    [SerializeField] private EnchantmentAnimHelper analyzerEffects;
    [SerializeField] private string analyzerAppearTrigger = "Appear";
    [SerializeField] private string analyzerDisappearTrigger = "Dissapear";
    [SerializeField, Min(0f)] private float effectStopDelay = 0.15f;

    [Header("Completion Effects")]
    [SerializeField] private ParticleSystem discoveryParticles;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip discoveryClip;

    private GrabbableItem currentItem;
    private ProcessedIngredientItem currentSample;
    private bool minigameActive;
    private Coroutine presentationRoutine;
    private Vector3 previousCameraPosition;
    private Quaternion previousCameraRotation;
    private bool cameraPositionSaved;

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

    private void Awake()
    {
        if (altarHighlight == null)
            altarHighlight = GetComponent<ObjectHighlight>();
    }

    public void SetItemHover(bool hovering, GrabbableItem item)
    {
        if (altarHighlight == null)
            return;

        if (hovering && item != null)
            altarHighlight.Show();
        else
            altarHighlight.Hide();
    }

    public void ReceiveItem(GrabbableItem item)
    {
        if (!CanReceiveItem(item))
            return;

        currentItem = item;
        currentSample = item.GetComponent<ProcessedIngredientItem>();
        TutorialEvents.Report(TutorialTrigger.AltarSampleInserted, TutorialEvents.GetIngredientId(currentSample.SourceIngredient));
        SnapSample();

        if (presentationRoutine != null)
            StopCoroutine(presentationRoutine);

        presentationRoutine = StartCoroutine(EnterAltarRoutine());
    }

    public string GetReceivePrompt(GrabbableItem item)
    {
        return receivePrompt;
    }

    private void OnDisable()
    {
        altarHighlight?.Hide();

        if (presentationRoutine != null)
        {
            StopCoroutine(presentationRoutine);
            presentationRoutine = null;
        }

        if (minigameActive)
            constellationMinigame?.Cancel();

        if (playerCamera != null && cameraPositionSaved)
            playerCamera.transform.SetPositionAndRotation(previousCameraPosition, previousCameraRotation);

        if (analyzerEffects != null)
            analyzerEffects.StopSwirl();

        currentItem = null;
        currentSample = null;
        minigameActive = false;
        cameraPositionSaved = false;

        if (grabController != null)
            grabController.ReleaseInputLock(this);
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

        Collider[] sampleColliders = currentItem.GetComponentsInChildren<Collider>();

        foreach (Collider sampleCollider in sampleColliders)
            sampleCollider.enabled = false;

        currentItem.enabled = false;
    }

    private void BeginMinigame()
    {
        if (constellationMinigame == null)
        {
            Debug.LogError($"{name}: No constellation minigame has been assigned.", this);
            BeginExitSequence();
            return;
        }

        constellationMinigame.Begin(currentSample.PropertyLevel, CompleteDiscovery);
    }

    private void CompleteDiscovery()
    {
        if (currentSample == null)
        {
            BeginExitSequence();
            return;
        }

        IngredientDiscoveryManager discoveryManager = IngredientDiscoveryManager.Instance;

        if (discoveryManager == null)
        {
            Debug.LogError($"{name}: No IngredientDiscoveryManager exists in the scene.", this);
            BeginExitSequence();
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

        string completionMessage = discovered ? $"{property.DisplayName} discovered for {ingredient.IngredientName}!" : string.Empty;
        BeginExitSequence(completionMessage);
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


    private IEnumerator EnterAltarRoutine()
    {
        minigameActive = true;

        if (grabController != null)
            grabController.AcquireInputLock(this);

        SaveCameraPosition();
        PlayAnalyzerEntrance();

        if (playerCamera != null && altarCameraPoint != null)
            yield return MoveCamera(playerCamera.transform.position, playerCamera.transform.rotation, altarCameraPoint.position, altarCameraPoint.rotation);

        if (effectStopDelay > 0f)
            yield return new WaitForSecondsRealtime(effectStopDelay);

        if (analyzerEffects != null)
            analyzerEffects.StopSwirl();

        presentationRoutine = null;
        BeginMinigame();
    }


    private void SaveCameraPosition()
    {
        if (playerCamera == null)
            return;

        previousCameraPosition = playerCamera.transform.position;
        previousCameraRotation = playerCamera.transform.rotation;
        cameraPositionSaved = true;
    }

    private void PlayAnalyzerEntrance()
    {
        if (analyzerAnimator != null)
        {
            analyzerAnimator.ResetTrigger(analyzerDisappearTrigger);
            analyzerAnimator.SetTrigger(analyzerAppearTrigger);
        }

        if (analyzerEffects != null)
            analyzerEffects.PlaySwirl();
    }


    private IEnumerator MoveCamera(Vector3 startPosition, Quaternion startRotation, Vector3 destinationPosition, Quaternion destinationRotation)
    {
        if (playerCamera == null)
            yield break;

        float elapsed = 0f;

        while (elapsed < cameraMoveDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / cameraMoveDuration);
            float curvedTime = cameraMoveCurve.Evaluate(normalizedTime);
            playerCamera.transform.position = Vector3.LerpUnclamped(startPosition, destinationPosition, curvedTime);
            playerCamera.transform.rotation = Quaternion.SlerpUnclamped(startRotation, destinationRotation, curvedTime);
            yield return null;
        }

        playerCamera.transform.SetPositionAndRotation(destinationPosition, destinationRotation);
    }


    private void BeginExitSequence(string completionMessage = "")
    {
        if (presentationRoutine != null)
            StopCoroutine(presentationRoutine);

        presentationRoutine = StartCoroutine(ExitAltarRoutine(completionMessage));
    }


    private IEnumerator ExitAltarRoutine(string completionMessage)
    {
        PlayAnalyzerExit();

        if (playerCamera != null && cameraPositionSaved)
        {
            Vector3 startPosition = playerCamera.transform.position;
            Quaternion startRotation = playerCamera.transform.rotation;
            yield return MoveCamera(startPosition, startRotation, previousCameraPosition, previousCameraRotation);
        }

        if (effectStopDelay > 0f)
            yield return new WaitForSecondsRealtime(effectStopDelay);

        if (analyzerEffects != null)
            analyzerEffects.StopSwirl();

        cameraPositionSaved = false;
        minigameActive = false;
        presentationRoutine = null;

        if (grabController != null)
            grabController.ReleaseInputLock(this);

        if (!string.IsNullOrWhiteSpace(completionMessage) && grabController != null)
            grabController.ShowTemporaryPrompt(completionMessage, 2.5f);
    }


    private void PlayAnalyzerExit()
    {
        if (analyzerAnimator != null)
        {
            analyzerAnimator.ResetTrigger(analyzerAppearTrigger);
            analyzerAnimator.SetTrigger(analyzerDisappearTrigger);
        }

        if (analyzerEffects != null)
            analyzerEffects.PlaySwirl();
    }
}
