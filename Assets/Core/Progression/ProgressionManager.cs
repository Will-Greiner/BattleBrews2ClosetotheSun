using System;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }

    [Header("Save")]
    [Range(1, SaveManager.SlotCount)] [SerializeField] private int activeSaveSlot = 1;
    [SerializeField] private bool loadOnAwake = true;

    [Header("New Save Defaults")]
    [Min(0)] [SerializeField] private int startingCurrency;
    [SerializeField] private List<string> initiallyUnlockedContentIds = new();

    private readonly HashSet<string> unlockedContentIds = new();
    private readonly HashSet<string> discoveredPropertyIds = new();
    private readonly Dictionary<string, int> upgradeLevels = new();
    private string createdUtc;

    public event Action<int> CurrencyChanged;
    public event Action<string> ContentUnlocked;
    public event Action<IngredientData, int, ItemPropertyData> PropertyDiscovered;
    public event Action<string, int> UpgradeLevelChanged;
    public event Action ProgressionLoaded;

    public int Currency { get; private set; }
    public int ActiveSaveSlot => activeSaveSlot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (loadOnAwake)
            LoadSlot(activeSaveSlot);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void LoadSlot(int slot)
    {
        activeSaveSlot = Mathf.Clamp(slot, 1, SaveManager.SlotCount);
        SaveGameData data = SaveManager.Load(activeSaveSlot);

        if (data == null)
            CreateNewSlot(activeSaveSlot);
        else
            Apply(data);

        ProgressionLoaded?.Invoke();
        CurrencyChanged?.Invoke(Currency);
    }

    public void CreateNewSlot(int slot)
    {
        activeSaveSlot = Mathf.Clamp(slot, 1, SaveManager.SlotCount);
        unlockedContentIds.Clear();
        discoveredPropertyIds.Clear();
        upgradeLevels.Clear();
        Currency = startingCurrency;
        createdUtc = DateTime.UtcNow.ToString("O");

        foreach (string id in initiallyUnlockedContentIds)
        {
            if (!string.IsNullOrWhiteSpace(id))
                unlockedContentIds.Add(id);
        }

        SaveNow();
    }

    [ContextMenu("Reset Active Save")]
    public void ResetActiveSave()
    {
        SaveManager.Delete(activeSaveSlot);
        CreateNewSlot(activeSaveSlot);
        ProgressionLoaded?.Invoke();
        CurrencyChanged?.Invoke(Currency);
        Debug.Log($"Reset Battle Brews save slot {activeSaveSlot}.", this);
    }

    public bool IsContentUnlocked(string id) => !string.IsNullOrWhiteSpace(id) && unlockedContentIds.Contains(id);

    public bool UnlockContent(string id, bool save = true)
    {
        if (string.IsNullOrWhiteSpace(id) || !unlockedContentIds.Add(id))
            return false;

        ContentUnlocked?.Invoke(id);

        if (save)
            SaveNow();

        return true;
    }

    public bool CanAfford(int amount) => amount >= 0 && Currency >= amount;

    public bool TrySpendCurrency(int amount)
    {
        if (!CanAfford(amount))
            return false;

        Currency -= amount;
        CurrencyChanged?.Invoke(Currency);
        return true;
    }

    public void AddCurrency(int amount, bool save = true)
    {
        if (amount <= 0)
            return;

        Currency += amount;
        CurrencyChanged?.Invoke(Currency);

        if (save)
            SaveNow();
    }

    public bool IsPropertyDiscovered(IngredientData ingredient, int level)
    {
        return ingredient != null && discoveredPropertyIds.Contains(GetPropertyKey(ingredient, level));
    }

    public bool DiscoverProperty(IngredientData ingredient, int level)
    {
        ItemPropertyData property = ingredient != null ? ingredient.GetPropertyAtLevel(level) : null;

        if (property == null || string.IsNullOrWhiteSpace(ingredient.PersistentId))
            return false;

        if (!discoveredPropertyIds.Add(GetPropertyKey(ingredient, level)))
            return false;

        PropertyDiscovered?.Invoke(ingredient, level, property);
        SaveNow();
        return true;
    }

    public void ResetIngredientDiscovery(IngredientData ingredient, bool save = true)
    {
        if (ingredient == null)
            return;

        for (int level = 1; level <= ingredient.PropertyCount; level++)
            discoveredPropertyIds.Remove(GetPropertyKey(ingredient, level));

        if (save)
            SaveNow();
    }

    public void ResetAllDiscovery()
    {
        discoveredPropertyIds.Clear();
        SaveNow();
    }

    public int GetUpgradeLevel(string id) => !string.IsNullOrWhiteSpace(id) && upgradeLevels.TryGetValue(id, out int level) ? level : 0;

    public void SetUpgradeLevel(string id, int level, bool save = true)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;

        level = Mathf.Max(0, level);
        upgradeLevels[id] = level;
        UpgradeLevelChanged?.Invoke(id, level);

        if (save)
            SaveNow();
    }

    public void SaveNow()
    {
        SaveGameData data = new()
        {
            createdUtc = string.IsNullOrWhiteSpace(createdUtc) ? DateTime.UtcNow.ToString("O") : createdUtc,
            lastPlayedUtc = DateTime.UtcNow.ToString("O"),
            currency = Currency,
            currentRound = GameManager.Instance != null ? GameManager.Instance.CurrentRound : 0,
            lives = GameManager.Instance != null ? GameManager.Instance.Lives : 0,
            unlockedContentIds = new List<string>(unlockedContentIds),
            discoveredPropertyIds = new List<string>(discoveredPropertyIds)
        };

        foreach (KeyValuePair<string, int> upgrade in upgradeLevels)
            data.upgrades.Add(new UpgradeSaveData { id = upgrade.Key, level = upgrade.Value });

        SaveManager.Save(activeSaveSlot, data);
    }

    private void Apply(SaveGameData data)
    {
        unlockedContentIds.Clear();
        discoveredPropertyIds.Clear();
        upgradeLevels.Clear();
        Currency = Mathf.Max(0, data.currency);
        createdUtc = data.createdUtc;

        if (data.unlockedContentIds != null)
            foreach (string id in data.unlockedContentIds)
                if (!string.IsNullOrWhiteSpace(id)) unlockedContentIds.Add(id);

        if (data.discoveredPropertyIds != null)
            foreach (string id in data.discoveredPropertyIds)
                if (!string.IsNullOrWhiteSpace(id)) discoveredPropertyIds.Add(id);

        if (data.upgrades != null)
            foreach (UpgradeSaveData upgrade in data.upgrades)
                if (upgrade != null && !string.IsNullOrWhiteSpace(upgrade.id)) upgradeLevels[upgrade.id] = Mathf.Max(0, upgrade.level);
    }

    private static string GetPropertyKey(IngredientData ingredient, int level) => $"{ingredient.PersistentId}:property:{level}";
}
