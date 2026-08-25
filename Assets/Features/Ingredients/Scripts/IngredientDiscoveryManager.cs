using System;
using System.Collections.Generic;
using UnityEngine;

public class IngredientDiscoveryManager : MonoBehaviour
{
    public static IngredientDiscoveryManager Instance { get; private set; }

    [Header("Reset")]
    [SerializeField] private List<IngredientData> knownIngredients = new();

    public event Action<IngredientData, int, ItemPropertyData> PropertyDiscovered;
    public event Action<IngredientData> IngredientDiscoveryReset;
    public event Action AllDiscoveryReset;

    private bool subscribed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable() => Subscribe();
    private void Start() => Subscribe();

    private void OnDisable()
    {
        if (subscribed && ProgressionManager.Instance != null)
            ProgressionManager.Instance.PropertyDiscovered -= HandlePropertyDiscovered;
        subscribed = false;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public bool DiscoverProperty(IngredientData ingredient, int level) =>
        ProgressionManager.Instance != null && ProgressionManager.Instance.DiscoverProperty(ingredient, level);

    public bool IsPropertyDiscovered(IngredientData ingredient, int level) =>
        ProgressionManager.Instance != null && ProgressionManager.Instance.IsPropertyDiscovered(ingredient, level);

    public bool IsPropertyDiscovered(IngredientData ingredient, ItemPropertyData property)
    {
        int level = ingredient != null ? ingredient.GetPropertyLevel(property) : 0;
        return level > 0 && IsPropertyDiscovered(ingredient, level);
    }

    public void ResetIngredientDiscovery(IngredientData ingredient)
    {
        if (ingredient == null || ProgressionManager.Instance == null) return;
        ProgressionManager.Instance.ResetIngredientDiscovery(ingredient);
        IngredientDiscoveryReset?.Invoke(ingredient);
    }

    [ContextMenu("Reset All Discovery")]
    public void ResetAllDiscovery()
    {
        if (ProgressionManager.Instance == null) return;
        ProgressionManager.Instance.ResetAllDiscovery();
        AllDiscoveryReset?.Invoke();
    }

    private void Subscribe()
    {
        if (subscribed || ProgressionManager.Instance == null) return;
        ProgressionManager.Instance.PropertyDiscovered += HandlePropertyDiscovered;
        subscribed = true;
    }

    private void HandlePropertyDiscovered(IngredientData ingredient, int level, ItemPropertyData property) =>
        PropertyDiscovered?.Invoke(ingredient, level, property);

    private void OnValidate()
    {
        for (int i = knownIngredients.Count - 1; i >= 0; i--)
            if (knownIngredients[i] == null || knownIngredients.IndexOf(knownIngredients[i]) != i) knownIngredients.RemoveAt(i);
    }
}
