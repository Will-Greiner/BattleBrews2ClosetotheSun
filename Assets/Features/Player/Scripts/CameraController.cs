using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private GrabController grabController;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 45f;
    [SerializeField] private float rotationLimit = 60f;
    [SerializeField] private float acceleration = 8f;

    [Header("Screen Edge")]
    [SerializeField, Range(0.01f, 0.5f)] private float edgeSize = 0.15f;
    [SerializeField] private float edgePower = 1.5f;

    private float currentYaw;
    private float currentSpeed;
    private float targetSpeed;
    private Transform playerRoot;
    private Quaternion forwardPlayerRotation;
    private Quaternion forwardPivotRotation;

    private void Awake()
    {
        playerRoot = transform.parent;
        forwardPlayerRotation = playerRoot != null ? playerRoot.rotation : Quaternion.identity;
        forwardPivotRotation = transform.localRotation;
    }

    private void Start()
    {
        if (grabController == null)
            grabController = FindFirstObjectByType<GrabController>();

        currentYaw = NormalizeAngle(transform.localEulerAngles.y);
    }

    private void Update()
    {
        bool inputLocked = grabController != null && !grabController.InputEnabled;

        bool processingControlIsGrabbed = IsProcessingControlGrabbed();

        if (inputLocked || processingControlIsGrabbed)
        {
            StopCameraMovement();
            return;
        }

        if (Mouse.current == null || Screen.width <= 0)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        float normalizedX = mousePosition.x / Screen.width;

        targetSpeed = CalculateTargetSpeed(normalizedX) * GameSettings.CameraSpeedMultiplier;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, 1f - Mathf.Exp(-acceleration * Time.deltaTime));

        currentYaw += currentSpeed * Time.deltaTime;
        currentYaw = Mathf.Clamp(currentYaw, -rotationLimit, rotationLimit);

        transform.localRotation = Quaternion.Euler(0f, currentYaw, 0f);
    }

    private bool IsProcessingControlGrabbed()
    {
        if (grabController == null || grabController.HeldItem == null)
            return false;

        GrabbableItem item = grabController.HeldItem;
        return item.GetComponent<MortarPestle>() != null || item.GetComponent<BurnerBellows>() != null || item.GetComponent<PulverizerCrank>() != null || item.GetComponent<ProcessingTableHandle>() != null;
    }

    private float CalculateTargetSpeed(float normalizedX)
    {
        if (normalizedX < edgeSize)
        {
            float amount = 1f - normalizedX / edgeSize;
            amount = Mathf.Pow(Mathf.Clamp01(amount), edgePower);
            return -rotationSpeed * amount;
        }

        if (normalizedX > 1f - edgeSize)
        {
            float amount = (normalizedX - (1f - edgeSize)) / edgeSize;
            amount = Mathf.Pow(Mathf.Clamp01(amount), edgePower);
            return rotationSpeed * amount;
        }

        return 0f;
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }

    private void StopCameraMovement()
    {
        targetSpeed = 0f;
        currentSpeed = 0f;
        currentYaw = NormalizeAngle(transform.localEulerAngles.y);
    }

    public IEnumerator FaceForward(float duration)
    {
        StopCameraMovement();
        Quaternion startingPivotRotation = transform.localRotation;
        Quaternion startingPlayerRotation = playerRoot != null ? playerRoot.rotation : Quaternion.identity;

        if (duration <= 0f)
        {
            SetForwardRotation();
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / duration);
            float smoothTime = Mathf.SmoothStep(0f, 1f, normalizedTime);
            transform.localRotation = Quaternion.SlerpUnclamped(startingPivotRotation, forwardPivotRotation, smoothTime);

            if (playerRoot != null)
                playerRoot.rotation = Quaternion.SlerpUnclamped(startingPlayerRotation, forwardPlayerRotation, smoothTime);

            currentYaw = NormalizeAngle(transform.localEulerAngles.y);
            yield return null;
        }

        SetForwardRotation();
        StopCameraMovement();
    }

    private void SetForwardRotation()
    {
        if (playerRoot != null)
            playerRoot.rotation = forwardPlayerRotation;

        transform.localRotation = forwardPivotRotation;
        currentYaw = NormalizeAngle(forwardPivotRotation.eulerAngles.y);
    }
}
