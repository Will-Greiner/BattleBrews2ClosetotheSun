using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CauldronIntakeZone : MonoBehaviour
{
    [SerializeField] private CauldronController cauldron;

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
}