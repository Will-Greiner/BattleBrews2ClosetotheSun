using UnityEngine;
using UnityEngine.InputSystem;

public class HandController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GrabController grabController;

    [Header("Depth")]
    [SerializeField] private float handDistance = 2f;
    [SerializeField] private float minDistance = 0.75f;
    [SerializeField] private float maxDistance = 8f;
    [SerializeField] private float depthMoveSpeed = 3f;
    [SerializeField] private float defaultDistance = 2f;

    [Header("Held Item Depth")]
    [Tooltip("Camera-relative distance that ordinary held objects move toward and remain at until released.")]
    [Min(0.01f)] [SerializeField] private float heldDistance = 2f;
    [Tooltip("How quickly the hand settles at the fixed held-item distance.")]
    [Min(0.01f)] [SerializeField] private float heldDepthMoveSpeed = 8f;

    [Header("Automatic Depth")]
    [SerializeField] private LayerMask depthLayers = ~0;
    [SerializeField] private LayerMask ignoredWhileHolding;
    [SerializeField] private float surfaceOffset = 0.15f;

    [Header("Grabbable Focus")]
    [Tooltip("How long focus remains after the cursor leaves a grabbable.")]
    [SerializeField] private float focusReleaseDelay = 0.2f;

    [Header("Rotation")]
    [SerializeField] private Vector3 rotationOffset;
    [SerializeField] private float rotationSmoothSpeed = 30f;

    [Header("Motion Tracking")]
    [Tooltip("Higher values make release momentum react more immediately to the latest hand movement.")]
    [Min(0.01f)] [SerializeField] private float velocityResponse = 18f;

    private Quaternion targetRotation;
    private GrabbableItem focusedItem;
    private ObjectHighlight focusedHighlight;
    private float focusedDistance;
    private float focusLostTimer;
    private GrabbableItem lastHeldItem;
    private Vector3 previousPosition;
    private Quaternion previousRotation;
    private Vector3 smoothedVelocity;
    private Vector3 smoothedAngularVelocity;
    private Renderer[] handRenderers;

    public float Distance => handDistance;
    public GrabbableItem FocusedItem => focusedItem;
    public Vector3 Velocity => smoothedVelocity;
    public Vector3 AngularVelocity => smoothedAngularVelocity;

    private void Awake()
    {
        handRenderers = GetComponentsInChildren<Renderer>(true);
    }

    public void SetVisualsVisible(bool visible)
    {
        if (handRenderers == null)
            handRenderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer handRenderer in handRenderers)
        {
            if (handRenderer != null)
                handRenderer.enabled = visible;
        }
    }

    private void Start()
    {
        if (grabController == null)
            grabController = FindFirstObjectByType<GrabController>();

        handDistance = Mathf.Clamp(handDistance, minDistance, maxDistance);
        defaultDistance = Mathf.Clamp(defaultDistance, minDistance, maxDistance);
        focusedDistance = handDistance;
        targetRotation = transform.rotation;
        previousPosition = transform.position;
        previousRotation = transform.rotation;

    }

    private void LateUpdate()
    {
        if (grabController != null && !grabController.InputEnabled)
        {
            ClearFocus();
            ResetMotionTracking();
            return;
        }

        if (Mouse.current == null || playerCamera == null)
            return;

        UpdateHandTransform();
        UpdateMotionTracking();
    }

    private void UpdateHandTransform()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray mouseRay = playerCamera.ScreenPointToRay(mousePosition);
        
        if (TryUpdateConstrainedInteractionHand(mouseRay))
            return;

        bool isHolding = grabController != null && grabController.IsHoldingItem;
        if (isHolding)
        {
            UpdateHeldItemDepth(mouseRay);
            return;
        }

        lastHeldItem = null;
        int layerMask = depthLayers;

        if (isHolding)
        {
            layerMask &= ~ignoredWhileHolding.value;
            ClearFocus();
        }

        float targetDistance = defaultDistance;

        bool foundSurface = Physics.Raycast(mouseRay, out RaycastHit hit, maxDistance, layerMask, QueryTriggerInteraction.Ignore);

        if (foundSurface)
        {
            GrabbableItem hitItem = hit.collider.GetComponentInParent<GrabbableItem>();

            if (!isHolding && hitItem != null && hitItem.CanGrab())
            {
                SetFocus(hitItem, hit.distance);
                targetDistance = focusedDistance;
            }
            else if (!isHolding)
            {
                ObjectHighlight hitHighlight = hit.collider.GetComponentInParent<ObjectHighlight>();

                if (hitHighlight != null)
                {
                    SetFocus(hitHighlight, hit.distance);
                    targetDistance = focusedDistance;
                }
                else
                {
                    ClearFocus();
                    targetDistance = GetSurfaceDistance(hit.distance);
                }
            }
        }
        else if (!isHolding && MaintainFocus())
        {
            targetDistance = focusedDistance;
        }

        handDistance = Mathf.MoveTowards(handDistance, targetDistance, depthMoveSpeed * Time.deltaTime);
        transform.position = mouseRay.GetPoint(handDistance);
        UpdateHandRotation(mouseRay);
    }

    private void UpdateHeldItemDepth(Ray mouseRay)
    {
        GrabbableItem heldItem = grabController != null ? grabController.HeldItem : null;

        if (heldItem == null)
            return;

        if (heldItem != lastHeldItem)
        {
            lastHeldItem = heldItem;
            ClearFocus();
        }

        float targetHeldDistance = Mathf.Clamp(heldDistance, minDistance, maxDistance);
        handDistance = Mathf.MoveTowards(handDistance, targetHeldDistance, heldDepthMoveSpeed * Time.deltaTime);
        transform.position = mouseRay.GetPoint(handDistance);
        UpdateHandRotation(mouseRay);
    }

    private void SetFocus(GrabbableItem item, float hitDistance)
    {
        if (focusedItem != item)
        {
            ClearFocus();
            focusedItem = item;
            focusedHighlight = item.GetComponent<ObjectHighlight>();

            if (focusedHighlight != null)
                focusedHighlight.Show();
        }

        focusedDistance = GetSurfaceDistance(hitDistance);
        focusLostTimer = focusReleaseDelay;
    }

    private void SetFocus(ObjectHighlight highlight, float hitDistance)
    {
        if (focusedHighlight != highlight)
        {
            ClearFocus();
            focusedHighlight = highlight;
            focusedHighlight.Show();
        }

        focusedItem = null;
        focusedDistance = GetSurfaceDistance(hitDistance);
        focusLostTimer = focusReleaseDelay;
    }

    private bool MaintainFocus()
    {
        if (focusedHighlight == null)
        {
            ClearFocus();
            return false;
        }

        if (focusedItem != null && !focusedItem.CanGrab())
        {
            ClearFocus();
            return false;
        }

        focusLostTimer -= Time.deltaTime;

        if (focusLostTimer <= 0f)
        {
            ClearFocus();
            return false;
        }

        return true;
    }

    private float GetSurfaceDistance(float hitDistance)
    {
        return Mathf.Clamp(hitDistance - surfaceOffset, minDistance, maxDistance);
    }

    private void ClearFocus()
    {
        if (focusedHighlight != null)
            focusedHighlight.Hide();

        focusedItem = null;
        focusedHighlight = null;
        focusLostTimer = 0f;
    }

    private bool TryUpdateConstrainedInteractionHand(Ray mouseRay)
    {
        if (grabController == null || grabController.HeldItem == null)
            return false;

        GrabbableItem heldItem = grabController.HeldItem;
        StirringStick stirringStick = heldItem.GetComponent<StirringStick>();
        CauldronRope clearRope = heldItem.GetComponent<CauldronRope>();
        MortarPestle mortarPestle = heldItem.GetComponent<MortarPestle>();
        BurnerBellows burnerBellows = heldItem.GetComponent<BurnerBellows>();
        PulverizerCrank pulverizerCrank = heldItem.GetComponent<PulverizerCrank>();
        ProcessingTableHandle tableHandle = heldItem.GetComponent<ProcessingTableHandle>();

        bool isStirring = stirringStick != null && stirringStick.IsStirring;
        bool isPullingRope = clearRope != null && clearRope.IsBeingPulled;
        bool isPounding = mortarPestle != null && mortarPestle.IsBeingUsed;
        bool isSqueezingBellows = burnerBellows != null && burnerBellows.IsBeingUsed;
        bool isCranking = pulverizerCrank != null && pulverizerCrank.IsBeingUsed;
        bool isTurningTable = tableHandle != null && tableHandle.IsBeingUsed;

        if (!isStirring && !isPullingRope && !isPounding && !isSqueezingBellows && !isCranking && !isTurningTable)
            return false;

        Transform grabPoint = heldItem.GrabPoint;

        UpdateHandRotation(mouseRay);

        Transform handAnchor = grabController.GrabTarget;

        if (handAnchor != null)
            transform.position += grabPoint.position - handAnchor.position;
        else
            transform.position = grabPoint.position;

        handDistance = Vector3.Distance(playerCamera.transform.position, transform.position);

        return true;
    }

    private void UpdateHandRotation(Ray mouseRay)
    {
        targetRotation = Quaternion.LookRotation(mouseRay.direction, playerCamera.transform.up) * Quaternion.Euler(rotationOffset);
        float rotationT = 1f - Mathf.Exp(-rotationSmoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationT);
    }

    private void UpdateMotionTracking()
    {
        float deltaTime = Time.unscaledDeltaTime;

        if (deltaTime <= Mathf.Epsilon)
            return;

        Vector3 frameVelocity = (transform.position - previousPosition) / deltaTime;
        Quaternion rotationDelta = transform.rotation * Quaternion.Inverse(previousRotation);
        rotationDelta.ToAngleAxis(out float angleDegrees, out Vector3 axis);

        if (angleDegrees > 180f)
            angleDegrees -= 360f;

        Vector3 frameAngularVelocity = axis.sqrMagnitude > Mathf.Epsilon ? axis.normalized * angleDegrees * Mathf.Deg2Rad / deltaTime : Vector3.zero;
        float response = 1f - Mathf.Exp(-velocityResponse * deltaTime);
        smoothedVelocity = Vector3.Lerp(smoothedVelocity, frameVelocity, response);
        smoothedAngularVelocity = Vector3.Lerp(smoothedAngularVelocity, frameAngularVelocity, response);
        previousPosition = transform.position;
        previousRotation = transform.rotation;
    }

    private void ResetMotionTracking()
    {
        smoothedVelocity = Vector3.zero;
        smoothedAngularVelocity = Vector3.zero;
        previousPosition = transform.position;
        previousRotation = transform.rotation;
    }
}
