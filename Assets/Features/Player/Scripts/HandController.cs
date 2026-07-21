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

    private Quaternion targetRotation;
    private GrabbableItem focusedItem;
    private ObjectHighlight focusedHighlight;
    private float focusedDistance;
    private float focusLostTimer;

    public float Distance => handDistance;
    public GrabbableItem FocusedItem => focusedItem;

    private void Start()
    {
        handDistance = Mathf.Clamp(handDistance, minDistance, maxDistance);
        defaultDistance = Mathf.Clamp(defaultDistance, minDistance, maxDistance);
        focusedDistance = handDistance;
        targetRotation = transform.rotation;

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    private void LateUpdate()
    {
        if (Mouse.current == null || playerCamera == null)
            return;

        UpdateHandTransform();
    }

    private void UpdateHandTransform()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray mouseRay = playerCamera.ScreenPointToRay(mousePosition);
        
        if (TryUpdateConstrainedInteractionHand(mouseRay))
            return;

        bool isHolding = grabController != null && grabController.IsHoldingItem;
        bool isStirring = IsStirring();
        int layerMask = depthLayers;

        if (isHolding)
        {
            layerMask &= ~ignoredWhileHolding.value;
            ClearFocus();
        }

        float targetDistance = defaultDistance;

        if (isStirring)
        {
            targetDistance = GetStirringDistance(mouseRay);
        }
        else
        {
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
                else
                {
                    targetDistance = GetSurfaceDistance(hit.distance);
                }
            }
            else if (!isHolding && MaintainFocus())
            {
                targetDistance = focusedDistance;
            }
        }

        handDistance = Mathf.MoveTowards(handDistance, targetDistance, depthMoveSpeed * Time.deltaTime);
        transform.position = mouseRay.GetPoint(handDistance);
        UpdateHandRotation(mouseRay);
    }

    private bool IsStirring()
    {
        if (grabController == null || grabController.HeldItem == null)
            return false;

        StirringStick stirringStick = grabController.HeldItem.GetComponent<StirringStick>();
        return stirringStick != null && stirringStick.IsStirring;
    }

    private float GetStirringDistance(Ray mouseRay)
    {
        if (grabController == null || grabController.HeldItem == null)
            return defaultDistance;

        Vector3 grabPointPosition = grabController.HeldItem.GrabPoint.position;
        float projectedDistance = Vector3.Dot(grabPointPosition - mouseRay.origin, mouseRay.direction);
        return Mathf.Clamp(projectedDistance, minDistance, maxDistance);
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

        bool isStirring = stirringStick != null && stirringStick.IsStirring;
        bool isPullingRope = clearRope != null && clearRope.IsBeingPulled;
        bool isPounding = mortarPestle != null && mortarPestle.IsBeingUsed;
        bool isSqueezingBellows = burnerBellows != null && burnerBellows.IsBeingUsed;
        bool isCranking = pulverizerCrank != null && pulverizerCrank.IsBeingUsed;

        if (!isStirring && !isPullingRope && !isPounding && !isSqueezingBellows && !isCranking)
            return false;

        Transform grabPoint = heldItem.GrabPoint;
        transform.position = grabPoint.position;
        handDistance = Vector3.Distance(playerCamera.transform.position, grabPoint.position);
        UpdateHandRotation(mouseRay);

        return true;
    }

    private void UpdateHandRotation(Ray mouseRay)
    {
        targetRotation = Quaternion.LookRotation(mouseRay.direction, playerCamera.transform.up) * Quaternion.Euler(rotationOffset);
        float rotationT = 1f - Mathf.Exp(-rotationSmoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationT);
    }
}