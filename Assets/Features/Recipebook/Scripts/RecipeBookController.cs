using System.Collections;
using UnityEngine;

public class RecipeBookController : MonoBehaviour, IHandInteractable
{
    [Header("References")]
    [SerializeField] private Transform bookViewPoint;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject leftPageCanvas;
    [SerializeField] private GameObject rightPageCanvas;
    [SerializeField] private RecipeBookContentUI contentUI;

    [Header("Player Controls")]
    [Tooltip("Assign HandController and the script responsible for camera movement.")]
    [SerializeField] private MonoBehaviour[] controlsToDisableWhileOpen;
    [Tooltip("Assign only the visible hand model, not the player or GrabController root.")]
    [SerializeField] private GameObject handVisualRoot;

    [Header("Animator")]
    [SerializeField] private string openTrigger = "Open";
    [SerializeField] private string closeTrigger = "Close";
    [Min(0f)] [SerializeField] private float openingDuration = 0.5f;
    [Min(0f)] [SerializeField] private float closingDuration = 0.5f;

    [Header("Movement")]
    [Min(0.01f)] [SerializeField] private float moveToViewDuration = 0.6f;
    [Min(0.01f)] [SerializeField] private float returnDuration = 0.6f;
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Prompt")]
    [SerializeField] private string openPrompt = "Read Recipe Book";

    private GrabController activeGrabController;
    private Coroutine transitionRoutine;
    private Transform pedestalParent;
    private Vector3 pedestalLocalPosition;
    private Quaternion pedestalLocalRotation;
    private Vector3 pedestalLocalScale;
    private bool[] previousControlStates;
    private bool handVisualWasActive;
    private bool isOpen;
    private bool isTransitioning;

    public bool IsOpen => isOpen;
    public bool IsTransitioning => isTransitioning;

    private void Awake()
    {
        pedestalParent = transform.parent;
        pedestalLocalPosition = transform.localPosition;
        pedestalLocalRotation = transform.localRotation;
        pedestalLocalScale = transform.localScale;

        if (contentUI == null)
            contentUI = GetComponent<RecipeBookContentUI>();

        SetPageCanvasesVisible(false);

        if (animator == null)
            Debug.LogError($"{name}: No Animator has been assigned.", this);

        if (bookViewPoint == null)
            Debug.LogError($"{name}: No book view point has been assigned.", this);

        if (contentUI == null)
            Debug.LogError($"{name}: No RecipeBookContentUI has been assigned.", this);
    }

    private void OnDisable()
    {
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        if (activeGrabController != null)
            RestorePlayerControls();

        activeGrabController = null;
        isOpen = false;
        isTransitioning = false;
    }

    public bool CanInteract(GrabController grabController)
    {
        return !isOpen && !isTransitioning && grabController != null && !grabController.IsHoldingItem;
    }

    public void Interact(GrabController grabController)
    {
        if (CanInteract(grabController))
            OpenBook(grabController);
    }

    public string GetInteractionPrompt(GrabController grabController)
    {
        return CanInteract(grabController) ? openPrompt : string.Empty;
    }

    public void OpenBook(GrabController grabController)
    {
        if (!CanInteract(grabController) || bookViewPoint == null || animator == null)
            return;

        activeGrabController = grabController;

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(OpenBookRoutine());
    }

    public void CloseBook()
    {
        if (!isOpen || isTransitioning || animator == null)
            return;

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(CloseBookRoutine());
    }

    private IEnumerator OpenBookRoutine()
    {
        isTransitioning = true;
        DisablePlayerControls();

        yield return MoveBook(transform.position, transform.rotation, bookViewPoint.position, bookViewPoint.rotation, moveToViewDuration);

        transform.SetParent(bookViewPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        animator.ResetTrigger(closeTrigger);
        animator.SetTrigger(openTrigger);

        if (openingDuration > 0f)
            yield return new WaitForSeconds(openingDuration);

        SetPageCanvasesVisible(true);

        if (contentUI != null)
            contentUI.OpenContent();

        isOpen = true;
        isTransitioning = false;
        transitionRoutine = null;
    }

    private IEnumerator CloseBookRoutine()
    {
        isTransitioning = true;
        SetPageCanvasesVisible(false);

        animator.ResetTrigger(openTrigger);
        animator.SetTrigger(closeTrigger);

        if (closingDuration > 0f)
            yield return new WaitForSeconds(closingDuration);

        transform.SetParent(pedestalParent);

        Vector3 targetPosition = pedestalParent != null ? pedestalParent.TransformPoint(pedestalLocalPosition) : pedestalLocalPosition;
        Quaternion targetRotation = pedestalParent != null ? pedestalParent.rotation * pedestalLocalRotation : pedestalLocalRotation;

        yield return MoveBook(transform.position, transform.rotation, targetPosition, targetRotation, returnDuration);

        transform.SetParent(pedestalParent);
        transform.localPosition = pedestalLocalPosition;
        transform.localRotation = pedestalLocalRotation;
        transform.localScale = pedestalLocalScale;

        isOpen = false;
        isTransitioning = false;
        RestorePlayerControls();

        activeGrabController = null;
        transitionRoutine = null;
    }

    private void DisablePlayerControls()
    {
        if (activeGrabController != null)
            activeGrabController.AcquireInputLock(this);

        previousControlStates = new bool[controlsToDisableWhileOpen.Length];

        for (int i = 0; i < controlsToDisableWhileOpen.Length; i++)
        {
            MonoBehaviour control = controlsToDisableWhileOpen[i];

            if (control == null)
                continue;

            previousControlStates[i] = control.enabled;
            control.enabled = false;
        }

        if (handVisualRoot != null)
        {
            handVisualWasActive = handVisualRoot.activeSelf;
            handVisualRoot.SetActive(false);
        }
    }

    private void RestorePlayerControls()
    {
        if (previousControlStates != null)
        {
            for (int i = 0; i < controlsToDisableWhileOpen.Length; i++)
            {
                MonoBehaviour control = controlsToDisableWhileOpen[i];

                if (control != null && i < previousControlStates.Length)
                    control.enabled = previousControlStates[i];
            }
        }

        if (handVisualRoot != null)
            handVisualRoot.SetActive(handVisualWasActive);

        if (activeGrabController != null)
            activeGrabController.ReleaseInputLock(this);
    }

    private IEnumerator MoveBook(Vector3 startPosition, Quaternion startRotation, Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
            float curvedTime = movementCurve.Evaluate(normalizedTime);
            transform.position = Vector3.LerpUnclamped(startPosition, targetPosition, curvedTime);
            transform.rotation = Quaternion.SlerpUnclamped(startRotation, targetRotation, curvedTime);
            yield return null;
        }

        transform.SetPositionAndRotation(targetPosition, targetRotation);
    }

    private void SetPageCanvasesVisible(bool visible)
    {
        if (leftPageCanvas != null)
            leftPageCanvas.SetActive(visible);

        if (rightPageCanvas != null)
            rightPageCanvas.SetActive(visible);
    }
}
