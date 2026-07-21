using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum CrankDirection
{
    Clockwise,
    CounterClockwise
}

[RequireComponent(typeof(GrabbableItem))]
[RequireComponent(typeof(Rigidbody))]
public class PulverizerCrank : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private IngredientProcessingStation station;
    [SerializeField] private Transform rotationCenter;
    [SerializeField] private Camera playerCamera;

    [Header("Rotation")]
    [SerializeField] private CrankDirection requiredDirection = CrankDirection.Clockwise;
    [SerializeField] private Vector3 localRotationAxis = Vector3.forward;
    [Min(0f)] [SerializeField] private float mouseSensitivity = 1f;
    [Min(0f)] [SerializeField] private float maximumAngleChangePerFrame = 45f;

    [Header("Resistance")]
    [Tooltip("Controls accepted crank movement based on processing progress. X is progress and Y is movement multiplier.")]
    [SerializeField] private AnimationCurve resistanceCurve = AnimationCurve.Linear(0f, 1f, 1f, 0.35f);
    [Range(0.05f, 1f)] [SerializeField] private float minimumMovementMultiplier = 0.2f;

    [Header("Return")]
    [Min(0.01f)] [SerializeField] private float returnDuration = 0.35f;
    [SerializeField] private AnimationCurve returnCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip rotationClip;
    [Min(0f)] [SerializeField] private float rotationSoundInterval = 0.15f;

    private GrabbableItem grabbableItem;
    private Rigidbody crankRigidbody;
    private GrabController currentHolder;
    private Coroutine returnRoutine;
    private Quaternion startingLocalRotation;
    private float currentVisualAngle;
    private float previousMouseAngle;
    private float nextRotationSoundTime;

    public bool IsBeingUsed => currentHolder != null;

    private void Awake()
    {
        grabbableItem = GetComponent<GrabbableItem>();
        crankRigidbody = GetComponent<Rigidbody>();
        startingLocalRotation = transform.localRotation;

        if (station == null)
            Debug.LogError($"{name}: No IngredientProcessingStation has been assigned.", this);

        if (rotationCenter == null)
            Debug.LogError($"{name}: No crank rotation center has been assigned.", this);

        if (playerCamera == null)
            Debug.LogError($"{name}: No player camera has been assigned.", this);
    }

    private void Start()
    {
        ConfigureKinematicBody();
    }

    private void Update()
    {
        if (currentHolder == null || Mouse.current == null || rotationCenter == null || playerCamera == null)
            return;

        UpdateCranking();
    }

    private void OnDisable()
    {
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        currentHolder = null;
    }

    public void BeginCranking(GrabController holder)
    {
        if (holder == null || rotationCenter == null)
            return;

        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        currentHolder = holder;

        if (playerCamera == null)
            playerCamera = holder.PlayerCamera;

        previousMouseAngle = GetMouseAngle();
        ConfigureKinematicBody();
    }

    public void EndCranking()
    {
        currentHolder = null;

        if (isActiveAndEnabled)
            BeginReturn();
    }

    private void UpdateCranking()
    {
        float currentMouseAngle = GetMouseAngle();
        float angleChange = Mathf.DeltaAngle(previousMouseAngle, currentMouseAngle);
        previousMouseAngle = currentMouseAngle;

        if (Mathf.Abs(angleChange) > maximumAngleChangePerFrame)
            return;

        float directionMultiplier = requiredDirection == CrankDirection.CounterClockwise ? 1f : -1f;
        float movementInRequiredDirection = angleChange * directionMultiplier;

        if (movementInRequiredDirection <= 0f)
            return;

        float resistanceMultiplier = GetResistanceMultiplier();
        float acceptedMovement = movementInRequiredDirection * mouseSensitivity * resistanceMultiplier;
        currentVisualAngle += acceptedMovement * directionMultiplier;
        ApplyRotation();

        if (station != null && station.HasIngredient)
            station.AddProgress(acceptedMovement / 360f);

        PlayRotationFeedback();
    }

    private void ApplyRotation()
    {
        Vector3 rotationAxis = localRotationAxis.sqrMagnitude > Mathf.Epsilon ? localRotationAxis.normalized : Vector3.forward;
        transform.localRotation = startingLocalRotation * Quaternion.AngleAxis(currentVisualAngle, rotationAxis);
    }

    private float GetMouseAngle()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 centerScreenPosition = playerCamera.WorldToScreenPoint(rotationCenter.position);
        Vector2 directionFromCenter = mousePosition - new Vector2(centerScreenPosition.x, centerScreenPosition.y);

        if (directionFromCenter.sqrMagnitude < 1f)
            return previousMouseAngle;

        return Mathf.Atan2(directionFromCenter.y, directionFromCenter.x) * Mathf.Rad2Deg;
    }

    private void BeginReturn()
    {
        if (returnRoutine != null)
            StopCoroutine(returnRoutine);

        returnRoutine = StartCoroutine(ReturnRoutine());
    }

    private IEnumerator ReturnRoutine()
    {
        Quaternion returnStartRotation = transform.localRotation;
        float elapsed = 0f;

        grabbableItem.enabled = false;
        ConfigureKinematicBody();

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / returnDuration);
            float curvedTime = returnCurve.Evaluate(normalizedTime);
            transform.localRotation = Quaternion.SlerpUnclamped(returnStartRotation, startingLocalRotation, curvedTime);
            yield return null;
        }

        transform.localRotation = startingLocalRotation;
        currentVisualAngle = 0f;
        grabbableItem.enabled = true;
        returnRoutine = null;
    }

    private void PlayRotationFeedback()
    {
        if (audioSource == null || rotationClip == null || Time.time < nextRotationSoundTime)
            return;

        audioSource.PlayOneShot(rotationClip);
        nextRotationSoundTime = Time.time + rotationSoundInterval;
    }

    private void ConfigureKinematicBody()
    {
        crankRigidbody.linearVelocity = Vector3.zero;
        crankRigidbody.angularVelocity = Vector3.zero;
        crankRigidbody.useGravity = false;
        crankRigidbody.isKinematic = true;
    }

    private float GetResistanceMultiplier()
    {
        if (station == null || !station.HasIngredient)
            return 1f;

        float processingProgress = station.NormalizedProgress;
        float curveMultiplier = resistanceCurve.Evaluate(processingProgress);
        return Mathf.Clamp(curveMultiplier, minimumMovementMultiplier, 1f);
    }
}