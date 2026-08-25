using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private List<ShopOfferData> offers = new();

    public event Action OffersChanged;
    public event Action<ShopOfferData> PurchaseCompleted;

    public IReadOnlyList<ShopOfferData> Offers => offers;

    public bool IsOfferAvailable(ShopOfferData offer, int round)
    {
        if (offer == null || round < offer.FirstAvailableRound || ProgressionManager.Instance == null)
            return false;

        foreach (ShopOfferData prerequisite in offer.Prerequisites)
        {
            if (prerequisite == null || !IsPurchased(prerequisite))
                return false;
        }

        return !IsMaxed(offer);
    }

    public bool CanPurchase(ShopOfferData offer, int round)
    {
        return IsOfferAvailable(offer, round) && ProgressionManager.Instance.CanAfford(offer.Price);
    }

    public bool TryPurchase(ShopOfferData offer, int round)
    {
        ProgressionManager progression = ProgressionManager.Instance;

        if (progression == null || !CanPurchase(offer, round) || !progression.TrySpendCurrency(offer.Price))
            return false;

        if (offer.OfferType == ShopOfferType.ContentUnlock)
            progression.UnlockContent(offer.UnlockId, false);
        else
            progression.SetUpgradeLevel(offer.UnlockId, progression.GetUpgradeLevel(offer.UnlockId) + 1, false);

        progression.SaveNow();
        PurchaseCompleted?.Invoke(offer);
        OffersChanged?.Invoke();
        return true;
    }

    public bool IsPurchased(ShopOfferData offer)
    {
        if (offer == null || ProgressionManager.Instance == null)
            return false;

        return offer.OfferType == ShopOfferType.ContentUnlock
            ? ProgressionManager.Instance.IsContentUnlocked(offer.UnlockId)
            : ProgressionManager.Instance.GetUpgradeLevel(offer.UnlockId) > 0;
    }

    private bool IsMaxed(ShopOfferData offer)
    {
        if (offer.OfferType == ShopOfferType.ContentUnlock)
            return IsPurchased(offer);

        return ProgressionManager.Instance.GetUpgradeLevel(offer.UnlockId) >= offer.MaximumLevel;
    }
}
