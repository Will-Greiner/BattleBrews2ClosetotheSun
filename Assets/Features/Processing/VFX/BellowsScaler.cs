using UnityEngine;

public class BellowsScaler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private IngredientProcessingStation station;
    [SerializeField] private GameObject mainFire;
    [SerializeField] private ParticleSystem bellowsEffect;

    [Header("Fire Scale")]
    [SerializeField, Range(0f, 1f)] private float minimumScale = 0.5f;
    [SerializeField, Min(1f)] private float maximumScale = 1.75f;
    [SerializeField, Min(0f)] private float scaleResponse = 8f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Bellows Burst")]
    [SerializeField, Min(0)] private int burstParticleCount = 50;

    private Vector3 fullFireScale;
    private float targetProgress;

    public float FirePercentage => targetProgress;

    private void Awake()
    {
        if (station == null)
            station = GetComponentInParent<IngredientProcessingStation>();

        if (mainFire != null)
        {
            fullFireScale = mainFire.transform.localScale;
            ConfigureFireScaling();
        }

        targetProgress = station != null ? station.NormalizedProgress : 0f;
        ApplyScale(targetProgress);
    }

    private void OnEnable()
    {
        if (station != null)
            station.ProgressChanged += HandleProgressChanged;
    }

    private void Start()
    {
        if (station == null)
            Debug.LogError($"{name}: BellowsScaler could not find an IngredientProcessingStation in its parents.", this);
    }

    private void OnDisable()
    {
        if (station != null)
            station.ProgressChanged -= HandleProgressChanged;
    }

    private void Update()
    {
        if (mainFire == null)
            return;

        float currentMultiplier = GetCurrentScaleMultiplier();
        float targetMultiplier = GetScaleMultiplier(targetProgress);
        float nextMultiplier = scaleResponse <= 0f ? targetMultiplier : Mathf.Lerp(currentMultiplier, targetMultiplier, 1f - Mathf.Exp(-scaleResponse * Time.deltaTime));
        mainFire.transform.localScale = fullFireScale * nextMultiplier;
    }

    public void Stoke()
    {
        EmitEmberBurst();
        targetProgress = station != null ? station.NormalizedProgress : targetProgress;
    }

    private void HandleProgressChanged(float currentProgress, float requiredProgress)
    {
        float newProgress = requiredProgress > 0f ? Mathf.Clamp01(currentProgress / requiredProgress) : 0f;

        targetProgress = newProgress;
    }

    private void EmitEmberBurst()
    {
        if (bellowsEffect != null && burstParticleCount > 0)
        {
            bellowsEffect.Play(true);
            bellowsEffect.Emit(burstParticleCount);
        }
    }

    private void ConfigureFireScaling()
    {
        ParticleSystem[] fireSystems = mainFire.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem fireSystem in fireSystems)
        {
            ParticleSystem.MainModule main = fireSystem.main;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        }
    }

    private void ApplyScale(float normalizedProgress)
    {
        if (mainFire != null)
            mainFire.transform.localScale = fullFireScale * GetScaleMultiplier(normalizedProgress);
    }

    private float GetCurrentScaleMultiplier()
    {
        if (fullFireScale.sqrMagnitude <= Mathf.Epsilon)
            return 0f;

        return mainFire.transform.localScale.magnitude / fullFireScale.magnitude;
    }

    private float GetScaleMultiplier(float normalizedProgress)
    {
        float curvedProgress = scaleCurve.Evaluate(Mathf.Clamp01(normalizedProgress));
        return Mathf.Lerp(minimumScale, maximumScale, curvedProgress);
    }
}
