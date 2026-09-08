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

    private Rigidbody handleRigidbody;
    private GrabController currentHolder;
    private float targetSpeed;
    private float currentSpeed;

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
            float mouseDelta = Mouse.current.delta.ReadValue().x;

            // Direct movement while held: the table follows the hand immediately.
            float angleThisFrame = -mouseDelta * mouseSensitivity;

            angleThisFrame = Mathf.Clamp(
                angleThisFrame,
                -maximumDegreesPerFrame,
                maximumDegreesPerFrame);

            RotateTable(angleThisFrame);

            // Remember the current movement for a subtle release coast.
            currentSpeed = Time.unscaledDeltaTime > 0f
                ? angleThisFrame / Time.unscaledDeltaTime
                : 0f;

            return;
        }

        // Only coast when the player has released the handle.
        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            0f,
            releaseDrag * Time.deltaTime);

        RotateTable(currentSpeed * Time.deltaTime);
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
    }

    public void EndTurning()
    {
        currentHolder = null;
    }
}