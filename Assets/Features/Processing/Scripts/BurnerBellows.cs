using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(GrabbableItem))]
[RequireComponent(typeof(Rigidbody))]
public class BurnerBellows : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GrabbableItem grabbableItem;
    [SerializeField] private IngredientProcessingStation station;

    [Header("Rotation")]
    [Tooltip("Local rotation axis around the bellows hinge.")]
    [SerializeField] private Vector3 localRotationAxis = Vector3.right;
    [SerializeField] private float compressionAngle = 35f;
    [SerializeField] private AnimationCurve compressionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Input")]
    [Min(0f)] [SerializeField] private float mouseSensitivity = 0.003f;
    [Min(0f)] [SerializeField] private float movementSpeed = 8f;

    [Header("Compression")]
    [Range(0f, 1f)] [SerializeField] private float compressionThreshold = 0.95f;
    [Range(0f, 1f)] [SerializeField] private float rearmThreshold = 0.25f;
    [Min(0.01f)] [SerializeField] private float progressPerCompression = 1f;

    [Header("Return")]
    [Min(0.01f)] [SerializeField] private float returnDuration = 0.3f;
    [SerializeField] private AnimationCurve returnCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Feedback")]
    [SerializeField] private ParticleSystem flameBurst;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip compressionClip;

    private Rigidbody bellowsRigidbody;
    private GrabController currentHolder;
    private Coroutine returnRoutine;
    private Quaternion restLocalRotation;
    private float currentCompression;
    private float targetCompression;
    private bool compressionArmed = true;

    public bool IsBeingUsed => currentHolder != null;
    public float Compression => currentCompression;

    private void Awake()
    {
        if (grabbableItem == null)
            grabbableItem = GetComponent<GrabbableItem>();

        bellowsRigidbody = GetComponent<Rigidbody>();
        restLocalRotation = transform.localRotation;

        if (station == null)
            Debug.LogError($"{name}: No IngredientProcessingStation has been assigned.", this);
    }

    private void Start()
    {
        ConfigureKinematicBody();
        SnapToRestRotation();
    }

    private void Update()
    {
        if (currentHolder == null || Mouse.current == null)
            return;

        ReadMouseMovement();
        UpdateRotation();
        UpdateCompression();
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

    public void BeginSqueezing(GrabController holder)
    {
        if (holder == null)
            return;

        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        currentHolder = holder;
        targetCompression = currentCompression;
        ConfigureKinematicBody();
    }

    public void EndSqueezing()
    {
        currentHolder = null;

        if (isActiveAndEnabled)
            BeginReturn();
    }

    private void ReadMouseMovement()
    {
        float mouseMovement = Mouse.current.delta.ReadValue().y;
        targetCompression -= mouseMovement * mouseSensitivity;
        targetCompression = Mathf.Clamp01(targetCompression);
    }

    private void UpdateRotation()
    {
        currentCompression = Mathf.MoveTowards(currentCompression, targetCompression, movementSpeed * Time.deltaTime);
        ApplyCompressionRotation(currentCompression);
    }

    private void ApplyCompressionRotation(float compression)
    {
        Vector3 rotationAxis = localRotationAxis.sqrMagnitude > Mathf.Epsilon ? localRotationAxis.normalized : Vector3.right;
        float curvedCompression = compressionCurve.Evaluate(Mathf.Clamp01(compression));
        Quaternion compressedOffset = Quaternion.AngleAxis(compressionAngle * curvedCompression, rotationAxis);
        transform.localRotation = restLocalRotation * compressedOffset;
    }

    private void UpdateCompression()
    {
        if (!compressionArmed && currentCompression <= rearmThreshold)
            compressionArmed = true;

        if (!compressionArmed || currentCompression < compressionThreshold)
            return;

        compressionArmed = false;
        PlayCompressionFeedback();

        if (station != null && station.HasIngredient)
            station.AddProgress(progressPerCompression);
    }

    private void BeginReturn()
    {
        if (returnRoutine != null)
            StopCoroutine(returnRoutine);

        returnRoutine = StartCoroutine(ReturnRoutine());
    }

    private IEnumerator ReturnRoutine()
    {
        float startCompression = currentCompression;
        float elapsed = 0f;

        ConfigureKinematicBody();
        grabbableItem.enabled = false;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / returnDuration);
            float curvedTime = returnCurve.Evaluate(normalizedTime);

            currentCompression = Mathf.Lerp(startCompression, 0f, curvedTime);
            targetCompression = currentCompression;
            ApplyCompressionRotation(currentCompression);
            yield return null;
        }

        SnapToRestRotation();
        grabbableItem.enabled = true;
        returnRoutine = null;
    }

    private void PlayCompressionFeedback()
    {
        if (flameBurst != null)
            flameBurst.Play();

        if (audioSource != null && compressionClip != null)
            audioSource.PlayOneShot(compressionClip);
    }

    private void ConfigureKinematicBody()
    {
        bellowsRigidbody.linearVelocity = Vector3.zero;
        bellowsRigidbody.angularVelocity = Vector3.zero;
        bellowsRigidbody.useGravity = false;
        bellowsRigidbody.isKinematic = true;
    }

    private void SnapToRestRotation()
    {
        transform.localRotation = restLocalRotation;
        currentCompression = 0f;
        targetCompression = 0f;
        compressionArmed = true;
        ConfigureKinematicBody();
    }
}