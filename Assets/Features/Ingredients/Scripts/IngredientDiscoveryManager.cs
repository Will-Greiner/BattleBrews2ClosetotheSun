using System;
using System.Collections.Generic;
using UnityEngine;

public class IngredientDiscoveryManager : MonoBehaviour
{
    public static IngredientDiscoveryManager Instance { get; private set; }

    private const string SaveKeyPrefix = "BattleBrew.IngredientProperty.";

    [Header("Saving")]
    [Tooltip("Disable during development to reset discoveries whenever Play Mode restarts.")]
    [SerializeField] private bool persistDiscoveries;

    [Header("Reset")]
    [Tooltip("Include every ingredient that should be cleared by Reset All Discovery.")]
    [SerializeField] private List<IngredientData> knownIngredients = new();

    private readonly HashSet<string> sessionDiscoveries = new();

    public event Action<IngredientData, int, ItemPropertyData> PropertyDiscovered;
    public event Action<IngredientData> IngredientDiscoveryReset;
    public event Action AllDiscoveryReset;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool DiscoverProperty(IngredientData ingredient, int level)
    {
        if (!IsValidLevel(ingredient, level))
            return false;

        string saveKey = GetSaveKey(ingredient, level);

        if (IsPropertyDiscovered(ingredient, level))
            return false;

        sessionDiscoveries.Add(saveKey);

        if (persistDiscoveries)
        {
            PlayerPrefs.SetInt(saveKey, 1);
            PlayerPrefs.Save();
        }

        ItemPropertyData property = ingredient.GetPropertyAtLevel(level);
        PropertyDiscovered?.Invoke(ingredient, level, property);
        return true;
    }

    public bool IsPropertyDiscovered(IngredientData ingredient, int level)
    {
        if (!IsValidLevel(ingredient, level))
            return false;

        string saveKey = GetSaveKey(ingredient, level);

        if (sessionDiscoveries.Contains(saveKey))
            return true;

        return persistDiscoveries && PlayerPrefs.GetInt(saveKey, 0) == 1;
    }

    public bool IsPropertyDiscovered(IngredientData ingredient, ItemPropertyData property)
    {
        if (ingredient == null || property == null)
            return false;

        int level = ingredient.GetPropertyLevel(property);
        return level > 0 && IsPropertyDiscovered(ingredient, level);
    }

    public void ResetIngredientDiscovery(IngredientData ingredient)
    {
        if (ingredient == null)
            return;

        for (int level = 1; level <= 3; level++)
        {
            string saveKey = GetSaveKey(ingredient, level);
            sessionDiscoveries.Remove(saveKey);
            PlayerPrefs.DeleteKey(saveKey);
        }

        PlayerPrefs.Save();
        IngredientDiscoveryReset?.Invoke(ingredient);
    }

    [ContextMenu("Reset All Discovery")]
    public void ResetAllDiscovery()
    {
        sessionDiscoveries.Clear();

        foreach (IngredientData ingredient in knownIngredients)
        {
            if (ingredient == null)
                continue;

            for (int level = 1; level <= 3; level++)
                PlayerPrefs.DeleteKey(GetSaveKey(ingredient, level));
        }

        PlayerPrefs.Save();
        AllDiscoveryReset?.Invoke();
        Debug.Log("All ingredient property discovery has been reset.", this);
    }

    private bool IsValidLevel(IngredientData ingredient, int level)
    {
        return ingredient != null && !string.IsNullOrWhiteSpace(ingredient.PersistentId) && level >= 1 && level <= 3 && ingredient.GetPropertyAtLevel(level) != null;
    }

    private string GetSaveKey(IngredientData ingredient, int level)
    {
        return $"{SaveKeyPrefix}{ingredient.PersistentId}.Level{level}";
    }

    private void OnValidate()
    {
        for (int i = knownIngredients.Count - 1; i >= 0; i--)
        {
            if (knownIngredients[i] == null || knownIngredients.IndexOf(knownIngredients[i]) != i)
                knownIngredients.RemoveAt(i);
        }
    }
}