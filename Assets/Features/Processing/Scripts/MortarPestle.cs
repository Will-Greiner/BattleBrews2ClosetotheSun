using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(GrabbableItem))]
[RequireComponent(typeof(Rigidbody))]
public class MortarPestle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GrabbableItem grabbableItem;
    [SerializeField] private IngredientProcessingStation station;
    [SerializeField] private Transform restPoint;
    [SerializeField] private Transform strikePoint;

    [Header("Path Movement")]
    [Min(0f)] [SerializeField] private float verticalMouseSensitivity = 0.003f;
    [Min(0f)] [SerializeField] private float movementSpeed = 8f;

    [Header("Variability")]
    [Min(0f)] [SerializeField] private float lateralMouseSensitivity = 0.001f;
    [Min(0f)] [SerializeField] private float maxLateralOffset = 0.04f;
    [Min(0f)] [SerializeField] private float maxTiltDegrees = 6f;

    [Header("Strike")]
    [Range(0f, 1f)] [SerializeField] private float strikeThreshold = 0.95f;
    [Range(0f, 1f)] [SerializeField] private float rearmThreshold = 0.4f;
    [Min(0.01f)] [SerializeField] private float progressPerStrike = 1f;

    [Header("Return")]
    [Min(0.01f)] [SerializeField] private float returnDuration = 0.35f;
    [SerializeField] private AnimationCurve returnCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Rigidbody pestleRigidbody;
    private GrabController currentHolder;
    private Coroutine returnRoutine;
    private float currentPathProgress;
    private float targetPathProgress;
    private float currentLateralOffset;
    private bool strikeArmed = true;

    public bool IsBeingUsed => currentHolder != null;
    public float PathProgress => currentPathProgress;

    private void Awake()
    {
        if (grabbableItem == null)
            grabbableItem = GetComponent<GrabbableItem>();

        pestleRigidbody = GetComponent<Rigidbody>();

        if (station == null)
            Debug.LogError($"{name}: No IngredientProcessingStation has been assigned.", this);

        if (restPoint == null)
            Debug.LogError($"{name}: No rest point has been assigned.", this);

        if (strikePoint == null)
            Debug.LogError($"{name}: No strike point has been assigned.", this);
    }

    private void Start()
    {
        ConfigureKinematicBody();
        SnapToRestPoint();
    }

    private void Update()
    {
        if (currentHolder == null || Mouse.current == null || restPoint == null || strikePoint == null)
            return;

        ReadMouseMovement();
        UpdateConstrainedTransform();
        UpdateStrike();
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

    public void BeginPounding(GrabController holder)
    {
        if (holder == null || restPoint == null || strikePoint == null)
            return;

        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        currentHolder = holder;
        targetPathProgress = currentPathProgress;
        ConfigureKinematicBody();
    }

    public void EndPounding()
    {
        currentHolder = null;

        if (isActiveAndEnabled)
            BeginReturn();
    }

    private void ReadMouseMovement()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        targetPathProgress -= mouseDelta.y * verticalMouseSensitivity;
        targetPathProgress = Mathf.Clamp01(targetPathProgress);

        currentLateralOffset += mouseDelta.x * lateralMouseSensitivity;
        currentLateralOffset = Mathf.Clamp(currentLateralOffset, -maxLateralOffset, maxLateralOffset);
    }

    private void UpdateConstrainedTransform()
    {
        currentPathProgress = Mathf.MoveTowards(currentPathProgress, targetPathProgress, movementSpeed * Time.deltaTime);

        Vector3 path = strikePoint.position - restPoint.position;
        Vector3 pathDirection = path.sqrMagnitude > Mathf.Epsilon ? path.normalized : Vector3.down;
        Vector3 lateralDirection = Vector3.ProjectOnPlane(restPoint.right, pathDirection).normalized;

        if (lateralDirection.sqrMagnitude <= Mathf.Epsilon)
            lateralDirection = Vector3.ProjectOnPlane(restPoint.forward, pathDirection).normalized;

        Vector3 pathPosition = Vector3.Lerp(restPoint.position, strikePoint.position, currentPathProgress);
        Vector3 targetPosition = pathPosition + lateralDirection * currentLateralOffset;
        Quaternion pathRotation = Quaternion.Slerp(restPoint.rotation, strikePoint.rotation, currentPathProgress);

        float normalizedLateralOffset = maxLateralOffset > Mathf.Epsilon ? currentLateralOffset / maxLateralOffset : 0f;
        Quaternion variableTilt = Quaternion.Euler(0f, 0f, -normalizedLateralOffset * maxTiltDegrees);
        Quaternion targetRotation = pathRotation * variableTilt;

        transform.SetPositionAndRotation(targetPosition, targetRotation);
    }

    private void UpdateStrike()
    {
        if (!strikeArmed && currentPathProgress <= rearmThreshold)
            strikeArmed = true;

        if (!strikeArmed || currentPathProgress < strikeThreshold)
            return;

        strikeArmed = false;

        if (station != null && station.HasIngredient)
            station.AddProgress(progressPerStrike);
    }

    private void BeginReturn()
    {
        if (returnRoutine != null)
            StopCoroutine(returnRoutine);

        returnRoutine = StartCoroutine(ReturnRoutine());
    }

    private IEnumerator ReturnRoutine()
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        float startPathProgress = currentPathProgress;
        float elapsed = 0f;

        ConfigureKinematicBody();
        grabbableItem.enabled = false;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / returnDuration);
            float curvedTime = returnCurve.Evaluate(normalizedTime);

            transform.position = Vector3.LerpUnclamped(startPosition, restPoint.position, curvedTime);
            transform.rotation = Quaternion.SlerpUnclamped(startRotation, restPoint.rotation, curvedTime);
            currentPathProgress = Mathf.Lerp(startPathProgress, 0f, curvedTime);
            yield return null;
        }

        SnapToRestPoint();
        grabbableItem.enabled = true;
        returnRoutine = null;
    }

    private void ConfigureKinematicBody()
    {
        pestleRigidbody.linearVelocity = Vector3.zero;
        pestleRigidbody.angularVelocity = Vector3.zero;
        pestleRigidbody.useGravity = false;
        pestleRigidbody.isKinematic = true;
    }

    private void SnapToRestPoint()
    {
        if (restPoint == null)
            return;

        transform.SetPositionAndRotation(restPoint.position, restPoint.rotation);
        currentPathProgress = 0f;
        targetPathProgress = 0f;
        currentLateralOffset = 0f;
        strikeArmed = true;
        ConfigureKinematicBody();
    }
}