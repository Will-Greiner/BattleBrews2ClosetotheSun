using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CauldronController))]
public class CauldronLiquidController : MonoBehaviour
{
    [Header("Animation")]
    [Min(0.01f)] [SerializeField] private float drainDuration = 0.8f;
    [Min(0f)] [SerializeField] private float emptyPause = 0.15f;
    [Min(0.01f)] [SerializeField] private float refillDuration = 1f;
    [Min(0.01f)] [SerializeField] private float responseSpeed = 4f;
    [Min(0f)] [SerializeField] private float drainSinkDistance = 0.3f;

    [Header("Ingredient Color")]
    [Min(0.01f)] [SerializeField] private float colorResponseSpeed = 3f;
    [Range(0f, 1f)] [SerializeField] private float minimumSaturation = 0.68f;
    [Range(0f, 1f)] [SerializeField] private float minimumBrightness = 0.62f;
    [SerializeField] private Color grossPotionColor = new Color(0.28f, 0.1f, 0.025f, 1f);
    [SerializeField] private Color unstablePotionColor = new Color(0.9f, 0.12f, 1f, 1f);

    private static readonly int ActivityId = Shader.PropertyToID("_Activity");
    private static readonly int StirStrengthId = Shader.PropertyToID("_StirStrength");
    private static readonly int ReactionId = Shader.PropertyToID("_Reaction");
    private static readonly int ReactionColorId = Shader.PropertyToID("_ReactionColor");
    private static readonly int DrainId = Shader.PropertyToID("_Drain");
    private static readonly int MixtureColorId = Shader.PropertyToID("_MixtureColor");
    private static readonly int MixtureAmountId = Shader.PropertyToID("_MixtureAmount");

    private CauldronController cauldron;
    private StirringStick stirringStick;
    private Renderer liquidRenderer;
    private MaterialPropertyBlock properties;
    private Coroutine drainRoutine;
    private Coroutine reactionRoutine;
    private int previousContributionCount;
    private float activity;
    private float stirStrength;
    private float drain;
    private float reaction;
    private Color reactionColor = Color.white;
    private Transform liquidTransform;
    private Vector3 fullLiquidLocalPosition;
    private Color mixtureColor;
    private Color targetMixtureColor;
    private float mixtureAmount;
    private float targetMixtureAmount;

    public float VisualActivity => activity;
    public float DrainAmount => drain;
    public Color CurrentMixtureColor => mixtureColor;
    public float MixtureAmount => mixtureAmount;

    private void Awake()
    {
        cauldron = GetComponent<CauldronController>();
        stirringStick = GetComponentInChildren<StirringStick>(true);
        liquidRenderer = FindLiquidRenderer();
        properties = new MaterialPropertyBlock();
        previousContributionCount = cauldron.ContributionCount;
        targetMixtureColor = CalculateMixtureColor();
        mixtureColor = targetMixtureColor;
        targetMixtureAmount = cauldron.ContributionCount > 0 ? 1f : 0f;
        mixtureAmount = targetMixtureAmount;

        if (liquidRenderer == null)
            Debug.LogError($"{name}: Could not find a child renderer using the cauldron liquid shader.", this);
        else
        {
            liquidTransform = liquidRenderer.transform;
            fullLiquidLocalPosition = liquidTransform.localPosition;
        }
    }

    private void OnEnable()
    {
        if (cauldron == null)
            return;

        cauldron.ContributionsChanged += HandleContributionsChanged;
        cauldron.PotionCreated += HandlePotionCreated;
    }

    private void OnDisable()
    {
        if (cauldron != null)
        {
            cauldron.ContributionsChanged -= HandleContributionsChanged;
            cauldron.PotionCreated -= HandlePotionCreated;
        }

        if (liquidTransform != null)
            liquidTransform.localPosition = fullLiquidLocalPosition;
    }

