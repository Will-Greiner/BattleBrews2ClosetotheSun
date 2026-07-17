using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum StirDirection
{
    Clockwise,
    CounterClockwise
}

[RequireComponent(typeof(GrabbableItem))]
[RequireComponent(typeof(Rigidbody))]
public class StirringStick : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform rotationCenter;
    [SerializeField] private CauldronController cauldron;

    [Header("Stirring")]
    [SerializeField] private StirDirection stirDirection = StirDirection.Clockwise;
    [Min(1)] [SerializeField] private int requiredRotations = 3;
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private float maximumAngleChangePerFrame = 45f;

    [Header("Reset")]
    [SerializeField] private float returnDuration = 0.5f;

    private Rigidbody stickRigidbody;
    private GrabController currentHolder;
    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private Vector3 startingOffset;
    private float previousMouseAngle;
    private float currentStickAngle;
    private float accumulatedStirAngle;
    private bool isStirring;
    private Coroutine returnRoutine;

    public bool IsStirring => isStirring;

    public float StirProgress
    {
        get
        {
            float requiredAngle = requiredRotations * 360f;

            if (requiredAngle <= 0f)
                return 0f;

            return Mathf.Clamp01(accumulatedStirAngle / requiredAngle);
        }
    }

    private void Awake()
    {
        stickRigidbody = GetComponent<Rigidbody>();
        startingPosition = transform.position;
        startingRotation = transform.rotation;
        stickRigidbody.isKinematic = true;
        stickRigidbody.useGravity = false;

        if (playerCamera == null)
            Debug.LogError($"{name}: No player camera has been assigned.", this);

        if (rotationCenter == null)
        {
            Debug.LogError($"{name}: No rotation center has been assigned.", this);
            return;
        }

        startingOffset = startingPosition - rotationCenter.position;
    }

    private void Update()
    {
        if (!isStirring || currentHolder == null || Mouse.current == null)
            return;

        UpdateStirring();
    }

    public void BeginStirring(GrabController holder)
    {
        if (holder == null || playerCamera == null || rotationCenter == null)
            return;

        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        currentHolder = holder;
        isStirring = true;
        currentStickAngle = 0f;
        accumulatedStirAngle = 0f;
        previousMouseAngle = GetMouseAngle();
        stickRigidbody.isKinematic = true;
        stickRigidbody.useGravity = false;
    }

    public void EndStirring()
    {
        currentHolder = null;
        isStirring = false;
        accumulatedStirAngle = 0f;

        if (returnRoutine != null)
            StopCoroutine(returnRoutine);

        returnRoutine = StartCoroutine(ReturnToStartRoutine());
    }

    private void UpdateStirring()
    {
        float currentMouseAngle = GetMouseAngle();
        float angleChange = Mathf.DeltaAngle(previousMouseAngle, currentMouseAngle);
        previousMouseAngle = currentMouseAngle;

        if (Mathf.Abs(angleChange) > maximumAngleChangePerFrame)
            return;

        float directionMultiplier = stirDirection == StirDirection.CounterClockwise ? 1f : -1f;
        float movementInRequiredDirection = angleChange * directionMultiplier;

        if (movementInRequiredDirection <= 0f)
            return;

        float acceptedMovement = movementInRequiredDirection * mouseSensitivity;
        currentStickAngle += acceptedMovement * directionMultiplier;
        accumulatedStirAngle += acceptedMovement;

        Quaternion circularRotation = Quaternion.AngleAxis(-currentStickAngle, rotationCenter.up);
        transform.position = rotationCenter.position + circularRotation * startingOffset;
        transform.rotation = circularRotation * startingRotation;

        float requiredAngle = requiredRotations * 360f;

        if (accumulatedStirAngle < requiredAngle)
            return;

        accumulatedStirAngle = 0f;

        bool potionCreated = cauldron != null && cauldron.Stir();
        GrabController holderToRelease = currentHolder;

        Debug.Log(potionCreated ? "Stirring complete: potion created." : "Stirring complete: mixture did not produce a potion.", this);

        if (holderToRelease != null)
            holderToRelease.Release();
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

    private IEnumerator ReturnToStartRoutine()
    {
        Vector3 returnStartPosition = transform.position;
        Quaternion returnStartRotation = transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < returnDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = returnDuration <= 0f ? 1f : Mathf.Clamp01(elapsedTime / returnDuration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            transform.position = Vector3.Lerp(returnStartPosition, startingPosition, easedProgress);
            transform.rotation = Quaternion.Slerp(returnStartRotation, startingRotation, easedProgress);

            yield return null;
        }

        transform.position = startingPosition;
        transform.rotation = startingRotation;
        currentStickAngle = 0f;
        stickRigidbody.isKinematic = true;
        stickRigidbody.useGravity = false;
        returnRoutine = null;
    }
}