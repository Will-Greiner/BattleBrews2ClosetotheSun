using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CauldronIntakeZone : MonoBehaviour, IItemReceiver, IItemRejectionFeedback
{
    [SerializeField] private CauldronController cauldron;
    [SerializeField] private string addIngredientPrompt = "[LMB] Drop into Cauldron";
    [SerializeField] private GrabController grabController;

    private void Awake()
    {
        Collider intakeCollider = GetComponent<Collider>();

        if (!intakeCollider.isTrigger)
            Debug.LogWarning($"{name}: The cauldron intake collider should have Is Trigger enabled.", this);

        if (cauldron == null)
            Debug.LogError($"{name}: No CauldronController has been assigned.", this);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryAcceptIngredient(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryAcceptIngredient(other);
    }

    private void TryAcceptIngredient(Collider other)
    {
        if (cauldron == null)
            return;

        GrabbableItem item = other.GetComponentInParent<GrabbableItem>();

        if (item == null || item.IsHeld)
            return;

        cauldron.TryAddIngredient(item);
    }

    public bool CanReceiveItem(GrabbableItem item)
    {
        return cauldron != null && cauldron.CanAcceptIngredient(item);
    }

    public void ReceiveItem(GrabbableItem item)
    {
        if (cauldron != null)
            cauldron.TryAddIngredient(item);
    }

    public string GetReceivePrompt(GrabbableItem item)
    {
        return CanReceiveItem(item) ? addIngredientPrompt : string.Empty;
    }

    public void ShowRejectionFeedback(GrabbableItem item)
    {
        if (grabController == null)
            return;

        if (item == null || item.GetComponent<IngredientItem>() == null)
            grabController.ShowTemporaryPrompt("The cauldron only accepts ingredients");
        else
            grabController.ShowTemporaryPrompt("The cauldron is full");
    }
}
