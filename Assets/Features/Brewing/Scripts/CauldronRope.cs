using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(GrabbableItem))]
[RequireComponent(typeof(Rigidbody))]
public class CauldronRope : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CauldronController cauldron;

    [Header("Pull")]
    [SerializeField] private Vector3 localPullDirection = Vector3.down;
    [SerializeField] private float maximumPullDistance = 0.5f;
    [SerializeField] private float activationDistance = 0.4f;
    [SerializeField] private float mousePullSensitivity = 0.003f;

    [Header("Reset")]
    [SerializeField] private float returnDuration = 0.35f;

    private Rigidbody ropeRigidbody;
    private GrabController currentHolder;
    private Vector3 startingLocalPosition;
    private float currentPullDistance;
    private bool isBeingPulled;
    private bool hasActivated;
    private Coroutine returnRoutine;

    public bool IsBeingPulled => isBeingPulled;

    private void Awake()
    {
        ropeRigidbody = GetComponent<Rigidbody>();
        startingLocalPosition = transform.localPosition;
        ropeRigidbody.isKinematic = true;
        ropeRigidbody.useGravity = false;

        if (cauldron == null)
            Debug.LogError($"{name}: No CauldronController has been assigned.", this);
    }

    private void Update()
    {
        if (!isBeingPulled || currentHolder == null || Mouse.current == null)
            return;

        UpdatePull();
    }

    public void BeginPull(GrabController holder)
    {
        if (holder == null)
            return;

        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        currentHolder = holder;
        isBeingPulled = true;
        hasActivated = false;
        currentPullDistance = GetCurrentPullDistance();
        ropeRigidbody.isKinematic = true;
        ropeRigidbody.useGravity = false;
    }

    public void EndPull()
    {
        currentHolder = null;
        isBeingPulled = false;

        if (returnRoutine != null)
            StopCoroutine(returnRoutine);

        returnRoutine = StartCoroutine(ReturnToStartRoutine());
    }

    private void UpdatePull()
    {
        float mouseMovement = Mouse.current.delta.ReadValue().y;
        float pullChange = -mouseMovement * mousePullSensitivity;
        currentPullDistance = Mathf.Clamp(currentPullDistance + pullChange, 0f, maximumPullDistance);

        Vector3 pullDirection = localPullDirection.sqrMagnitude > 0f ? localPullDirection.normalized : Vector3.down;
        transform.localPosition = startingLocalPosition + pullDirection * currentPullDistance;

        if (hasActivated || currentPullDistance < activationDistance)
            return;

        hasActivated = true;

        if (cauldron != null)
            cauldron.ClearCauldron();

        GrabController holderToRelease = currentHolder;

        if (holderToRelease != null)
            holderToRelease.Release();
    }

    private float GetCurrentPullDistance()
    {
        Vector3 pullDirection = localPullDirection.sqrMagnitude > 0f ? localPullDirection.normalized : Vector3.down;
        Vector3 offset = transform.localPosition - startingLocalPosition;
        return Mathf.Clamp(Vector3.Dot(offset, pullDirection), 0f, maximumPullDistance);
    }

    private IEnumerator ReturnToStartRoutine()
    {
        Vector3 returnStartPosition = transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < returnDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = returnDuration <= 0f ? 1f : Mathf.Clamp01(elapsedTime / returnDuration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            transform.localPosition = Vector3.Lerp(returnStartPosition, startingLocalPosition, easedProgress);
            yield return null;
        }

        transform.localPosition = startingLocalPosition;
        currentPullDistance = 0f;
        hasActivated = false;
        ropeRigidbody.isKinematic = true;
        ropeRigidbody.useGravity = false;
        returnRoutine = null;
    }
}