    private void Update()
    {
        if (liquidRenderer == null || cauldron == null)
            return;

        float targetActivity = cauldron.Capacity > 0
            ? cauldron.ContributionCount / (float)cauldron.Capacity
            : 0f;
        float targetStir = stirringStick != null && stirringStick.IsStirring
            ? stirringStick.StirProgress
            : 0f;

        activity = Mathf.MoveTowards(activity, targetActivity, responseSpeed * Time.deltaTime);
        stirStrength = targetStir;
        mixtureColor = Color.Lerp(mixtureColor, targetMixtureColor, 1f - Mathf.Exp(-colorResponseSpeed * Time.deltaTime));
        mixtureAmount = Mathf.MoveTowards(mixtureAmount, targetMixtureAmount, colorResponseSpeed * Time.deltaTime);

        if (liquidTransform != null)
            liquidTransform.localPosition = fullLiquidLocalPosition + Vector3.down * (drain * drainSinkDistance);

        liquidRenderer.GetPropertyBlock(properties);
        properties.SetFloat(ActivityId, activity);
        properties.SetFloat(StirStrengthId, stirStrength);
        properties.SetFloat(ReactionId, reaction);
        properties.SetColor(ReactionColorId, reactionColor);
        properties.SetFloat(DrainId, drain);
        properties.SetColor(MixtureColorId, mixtureColor);
        properties.SetFloat(MixtureAmountId, mixtureAmount);
        liquidRenderer.SetPropertyBlock(properties);
    }

    private Renderer FindLiquidRenderer()
    {
        foreach (Renderer candidate in GetComponentsInChildren<Renderer>(true))
        {
            Material material = candidate.sharedMaterial;

            if (material != null && material.HasProperty(DrainId))
                return candidate;
        }

        return null;
    }

    private void HandleContributionsChanged()
    {
        int currentCount = cauldron.ContributionCount;

        targetMixtureColor = CalculateMixtureColor();
        targetMixtureAmount = currentCount > 0 ? 1f : 0f;

        if (previousContributionCount > 0 && currentCount == 0)
            StartDrainAndRefill();

        previousContributionCount = currentCount;
    }

    private void HandlePotionCreated(PotionData potion)
    {
        if (reactionRoutine != null)
            StopCoroutine(reactionRoutine);

        if (cauldron.IsGrossPotion(potion))
            reactionColor = grossPotionColor;
        else if (cauldron.IsUnstablePotion(potion))
            reactionColor = unstablePotionColor;
        else
            reactionColor = mixtureColor;

        reactionRoutine = StartCoroutine(ReactionRoutine());
    }

    private Color CalculateMixtureColor()
    {
        if (cauldron == null || cauldron.ContributionCount == 0)
            return mixtureColor == default ? new Color(0.35f, 0.8f, 0.3f, 1f) : mixtureColor;

        float hueX = 0f;
        float hueY = 0f;
        float saturation = 0f;
        float brightness = 0f;
        Color latestColor = Color.green;
        int colorCount = 0;

        foreach (CauldronContribution contribution in cauldron.Contributions)
        {
            IngredientData ingredient = contribution != null ? contribution.SourceIngredient : null;

            if (ingredient == null)
                continue;

            latestColor = ingredient.BrewColor;
            Color.RGBToHSV(latestColor, out float hue, out float ingredientSaturation, out float ingredientBrightness);
            float angle = hue * Mathf.PI * 2f;
            hueX += Mathf.Cos(angle);
            hueY += Mathf.Sin(angle);
            saturation += ingredientSaturation;
            brightness += ingredientBrightness;
            colorCount++;
        }

        if (colorCount == 0)
            return mixtureColor;

        float mixedHue;

        if (new Vector2(hueX, hueY).sqrMagnitude < 0.01f)
        {
            Color.RGBToHSV(latestColor, out mixedHue, out _, out _);
        }
        else
        {
            mixedHue = Mathf.Atan2(hueY, hueX) / (Mathf.PI * 2f);

            if (mixedHue < 0f)
                mixedHue += 1f;
        }

        float mixedSaturation = Mathf.Max(minimumSaturation, saturation / colorCount);
        float mixedBrightness = Mathf.Max(minimumBrightness, brightness / colorCount);
        return Color.HSVToRGB(mixedHue, mixedSaturation, mixedBrightness);
    }

    private void StartDrainAndRefill()
    {
        if (drainRoutine != null)
            StopCoroutine(drainRoutine);

        drainRoutine = StartCoroutine(DrainAndRefillRoutine());
    }

    private IEnumerator DrainAndRefillRoutine()
    {
        yield return AnimateDrain(drain, 1f, drainDuration);

        if (emptyPause > 0f)
            yield return new WaitForSeconds(emptyPause);

        yield return AnimateDrain(1f, 0f, refillDuration);
        drain = 0f;
        drainRoutine = null;
    }

    private IEnumerator AnimateDrain(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            drain = Mathf.Lerp(from, to, t);
            yield return null;
        }

        drain = to;
    }

    private IEnumerator ReactionRoutine()
    {
        const float duration = 0.7f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            reaction = t < 0.15f ? t / 0.15f : 1f - (t - 0.15f) / 0.85f;
            yield return null;
        }

        reaction = 0f;
        reactionRoutine = null;
    }
}
