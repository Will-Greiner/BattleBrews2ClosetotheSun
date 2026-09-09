using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrabController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private HandController handController;
    [SerializeField] private HandAnimationController handAnimationController;
    [SerializeField] private InteractionPromptUI interactionPromptUI;

    [Header("Detection")]
    [SerializeField] private float interactionDistance = 10f;
    [SerializeField] private LayerMask interactionLayers = ~0;

    [Header("Prompts")]
    [SerializeField] private string grabPrompt = "[LMB] Grab";
    [SerializeField] private string releasePrompt = "[LMB] Release";

    [Header("Safety")]
    [SerializeField] private float breakDistance = 8f;

    [Header("Throwing")]
    [Tooltip("Multiplier applied to the hand's recent movement when an ordinary item is released.")]
    [Min(0f)] [SerializeField] private float throwStrength = 1.15f;
    [Min(0f)] [SerializeField] private float maximumThrowSpeed = 12f;
    [Min(0f)] [SerializeField] private float angularThrowStrength = 0.35f;
    [Min(0f)] [SerializeField] private float maximumAngularSpeed = 20f;

    private GrabbableItem heldItem;
    private ConfigurableJoint grabJoint;
    private Rigidbody handRigidbody;
    private bool manualInputEnabled = true;
    private readonly HashSet<object> inputLockOwners = new();
    private ObjectHighlight receiverHighlight;
    private IItemHoverFeedback receiverHoverFeedback;

    public Camera PlayerCamera => playerCamera;
    public GrabbableItem HeldItem => heldItem;
    public bool IsHoldingItem => heldItem != null;
    public bool InputEnabled => manualInputEnabled && inputLockOwners.Count == 0;

    private void Awake()
    {
        handRigidbody = GetComponent<Rigidbody>();

        if (handRigidbody == null)
            handRigidbody = gameObject.AddComponent<Rigidbody>();

        if (handAnimationController == null)
            handAnimationController = GetComponentInChildren<HandAnimationController>();

        handRigidbody.isKinematic = true;
        handRigidbody.useGravity = false;
        handRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        handRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    private void Update()
    {
        if (!InputEnabled || Mouse.current == null || playerCamera == null)
        {
            ClearReceiverHighlight();
            HideInteractionPrompt();
            handAnimationController?.SetInteractionState(false, false);
            return;
        }

        UpdateReceiverHighlight();
        UpdateInteractionPrompt();
        UpdateHandAnimationState();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandlePrimaryPress();
            UpdateReceiverHighlight();
            UpdateInteractionPrompt();
            UpdateHandAnimationState();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            HandlePrimaryRelease();
            UpdateReceiverHighlight();
            UpdateInteractionPrompt();
            UpdateHandAnimationState();
        }
    }

    private void FixedUpdate()
    {
        if (heldItem == null)
            return;

        if (heldItem.GetComponent<StirringStick>() != null || heldItem.GetComponent<CauldronRope>() != null || heldItem.GetComponent<MortarPestle>() != null || heldItem.GetComponent<BurnerBellows>() != null || heldItem.GetComponent<PulverizerCrank>() != null)
            return;

        float distance = Vector3.Distance(heldItem.GrabPoint.position, transform.position);

        if (distance > breakDistance)
            Release();
    }

    private void OnDisable()
    {
        HideInteractionPrompt();
    }

    private void HandlePrimaryPress()
    {
        if (heldItem != null)
            return;

        TryUseTargetUnderMouse();
    }

    private void HandlePrimaryRelease()
    {
        if (heldItem == null)
            return;

        if (!TryUseHeldItemOnReceiver())
            Release();
    }

    private void UpdateInteractionPrompt()
    {
        if (interactionPromptUI == null)
            return;

        string prompt = GetCurrentInteractionPrompt();

        if (string.IsNullOrWhiteSpace(prompt))
            interactionPromptUI.Hide();
        else
            interactionPromptUI.Show(prompt);
    }

    private string GetCurrentInteractionPrompt()
    {
        if (heldItem != null)
            return GetHeldItemPrompt();

        return GetEmptyHandPrompt();
    }

    private string GetHeldItemPrompt()
    {
        if (!string.IsNullOrWhiteSpace(heldItem.HeldPromptOverride))
            return heldItem.HeldPromptOverride;

        if (!TryGetPointerHitIgnoringHeldItem(out RaycastHit hit))
            return releasePrompt;

        IItemReceiver receiver = FindItemReceiver(hit.collider);

        if (receiver != null && receiver.CanReceiveItem(heldItem))
            return receiver.GetReceivePrompt(heldItem);

        return releasePrompt;
    }

    private string GetEmptyHandPrompt()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = playerCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayers, QueryTriggerInteraction.Ignore))
        {
            GrabbableItem grabbableItem = hit.collider.GetComponentInParent<GrabbableItem>();

            if (grabbableItem != null && grabbableItem.CanGrab())
                return GetGrabPrompt(grabbableItem);

            IHandInteractable interactable = FindHandInteractable(hit.collider);

            if (interactable != null && interactable.CanInteract(this))
                return interactable.GetInteractionPrompt(this);

            return string.Empty;
        }

        if (handController == null)
            return string.Empty;

        GrabbableItem focusedItem = handController.FocusedItem;

        if (focusedItem != null && focusedItem.CanGrab() && IsWithinInteractionDistance(focusedItem))
            return GetGrabPrompt(focusedItem);

        return string.Empty;
    }

    private string GetGrabPrompt(GrabbableItem item)
    {
        if (item == null)
            return string.Empty;

        if (!string.IsNullOrWhiteSpace(item.GrabPromptOverride))
            return item.GrabPromptOverride;

        return $"{grabPrompt} {item.DisplayName}";
    }

    public void ShowTemporaryPrompt(string message, float duration = 1.5f)
    {
        if (interactionPromptUI != null)
            interactionPromptUI.ShowTemporary(message, duration);
    }

    private void HideInteractionPrompt()
    {
        if (interactionPromptUI != null)
            interactionPromptUI.Hide();
    }

    private void UpdateHandAnimationState()
    {
        if (handAnimationController == null)
            return;

        bool holding = heldItem != null;
        bool hovering = holding ? receiverHighlight != null || receiverHoverFeedback != null : !string.IsNullOrWhiteSpace(GetEmptyHandPrompt());
        handAnimationController.SetInteractionState(holding, hovering);
    }

    private bool TryUseHeldItemOnReceiver()
    {
        if (!TryGetPointerHitIgnoringHeldItem(out RaycastHit hit))
            return false;

        IItemReceiver receiver = FindItemReceiver(hit.collider);

        if (receiver == null)
            return false;

        if (!receiver.CanReceiveItem(heldItem))
        {
            if (receiver is IItemRejectionFeedback rejectionFeedback)
                rejectionFeedback.ShowRejectionFeedback(heldItem);

            return false;
        }

        GrabbableItem transferredItem = TakeHeldItem();

        if (transferredItem == null)
            return false;

        receiver.ReceiveItem(transferredItem);
        return true;
    }

    private bool TryGetPointerHitIgnoringHeldItem(out RaycastHit validHit)
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = playerCamera.ScreenPointToRay(mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, interactionDistance, interactionLayers, QueryTriggerInteraction.Collide);

        Array.Sort(hits, (firstHit, secondHit) => firstHit.distance.CompareTo(secondHit.distance));

        foreach (RaycastHit hit in hits)
        {
            if (heldItem != null && hit.collider.transform.IsChildOf(heldItem.transform))
                continue;

            validHit = hit;
            return true;
        }

        validHit = default;
        return false;
    }

    private void TryUseTargetUnderMouse()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = playerCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayers, QueryTriggerInteraction.Ignore))
        {
            GrabbableItem grabbableItem = hit.collider.GetComponentInParent<GrabbableItem>();

            if (grabbableItem != null && grabbableItem.CanGrab())
            {
                Grab(grabbableItem);
                return;
            }

            IHandInteractable interactable = FindHandInteractable(hit.collider);

            if (interactable != null && interactable.CanInteract(this))
            {
                interactable.Interact(this);
                return;
            }

            return;
        }

        TryGrabFocusedItem();
    }

    private bool IsWithinInteractionDistance(GrabbableItem item)
    {
        if (item == null || playerCamera == null)
            return false;

        float distance = Vector3.Distance(playerCamera.transform.position, item.GrabPoint.position);
        return distance <= interactionDistance;
    }

    private IItemReceiver FindItemReceiver(Collider targetCollider)
    {
        MonoBehaviour[] behaviours = targetCollider.GetComponentsInParent<MonoBehaviour>();

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IItemReceiver receiver)
                return receiver;
        }

        return null;
    }

    private IHandInteractable FindHandInteractable(Collider targetCollider)
    {
        MonoBehaviour[] behaviours = targetCollider.GetComponentsInParent<MonoBehaviour>();

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IHandInteractable interactable)
                return interactable;
        }

        return null;
    }

    public bool Grab(GrabbableItem item)
    {
        if (heldItem != null || item == null || !item.CanGrab())
            return false;

        heldItem = item;
        heldItem.OnGrabbed();

        StirringStick stirringStick = heldItem.GetComponent<StirringStick>();

        if (stirringStick != null)
        {
            stirringStick.BeginStirring(this);
            return true;
        }

        CauldronRope clearRope = heldItem.GetComponent<CauldronRope>();

        if (clearRope != null)
        {
            clearRope.BeginPull(this);
            return true;
        }

        MortarPestle mortarPestle = heldItem.GetComponent<MortarPestle>();

        if (mortarPestle != null)
        {
            mortarPestle.BeginPounding(this);
            return true;
        }

        BurnerBellows burnerBellows = heldItem.GetComponent<BurnerBellows>();

        if (burnerBellows != null)
        {
            burnerBellows.BeginSqueezing(this);
            return true;
        }

        PulverizerCrank pulverizerCrank = heldItem.GetComponent<PulverizerCrank>();

        if (pulverizerCrank != null)
        {
            pulverizerCrank.BeginCranking(this);
            return true;
        }

        ProcessingTableHandle tableHandle = heldItem.GetComponent<ProcessingTableHandle>();

        if (tableHandle != null)
        {
            tableHandle.BeginTurning(this);
            return true;
        }

        Rigidbody body = heldItem.Rigidbody;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.solverIterations = 12;
        body.solverVelocityIterations = 12;

        grabJoint = body.gameObject.AddComponent<ConfigurableJoint>();
        grabJoint.connectedBody = handRigidbody;
        grabJoint.autoConfigureConnectedAnchor = false;
        grabJoint.anchor = body.transform.InverseTransformPoint(heldItem.GrabPoint.position);
        grabJoint.connectedAnchor = Vector3.zero;
        grabJoint.xMotion = ConfigurableJointMotion.Locked;
        grabJoint.yMotion = ConfigurableJointMotion.Locked;
        grabJoint.zMotion = ConfigurableJointMotion.Locked;
        grabJoint.angularXMotion = ConfigurableJointMotion.Free;
        grabJoint.angularYMotion = ConfigurableJointMotion.Free;
        grabJoint.angularZMotion = ConfigurableJointMotion.Free;
        grabJoint.enableCollision = false;
        grabJoint.enablePreprocessing = true;
        grabJoint.projectionMode = JointProjectionMode.PositionAndRotation;
        grabJoint.projectionDistance = 0.01f;
        grabJoint.projectionAngle = 180f;

        return true;
    }

    public bool Release()
    {
        GrabbableItem releasedItem = TakeHeldItem();

        if (releasedItem == null)
            return false;

        ApplyReleaseMomentum(releasedItem);
        return true;
    }

    public GrabbableItem TakeHeldItem()
    {
        if (heldItem == null)
            return null;

        ClearReceiverHighlight();

        GrabbableItem transferredItem = heldItem;
        heldItem = null;

        if (grabJoint != null)
        {
            Destroy(grabJoint);
            grabJoint = null;
        }

        StirringStick stirringStick = transferredItem.GetComponent<StirringStick>();

        if (stirringStick != null)
            stirringStick.EndStirring();

        CauldronRope clearRope = transferredItem.GetComponent<CauldronRope>();

        if (clearRope != null)
            clearRope.EndPull();

        MortarPestle mortarPestle = transferredItem.GetComponent<MortarPestle>();

        if (mortarPestle != null)
            mortarPestle.EndPounding();

        BurnerBellows burnerBellows = transferredItem.GetComponent<BurnerBellows>();

        if (burnerBellows != null)
            burnerBellows.EndSqueezing();

        PulverizerCrank pulverizerCrank = transferredItem.GetComponent<PulverizerCrank>();

        if (pulverizerCrank != null)
            pulverizerCrank.EndCranking();

        ProcessingTableHandle tableHandle = transferredItem.GetComponent<ProcessingTableHandle>();

        if (tableHandle != null)
            tableHandle.EndTurning();

        transferredItem.OnReleased();
        return transferredItem;
    }

    private void ApplyReleaseMomentum(GrabbableItem item)
    {
        if (item == null || item.Rigidbody == null || handController == null || IsConstrainedInteraction(item))
            return;

        Vector3 releaseVelocity = Vector3.ClampMagnitude(handController.Velocity * throwStrength, maximumThrowSpeed);
        Vector3 releaseAngularVelocity = Vector3.ClampMagnitude(handController.AngularVelocity * angularThrowStrength, maximumAngularSpeed);
        item.Rigidbody.linearVelocity = releaseVelocity;
        item.Rigidbody.angularVelocity = releaseAngularVelocity;
    }

    private bool IsConstrainedInteraction(GrabbableItem item)
    {
        return item.GetComponent<StirringStick>() != null || item.GetComponent<CauldronRope>() != null || item.GetComponent<MortarPestle>() != null || item.GetComponent<BurnerBellows>() != null || item.GetComponent<PulverizerCrank>() != null || item.GetComponent<ProcessingTableHandle>() != null;
    }

    public void SetInputEnabled(bool enabled)
    {
        manualInputEnabled = enabled;

        if (!InputEnabled)
        {
            ClearReceiverHighlight();
            HideInteractionPrompt();
        }
    }

    public void AcquireInputLock(object owner)
    {
        if (owner == null)
            return;

        inputLockOwners.Add(owner);
        ClearReceiverHighlight();
        HideInteractionPrompt();
    }

    public void ReleaseInputLock(object owner)
    {
        if (owner == null)
            return;

        inputLockOwners.Remove(owner);
    }

    public void ClearAllInputLocks()
    {
        inputLockOwners.Clear();

        if (!InputEnabled)
        {
            ClearReceiverHighlight();
            HideInteractionPrompt();
        }
    }

    private void TryGrabFocusedItem()
    {
        if (handController == null)
            return;

        GrabbableItem focusedItem = handController.FocusedItem;

        if (focusedItem == null || !focusedItem.CanGrab() || !IsWithinInteractionDistance(focusedItem))
            return;

        Grab(focusedItem);
    }

    private void UpdateReceiverHighlight()
    {
        if (heldItem == null)
        {
            ClearReceiverHighlight();
            return;
        }

        if (!TryGetPointerHitIgnoringHeldItem(out RaycastHit hit))
        {
            ClearReceiverHighlight();
            return;
        }

        IItemReceiver receiver = FindItemReceiver(hit.collider);

        if (receiver == null)
        {
            ClearReceiverHighlight();
            return;
        }

        IItemHoverFeedback newHoverFeedback = receiver as IItemHoverFeedback;

        if (!receiver.CanReceiveItem(heldItem))
        {
            if (newHoverFeedback == null)
            {
                ClearReceiverHighlight();
                return;
            }

            if (newHoverFeedback != receiverHoverFeedback)
            {
                ClearReceiverHighlight();
                receiverHoverFeedback = newHoverFeedback;
                receiverHoverFeedback.SetItemHover(true, heldItem);
            }

            if (receiverHighlight != null)
            {
                receiverHighlight.Hide();
                receiverHighlight = null;
            }

            return;
        }

        ObjectHighlight newHighlight = hit.collider.GetComponentInParent<ObjectHighlight>();

        if (newHighlight == receiverHighlight && newHoverFeedback == receiverHoverFeedback)
            return;

        ClearReceiverHighlight();
        receiverHighlight = newHighlight;
        receiverHoverFeedback = newHoverFeedback;

        if (receiverHighlight != null)
            receiverHighlight.Show();

        receiverHoverFeedback?.SetItemHover(true, heldItem);
    }

    private void ClearReceiverHighlight()
    {
        ClearReceiverHoverFeedback();

        if (receiverHighlight != null)
            receiverHighlight.Hide();

        receiverHighlight = null;
    }

    private void ClearReceiverHoverFeedback()
    {
        if (receiverHoverFeedback != null)
            receiverHoverFeedback.SetItemHover(false, heldItem);

        receiverHoverFeedback = null;
    }
}
