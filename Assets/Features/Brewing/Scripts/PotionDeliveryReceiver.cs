using UnityEngine;

public class PotionDeliveryReceiver : MonoBehaviour, IItemReceiver, IItemHoverFeedback
{
    [Header("Prompt")]
    [SerializeField] private string deliverAction = "[LMB] Deliver";

    [Header("Hatch")]
    [SerializeField] private Transform hatch;
    [SerializeField] private float openXAngleOffset = 90f;
    [Min(0.01f)] [SerializeField] private float hatchRotationSpeed = 8f;

    private Quaternion closedHatchRotation;
    private bool hatchOpen;

    private void Awake()
    {
        if (hatch != null)
            closedHatchRotation = hatch.localRotation;
    }

    private void Update()
    {
        if (hatch == null)
            return;

        Quaternion target = hatchOpen
            ? closedHatchRotation * Quaternion.Euler(openXAngleOffset, 0f, 0f)
            : closedHatchRotation;
        float rotationT = 1f - Mathf.Exp(-hatchRotationSpeed * Time.deltaTime);
        hatch.localRotation = Quaternion.Slerp(hatch.localRotation, target, rotationT);
    }

    private void OnDisable()
    {
        hatchOpen = false;

        if (hatch != null)
            hatch.localRotation = closedHatchRotation;
    }

    public bool CanReceiveItem(GrabbableItem item)
    {
        return item != null
            && GameManager.Instance != null
            && GameManager.Instance.State == GameState.RoundActive;
    }

    public void ReceiveItem(GrabbableItem item)
    {
        if (item == null || GameManager.Instance == null)
            return;

        PotionItem potionItem = item.GetComponent<PotionItem>();

        bool deliveryAccepted = potionItem != null && potionItem.Data != null
            ? GameManager.Instance.DeliverPotion(potionItem.Data)
            : GameManager.Instance.FailCurrentRound();

        if (!deliveryAccepted)
            return;

        Destroy(item.gameObject);
    }

    public string GetReceivePrompt(GrabbableItem item)
    {
        if (item == null)
            return string.Empty;

        return $"{deliverAction} {item.DisplayName}";
    }

    public void SetItemHover(bool hovering, GrabbableItem item)
    {
        hatchOpen = hovering && item != null;
    }
}
