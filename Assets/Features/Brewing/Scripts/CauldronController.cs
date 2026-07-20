using System;
using System.Collections.Generic;
using UnityEngine;

public class CauldronController : MonoBehaviour
{
    [Header("Recipes")]
    [SerializeField] private PotionDatabase potionDatabase;
    [SerializeField] private PotionData grossPotion;
    [SerializeField] private PotionData unstablePotion;

    [Header("Capacity")]
    [SerializeField] private int maxContributions = 15;

    [Header("Potion Spawning")]
    [SerializeField] private Transform potionSpawnPoint;

    [Header("Ingredient Effects")]
    [SerializeField] private ParticleSystem ingredientAddedParticles;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip ingredientAddedClip;

    [Header("Potion Effects")]
    [SerializeField] private ParticleSystem goodPotionSpawnParticles;
    [SerializeField] private ParticleSystem badPotionSpawnParticles;
    [SerializeField] private AudioClip goodPotionClip;
    [SerializeField] private AudioClip badPotionClip;

    [Header("Current Ingredients")]
    [SerializeField] private List<CauldronContribution> contributions = new();

    public event Action ContributionsChanged;
    public event Action<PotionData> PotionCreated;

    public IReadOnlyList<CauldronContribution> Contributions => contributions;
    public int ContributionCount => contributions.Count;

    public bool CanAcceptIngredient(GrabbableItem item)
    {
        if (item == null || !item.gameObject.activeInHierarchy || contributions.Count >= maxContributions)
            return false;

        IngredientItem ingredientItem = item.GetComponent<IngredientItem>();
        return ingredientItem != null && ingredientItem.Data != null;
    }

    public bool TryAddIngredient(GrabbableItem item)
    {
        if (!CanAcceptIngredient(item))
            return false;

        IngredientItem ingredientItem = item.GetComponent<IngredientItem>();

        if (ingredientItem == null || ingredientItem.Data == null)
            return false;

        contributions.Add(new CauldronContribution(ingredientItem.Data));
        PlayIngredientAddedEffects();
        ContributionsChanged?.Invoke();

        item.gameObject.SetActive(false);
        Destroy(item.gameObject);
        return true;
    }

    [Obsolete("Use TryAddIngredient. Contributions are now assigned automatically.")]
    public bool TryAddContribution(GrabbableItem item, CauldronContribution contribution)
    {
        return TryAddIngredient(item);
    }

    public RecipeMatchResult EvaluatePotion(PotionData potion)
    {
        return ContributionRecipeMatcher.Evaluate(potion, contributions);
    }

    public bool Stir()
    {
        if (contributions.Count == 0)
            return false;

        PotionData exactMatch = ContributionRecipeMatcher.FindExactMatch(potionDatabase, contributions);

        if (exactMatch != null)
        {
            CreatePotion(exactMatch, false);
            return true;
        }

        PotionData overfilledMatch = ContributionRecipeMatcher.FindOverfilledMatch(potionDatabase, contributions);

        if (overfilledMatch != null)
        {
            CreatePotion(unstablePotion, true);
            return true;
        }

        CreatePotion(grossPotion, true);
        return true;
    }

    public void ClearCauldron()
    {
        contributions.Clear();
        ContributionsChanged?.Invoke();
    }

    private void CreatePotion(PotionData potion, bool isFailure)
    {
        if (potion == null)
        {
            Debug.LogError($"{name}: Cannot create a potion because its PotionData reference is missing.", this);
            return;
        }

        if (potion.Prefab == null)
        {
            Debug.LogError($"{name}: {potion.PotionName} does not have a prefab assigned.", potion);
            return;
        }

        if (potionSpawnPoint == null)
        {
            Debug.LogError($"{name}: No potion spawn point has been assigned.", this);
            return;
        }

        GameObject spawnedPotion = Instantiate(potion.Prefab, potionSpawnPoint.position, potionSpawnPoint.rotation);
        PotionItem potionItem = spawnedPotion.GetComponent<PotionItem>();

        if (potionItem != null)
            potionItem.Initialize(potion);
        else
            Debug.LogError($"{spawnedPotion.name} does not have a PotionItem component.", spawnedPotion);

        if (!isFailure)
            potion.SetDiscovered(true);

        PlayPotionCreatedEffects(isFailure);
        ClearCauldron();
        PotionCreated?.Invoke(potion);
    }

    private void PlayIngredientAddedEffects()
    {
        if (ingredientAddedParticles != null)
            ingredientAddedParticles.Play();

        if (audioSource != null && ingredientAddedClip != null)
            audioSource.PlayOneShot(ingredientAddedClip);
    }

    private void PlayPotionCreatedEffects(bool isFailure)
    {
        ParticleSystem particles = isFailure ? badPotionSpawnParticles : goodPotionSpawnParticles;
        AudioClip clip = isFailure ? badPotionClip : goodPotionClip;

        if (particles != null)
            particles.Play();

        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}