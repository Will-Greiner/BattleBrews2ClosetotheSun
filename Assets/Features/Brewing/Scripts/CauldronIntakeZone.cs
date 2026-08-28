using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CauldronIntakeZone : MonoBehaviour, IItemReceiver, IItemRejectionFeedback
{
    [SerializeField] private CauldronController cauldron;
    [SerializeField] private string addIngredientPrompt = "[LMB] Drop into Cauldron";
    [SerializeField] private GrabController grabController;
    [SerializeField] private CauldronSplashController splashController;

    private readonly HashSet<int> splashedItems = new HashSet<int>();

    private void Awake()
    {
        Collider intakeCollider = GetComponent<Collider>();

        if (!intakeCollider.isTrigger)
            Debug.LogWarning($"{name}: The cauldron intake collider should have Is Trigger enabled.", this);

        if (cauldron == null)
            Debug.LogError($"{name}: No CauldronController has been assigned.", this);

        if (splashController == null)
            splashController = GetComponentInParent<CauldronSplashController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        TrySplashHand(other);
        TryAcceptIngredient(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TrySplashHand(other);
        TryAcceptIngredient(other);
    }

    private void OnTriggerExit(Collider other)
    {
        GrabbableItem item = other.GetComponentInParent<GrabbableItem>();

        if (item != null)
            splashedItems.Remove(item.GetInstanceID());
    }

    private void TrySplashHand(Collider other)
    {
        HandController hand = other.GetComponentInParent<HandController>();

        if (hand != null && splashController != null)
            splashController.TryPlayHandSplash(other.ClosestPoint(transform.position));
    }

    private void TryAcceptIngredient(Collider other)
    {
        if (cauldron == null)
            return;

        GrabbableItem item = other.GetComponentInParent<GrabbableItem>();

        if (item == null || item.IsHeld)
            return;

        Vector3 impactPosition = item.transform.position;

        if (splashedItems.Add(item.GetInstanceID()) && splashController != null)
            splashController.PlayItemSplash(impactPosition);

        cauldron.TryAddIngredient(item);
    }

    public bool CanReceiveItem(GrabbableItem item)
    {
        return cauldron != null && cauldron.CanAcceptIngredient(item);
    }

    public void ReceiveItem(GrabbableItem item)
    {
        if (cauldron == null || item == null)
            return;

        Vector3 impactPosition = item.transform.position;

        if (splashedItems.Add(item.GetInstanceID()) && splashController != null)
            splashController.PlayItemSplash(impactPosition);

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
