using System.Collections;
using UnityEngine;

public class ShopPhaseController : MonoBehaviour
{
    public static ShopPhaseController Instance { get; private set; }

    [Header("Presentation")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform shopFacingPoint;
    [SerializeField] private GameObject shopkeeperRoot;
    [SerializeField] private CanvasGroup shopCanvas;
    [Min(0.01f)] [SerializeField] private float rotationDuration = 0.6f;
    [Min(0.01f)] [SerializeField] private float interfaceFadeDuration = 0.25f;
    [SerializeField] private AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Input")]
    [SerializeField] private GrabController grabController;

    private Coroutine routine;
    private Vector3 gameplayCameraPosition;
    private Quaternion gameplayCameraRotation;
    private bool gameplayCameraPoseSaved;
    private bool subscribed;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (playerCamera == null && grabController != null)
            playerCamera = grabController.PlayerCamera;

        if (playerCamera == null)
            playerCamera = Camera.main;

        SetShopVisible(false);
    }

    private void OnEnable() => Subscribe();
    private void Start() => Subscribe();

    private void OnDisable()
    {
        if (subscribed && GameManager.Instance != null)
            GameManager.Instance.ShopStarted -= HandleShopStarted;
        subscribed = false;

        if (grabController != null)
            grabController.ReleaseInputLock(this);

        CursorManager.Instance?.HideForUI(this);

        RestoreGameplayCameraImmediately();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void CloseShop()
    {
        if (!IsOpen || routine != null) return;
        routine = StartCoroutine(CloseRoutine());
    }

    private void Subscribe()
    {
        if (subscribed || GameManager.Instance == null) return;
        GameManager.Instance.ShopStarted += HandleShopStarted;
        subscribed = true;
    }

    private void HandleShopStarted()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        if (grabController != null) grabController.AcquireInputLock(this);
        SaveGameplayCameraPose();
        if (shopkeeperRoot != null) shopkeeperRoot.SetActive(true);

        if (playerCamera != null && shopFacingPoint != null)
            yield return MoveCamera(playerCamera.transform.position, playerCamera.transform.rotation, shopFacingPoint.position, shopFacingPoint.rotation);
        else if (shopFacingPoint == null)
            Debug.LogError($"{name}: A Shop Facing Point must be assigned for the shop camera transition.", this);

        yield return FadeShop(1f);
        CursorManager.Instance?.ShowForUI(this);
        IsOpen = true;
        TutorialEvents.Report(TutorialTrigger.ShopOpened);
        routine = null;
    }

    private IEnumerator CloseRoutine()
    {
        CursorManager.Instance?.HideForUI(this);
        yield return FadeShop(0f);
        IsOpen = false;

        if (playerCamera != null && gameplayCameraPoseSaved)
            yield return MoveCamera(playerCamera.transform.position, playerCamera.transform.rotation, gameplayCameraPosition, gameplayCameraRotation);

        gameplayCameraPoseSaved = false;

        if (shopkeeperRoot != null) shopkeeperRoot.SetActive(false);
        if (grabController != null) grabController.ReleaseInputLock(this);

        routine = null;
        if (GameManager.Instance != null) GameManager.Instance.CompleteShopPhase();
    }

    private void SaveGameplayCameraPose()
    {
        if (playerCamera == null)
            return;

        gameplayCameraPosition = playerCamera.transform.position;
        gameplayCameraRotation = playerCamera.transform.rotation;
        gameplayCameraPoseSaved = true;
    }

    private void RestoreGameplayCameraImmediately()
    {
        if (playerCamera == null || !gameplayCameraPoseSaved)
            return;

        playerCamera.transform.SetPositionAndRotation(gameplayCameraPosition, gameplayCameraRotation);
        gameplayCameraPoseSaved = false;
    }

    private IEnumerator MoveCamera(Vector3 startPosition, Quaternion startRotation, Vector3 destinationPosition, Quaternion destinationRotation)
    {
        if (playerCamera == null)
            yield break;

        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = rotationCurve.Evaluate(Mathf.Clamp01(elapsed / rotationDuration));
            playerCamera.transform.position = Vector3.LerpUnclamped(startPosition, destinationPosition, t);
            playerCamera.transform.rotation = Quaternion.SlerpUnclamped(startRotation, destinationRotation, t);
            yield return null;
        }

        playerCamera.transform.SetPositionAndRotation(destinationPosition, destinationRotation);
    }

    private void SetShopVisible(bool visible)
    {
        if (shopCanvas == null) return;
        shopCanvas.alpha = visible ? 1f : 0f;
        shopCanvas.interactable = visible;
        shopCanvas.blocksRaycasts = visible;
    }

    private IEnumerator FadeShop(float targetAlpha)
    {
        if (shopCanvas == null)
            yield break;

        float startAlpha = shopCanvas.alpha;
        bool showing = targetAlpha > startAlpha;
        shopCanvas.interactable = false;
        shopCanvas.blocksRaycasts = showing;
        float elapsed = 0f;

        while (elapsed < interfaceFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / interfaceFadeDuration);
            shopCanvas.alpha = Mathf.SmoothStep(startAlpha, targetAlpha, t);
            yield return null;
        }

        shopCanvas.alpha = targetAlpha;
        shopCanvas.interactable = targetAlpha > 0.99f;
        shopCanvas.blocksRaycasts = targetAlpha > 0.01f;
    }
}
