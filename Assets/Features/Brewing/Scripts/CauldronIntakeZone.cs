using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CauldronIntakeZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CauldronController cauldron;
    [SerializeField] private IngredientContributionUI contributionUI;
    [SerializeField] private GrabController grabController;
    [SerializeField] private Transform ejectPoint;

    [Header("Cancel Ejection")]
    [SerializeField] private Vector3 ejectionVelocity = new Vector3(0f, 2f, 1f);

    private GrabbableItem pendingItem;
    private Rigidbody pendingRigidbody;
    private bool pendingWasKinematic;
    private bool pendingUsedGravity;

    private void Awake()
    {
        Collider intakeCollider = GetComponent<Collider>();

        if (!intakeCollider.isTrigger)
            Debug.LogWarning($"{name}: The cauldron intake collider should have Is Trigger enabled.", this);

        if (cauldron == null)
            Debug.LogError($"{name}: No CauldronController has been assigned.", this);

        if (contributionUI == null)
            Debug.LogError($"{name}: No IngredientContributionUI has been assigned.", this);

        if (grabController == null)
            Debug.LogError($"{name}: No GrabController has been assigned.", this);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryBeginSelection(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryBeginSelection(other);
    }

    private void TryBeginSelection(Collider other)
    {
        if (pendingItem != null || contributionUI == null || contributionUI.IsOpen)
            return;

        GrabbableItem item = other.GetComponentInParent<GrabbableItem>();

        if (item == null || item.IsHeld || !cauldron.CanAcceptIngredient(item))
            return;

        IngredientItem ingredientItem = item.GetComponent<IngredientItem>();

        if (ingredientItem == null || ingredientItem.Data == null)
            return;

        pendingItem = item;
        FreezePendingItem();

        if (grabController != null)
            grabController.SetInputEnabled(false);

        Transform handTransform = grabController != null ? grabController.transform : transform;
        contributionUI.Show(ingredientItem.Data, handTransform, ConfirmContribution, CancelContribution);
    }

    private void FreezePendingItem()
    {
        pendingRigidbody = pendingItem.Rigidbody;

        if (pendingRigidbody == null)
            return;

        pendingWasKinematic = pendingRigidbody.isKinematic;
        pendingUsedGravity = pendingRigidbody.useGravity;
        pendingRigidbody.linearVelocity = Vector3.zero;
        pendingRigidbody.angularVelocity = Vector3.zero;
        pendingRigidbody.useGravity = false;
        pendingRigidbody.isKinematic = true;
    }

    private void ConfirmContribution(CauldronContribution contribution)
    {
        GrabbableItem itemToConsume = pendingItem;
        ClearPendingReferences();

        if (grabController != null)
            grabController.SetInputEnabled(true);

        if (itemToConsume == null || !cauldron.TryAddContribution(itemToConsume, contribution))
            EjectItem(itemToConsume);
    }

    private void CancelContribution()
    {
        GrabbableItem itemToEject = pendingItem;
        ClearPendingReferences();

        if (grabController != null)
            grabController.SetInputEnabled(true);

        EjectItem(itemToEject);
    }

    private void EjectItem(GrabbableItem item)
    {
        if (item == null)
            return;

        Rigidbody itemRigidbody = item.Rigidbody;

        if (ejectPoint != null)
        {
            item.transform.position = ejectPoint.position;
            item.transform.rotation = ejectPoint.rotation;
        }

        if (itemRigidbody == null)
            return;

        itemRigidbody.isKinematic = pendingWasKinematic;
        itemRigidbody.useGravity = pendingUsedGravity;

        if (!itemRigidbody.isKinematic)
        {
            itemRigidbody.linearVelocity = ejectPoint != null ? ejectPoint.TransformDirection(ejectionVelocity) : ejectionVelocity;
            itemRigidbody.angularVelocity = Vector3.zero;
        }
    }

    private void ClearPendingReferences()
    {
        pendingItem = null;
        pendingRigidbody = null;
    }

    private void OnDisable()
    {
        if (pendingItem != null)
            CancelContribution();
    }
}