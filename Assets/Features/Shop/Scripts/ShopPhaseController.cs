using System.Collections;
using UnityEngine;

public class ShopPhaseController : MonoBehaviour
{
    public static ShopPhaseController Instance { get; private set; }

    [Header("Presentation")]
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Transform shopFacingPoint;
    [SerializeField] private GameObject shopkeeperRoot;
    [SerializeField] private CanvasGroup shopCanvas;
    [Min(0.01f)] [SerializeField] private float rotationDuration = 0.6f;
    [Min(0.01f)] [SerializeField] private float interfaceFadeDuration = 0.25f;
    [SerializeField] private AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Input")]
    [SerializeField] private GrabController grabController;

    private Coroutine routine;
    private Quaternion playAreaRotation;
    private bool subscribed;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
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
        if (playerRoot != null) playAreaRotation = playerRoot.rotation;
        if (shopkeeperRoot != null) shopkeeperRoot.SetActive(true);

        Quaternion shopRotation = GetShopRotation();
        yield return RotatePlayer(shopRotation);
        yield return FadeShop(1f);
        IsOpen = true;
        routine = null;
    }

    private IEnumerator CloseRoutine()
    {
        yield return FadeShop(0f);
        IsOpen = false;
        yield return RotatePlayer(playAreaRotation);

        if (shopkeeperRoot != null) shopkeeperRoot.SetActive(false);
        if (grabController != null) grabController.ReleaseInputLock(this);

        routine = null;
        if (GameManager.Instance != null) GameManager.Instance.CompleteShopPhase();
    }

    private Quaternion GetShopRotation()
    {
        if (playerRoot == null) return Quaternion.identity;

        if (shopFacingPoint == null)
            return playerRoot.rotation * Quaternion.Euler(0f, 180f, 0f);

        Vector3 direction = shopFacingPoint.position - playerRoot.position;
        direction.y = 0f;
        return direction.sqrMagnitude > 0.001f ? Quaternion.LookRotation(direction.normalized, Vector3.up) : playerRoot.rotation;
    }

    private IEnumerator RotatePlayer(Quaternion target)
    {
        if (playerRoot == null) yield break;
        Quaternion start = playerRoot.rotation;
        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = rotationCurve.Evaluate(Mathf.Clamp01(elapsed / rotationDuration));
            playerRoot.rotation = Quaternion.SlerpUnclamped(start, target, t);
            yield return null;
        }

        playerRoot.rotation = target;
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
