using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private ShopPhaseController phaseController;
    [SerializeField] private TMP_Text currencyText;
    [SerializeField] private Transform offerContainer;
    [SerializeField] private ShopOfferRowUI offerRowPrefab;
    [SerializeField] private Button closeButton;

    private readonly List<ShopOfferRowUI> rows = new();

    private void Awake()
    {
        if (closeButton != null) closeButton.onClick.AddListener(HandleClose);
    }

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void Start()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        if (shopManager != null) shopManager.OffersChanged -= Refresh;
        if (ProgressionManager.Instance != null) ProgressionManager.Instance.CurrencyChanged -= HandleCurrencyChanged;
    }

    private void OnDestroy()
    {
        if (closeButton != null) closeButton.onClick.RemoveListener(HandleClose);
    }

    public void Refresh()
    {
        ClearRows();
        ProgressionManager progression = ProgressionManager.Instance;

        if (currencyText != null)
            currencyText.text = progression != null ? $"Coins: {progression.Currency}" : "Coins: 0";

        if (shopManager == null || offerContainer == null || offerRowPrefab == null || progression == null)
            return;

        int round = GameManager.Instance != null ? GameManager.Instance.CurrentRound : 1;

        foreach (ShopOfferData offer in shopManager.Offers)
        {
            if (!shopManager.IsOfferAvailable(offer, round)) continue;
            ShopOfferRowUI row = Instantiate(offerRowPrefab, offerContainer);
            row.Display(offer, shopManager.CanPurchase(offer, round), HandlePurchase);
            rows.Add(row);
        }
    }

    private void Subscribe()
    {
        if (shopManager != null)
        {
            shopManager.OffersChanged -= Refresh;
            shopManager.OffersChanged += Refresh;
        }

        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.Instance.CurrencyChanged -= HandleCurrencyChanged;
            ProgressionManager.Instance.CurrencyChanged += HandleCurrencyChanged;
        }
    }

    private void HandlePurchase(ShopOfferData offer)
    {
        int round = GameManager.Instance != null ? GameManager.Instance.CurrentRound : 1;
        shopManager?.TryPurchase(offer, round);
    }

    private void HandleCurrencyChanged(int currency) => Refresh();
    private void HandleClose() => phaseController?.CloseShop();

    private void ClearRows()
    {
        foreach (ShopOfferRowUI row in rows)
            if (row != null) Destroy(row.gameObject);
        rows.Clear();
    }

}
