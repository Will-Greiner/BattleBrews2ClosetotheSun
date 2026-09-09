using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ProcessingIntakeZone : MonoBehaviour, IItemReceiver, IItemRejectionFeedback
{
    [SerializeField] private IngredientProcessingStation station;
    [SerializeField] private GrabController grabController;
    [Min(0f)] [SerializeField] private float feedbackDuration = 1.5f;
    [Min(0f)] [SerializeField] private float repeatedFeedbackDelay = 1f;
    [SerializeField] private string processPrompt = "Insert";

    private GrabbableItem lastRejectedItem;
    private float nextFeedbackTime;

    private void Awake()
    {
        Collider intakeCollider = GetComponent<Collider>();

        if (!intakeCollider.isTrigger)
            Debug.LogWarning($"{name}: The processing intake collider should have Is Trigger enabled.", this);

        if (station == null)
            Debug.LogError($"{name}: No IngredientProcessingStation has been assigned.", this);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryAcceptIngredient(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryAcceptIngredient(other);
    }

    private void OnTriggerExit(Collider other)
    {
        GrabbableItem item = other.GetComponentInParent<GrabbableItem>();

        if (item == lastRejectedItem)
            lastRejectedItem = null;
    }

    private void TryAcceptIngredient(Collider other)
    {
        if (station == null)
            return;

        GrabbableItem item = other.GetComponentInParent<GrabbableItem>();

        if (item == null || item.IsHeld)
            return;

        if (station.TryAcceptIngredient(item))
        {
            lastRejectedItem = null;
            return;
        }
    }

    public bool CanReceiveItem(GrabbableItem item)
    {
        return station != null && station.CanAcceptIngredient(item);
    }

    public void ReceiveItem(GrabbableItem item)
    {
        if (station == null || item == null)
            return;

        station.TryAcceptIngredient(item);
    }

    public string GetReceivePrompt(GrabbableItem item)
    {
        return CanReceiveItem(item) ? processPrompt : string.Empty;
    }

    public void ShowRejectionFeedback(GrabbableItem item)
    {
        if (grabController != null && station != null)
            grabController.ShowTemporaryPrompt(station.GetRejectionMessage(item), feedbackDuration);
    }
}