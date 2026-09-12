using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(GrabbableItem))]
[RequireComponent(typeof(Rigidbody))]
public class ProcessingTableHandle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform carouselPivot;

    [Header("Rotation")]
    [SerializeField] private Vector3 localRotationAxis = Vector3.up;
    [SerializeField, Min(0f)] private float mouseSensitivity = 0.6f;
    [SerializeField, Min(0f)] private float maximumDegreesPerFrame = 30f;

    [Header("Feel")]
    [SerializeField] private float releaseDrag = 300f;

    [Header("Smoothing")]
    [SerializeField, Min(0.01f)] private float inputSharpness = 20f;

    private Rigidbody handleRigidbody;
    private GrabController currentHolder;
    private float currentSpeed;
    private float smoothedMouseDelta;

    public bool IsBeingUsed => currentHolder != null;

    private void Awake()
    {
        handleRigidbody = GetComponent<Rigidbody>();

        handleRigidbody.isKinematic = true;
        handleRigidbody.useGravity = false;

        if (carouselPivot == null)
        {
            Debug.LogError(
                $"{name}: No carousel pivot has been assigned.",
                this);
        }
    }

    private void Update()
    {
        if (carouselPivot == null)
            return;

        if (currentHolder != null && Mouse.current != null)
        {
            float deltaTime = Time.unscaledDeltaTime;

            if (deltaTime <= Mathf.Epsilon)
                return;

            float mouseDelta = Mouse.current.delta.ReadValue().x;
            float smoothing = 1f - Mathf.Exp(-inputSharpness * deltaTime);
            smoothedMouseDelta = Mathf.Lerp(smoothedMouseDelta, mouseDelta, smoothing);

            float angleThisFrame = Mathf.Clamp(-smoothedMouseDelta * mouseSensitivity, -maximumDegreesPerFrame, maximumDegreesPerFrame);
            RotateTable(angleThisFrame);
            currentSpeed = angleThisFrame / deltaTime;
            return;
        }

        float releaseDeltaTime = Time.unscaledDeltaTime;
        currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, releaseDrag * releaseDeltaTime);
        RotateTable(currentSpeed * releaseDeltaTime);
    }

    private void RotateTable(float degrees)
    {
        if (Mathf.Abs(degrees) < 0.001f)
            return;

        Vector3 axis = localRotationAxis.sqrMagnitude > Mathf.Epsilon
            ? localRotationAxis.normalized
            : Vector3.up;

        carouselPivot.Rotate(axis, degrees, Space.Self);
    }

    public void BeginTurning(GrabController holder)
    {
        if (holder == null)
            return;

        currentHolder = holder;
        currentSpeed = 0f;
        smoothedMouseDelta = 0f;
    }

    public void EndTurning()
    {
        currentHolder = null;
        smoothedMouseDelta = 0f;
    }
}
