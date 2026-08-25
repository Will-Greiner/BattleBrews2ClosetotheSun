using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopOfferRowUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button purchaseButton;

    private ShopOfferData offer;
    private Action<ShopOfferData> purchaseRequested;

    private void Awake()
    {
        if (purchaseButton != null) purchaseButton.onClick.AddListener(HandlePurchase);
    }

    private void OnDestroy()
    {
        if (purchaseButton != null) purchaseButton.onClick.RemoveListener(HandlePurchase);
    }

    public void Display(ShopOfferData value, bool canAfford, Action<ShopOfferData> onPurchase)
    {
        offer = value;
        purchaseRequested = onPurchase;

        if (icon != null)
        {
            icon.sprite = offer != null ? offer.Icon : null;
            icon.enabled = icon.sprite != null;
        }

        if (nameText != null) nameText.text = offer != null ? offer.DisplayName : string.Empty;
        if (descriptionText != null) descriptionText.text = offer != null ? offer.Description : string.Empty;
        if (priceText != null) priceText.text = offer != null ? $"{offer.Price} coins" : string.Empty;
        if (purchaseButton != null) purchaseButton.interactable = offer != null && canAfford;
    }

    private void HandlePurchase()
    {
        if (offer != null) purchaseRequested?.Invoke(offer);
    }

}
