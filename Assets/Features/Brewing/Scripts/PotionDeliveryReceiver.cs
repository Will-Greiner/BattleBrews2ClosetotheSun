using UnityEngine;

public class PotionDeliveryReceiver : MonoBehaviour, IItemReceiver
{
    [Header("Prompt")]
    [SerializeField] private string deliverAction = "[LMB] Deliver";

    public bool CanReceiveItem(GrabbableItem item)
    {
        if (item == null || GameManager.Instance == null)
            return false;

        PotionItem potionItem = item.GetComponent<PotionItem>();

        if (potionItem == null || potionItem.Data == null)
            return false;

        return GameManager.Instance.CanDeliverPotion(potionItem.Data);
    }

    public void ReceiveItem(GrabbableItem item)
    {
        if (item == null || GameManager.Instance == null)
            return;

        PotionItem potionItem = item.GetComponent<PotionItem>();

        if (potionItem == null || potionItem.Data == null)
            return;

        if (!GameManager.Instance.DeliverPotion(potionItem.Data))
            return;

        Destroy(item.gameObject);
    }

    public string GetReceivePrompt(GrabbableItem item)
    {
        if (item == null)
            return string.Empty;

        PotionItem potionItem = item.GetComponent<PotionItem>();

        if (potionItem == null || potionItem.Data == null)
            return string.Empty;

        return $"{deliverAction} {potionItem.Data.PotionName}";
    }
}