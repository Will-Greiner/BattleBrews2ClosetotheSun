using System;
using System.Collections.Generic;
using UnityEngine;

public enum ShopOfferType
{
    ContentUnlock,
    PermanentUpgrade
}

[CreateAssetMenu(fileName = "ShopOffer", menuName = "Battle Brews/Shop Offer")]
public class ShopOfferData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string displayName;
    [TextArea] [SerializeField] private string description;
    [SerializeField] private Sprite icon;
    [HideInInspector] [SerializeField] private string persistentId;

    [Header("Purchase")]
    [Min(0)] [SerializeField] private int price = 100;
    [Min(1)] [SerializeField] private int firstAvailableRound = 1;
    [SerializeField] private ShopOfferType offerType;
    [SerializeField] private string unlockId;
    [Min(1)] [SerializeField] private int maximumLevel = 1;
    [SerializeField] private List<ShopOfferData> prerequisites = new();

    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;
    public string PersistentId => persistentId;
    public int Price => price;
    public int FirstAvailableRound => firstAvailableRound;
    public ShopOfferType OfferType => offerType;
    public string UnlockId => unlockId;
    public int MaximumLevel => maximumLevel;
    public IReadOnlyList<ShopOfferData> Prerequisites => prerequisites;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(persistentId))
            persistentId = Guid.NewGuid().ToString("N");

        firstAvailableRound = Mathf.Max(1, firstAvailableRound);
        maximumLevel = Mathf.Max(1, maximumLevel);
        price = Mathf.Max(0, price);
    }
}
