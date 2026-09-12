using UnityEngine;

[RequireComponent(typeof(CauldronController))]
public class CauldronSplashController : MonoBehaviour
{
    [Header("Splash Prefab")]
    [SerializeField] private GameObject splashPrefab;
    [Min(0f)] [SerializeField] private float itemSplashScale = 1f;
    [Min(0f)] [SerializeField] private float handSplashScale = 0.55f;
    [Min(0.01f)] [SerializeField] private float splashLifetime = 0.65f;
    [Min(0f)] [SerializeField] private float surfaceOffset = 0.065f;

    [Header("Hand Contact")]
    [Min(0f)] [SerializeField] private float handSplashCooldown = 0.2f;

    [Header("Audio")]
    [SerializeField] private AudioCue splashCue;

    private static readonly int DrainId = Shader.PropertyToID("_Drain");

    private Renderer liquidRenderer;
    private float nextHandSplashTime;
    private CauldronLiquidController liquidController;
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int BottomColorId = Shader.PropertyToID("_Bottom_Color");
    private static readonly int MediumColorId = Shader.PropertyToID("_Medium_Color");
    private static readonly int HighlightColorId = Shader.PropertyToID("_Highlight_Color");

    private void Awake()
    {
        liquidRenderer = FindLiquidRenderer();
        liquidController = GetComponent<CauldronLiquidController>();

        if (liquidRenderer == null)
            Debug.LogError($"{name}: Could not find the cauldron liquid renderer for splash placement.", this);

        if (splashPrefab == null)
            Debug.LogError($"{name}: Assign a splash prefab to enable cauldron splashes.", this);
    }

    public void PlayItemSplash(Vector3 worldPosition)
    {
        SpawnSplash(worldPosition, itemSplashScale);
    }

    public void TryPlayHandSplash(Vector3 worldPosition)
    {
        if (Time.time < nextHandSplashTime)
            return;

        nextHandSplashTime = Time.time + handSplashCooldown;
        SpawnSplash(worldPosition, handSplashScale);
    }

    private void SpawnSplash(Vector3 contactPosition, float scale)
    {
        if (splashPrefab == null || liquidRenderer == null || scale <= 0f)
            return;

        Bounds surface = liquidRenderer.bounds;
        Vector3 position = contactPosition;
        position.x = Mathf.Clamp(position.x, surface.min.x, surface.max.x);
        position.y = surface.max.y + surfaceOffset;
        position.z = Mathf.Clamp(position.z, surface.min.z, surface.max.z);

        Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        GameObject splash = Instantiate(splashPrefab, position, rotation, transform);
        splash.transform.localScale *= scale;
        splash.name = "Splash";
        ApplySplashColor(splash);

        Animator animator = splash.GetComponentInChildren<Animator>();

        AudioManager.Instance.PlayAtPosition(splashCue, contactPosition);

        if (animator != null)
            animator.Play(0, 0, 0f);

        Destroy(splash, splashLifetime);
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

    private void ApplySplashColor(GameObject splash)
    {
        if (splash == null || liquidController == null)
            return;

        Color liquidColor = liquidController.MixtureAmount > 0.001f ? liquidController.CurrentLiquidColor : liquidController.StartingLiquidColor;
        Color bottomColor = Color.Lerp(liquidColor, Color.black, 0.25f);
        Color mediumColor = Color.Lerp(liquidColor, Color.white, 0.08f);
        Color highlightColor = Color.Lerp(liquidColor, Color.white, 0.55f);

        bottomColor.a = liquidColor.a;
        mediumColor.a = liquidColor.a;
        highlightColor.a = liquidColor.a;

        MaterialPropertyBlock properties = new();

        foreach (Renderer splashRenderer in splash.GetComponentsInChildren<Renderer>(true))
        {
            splashRenderer.GetPropertyBlock(properties);
            properties.SetColor(ColorId, liquidColor);
            properties.SetColor(BottomColorId, bottomColor);
            properties.SetColor(MediumColorId, mediumColor);
            properties.SetColor(HighlightColorId, highlightColor);
            splashRenderer.SetPropertyBlock(properties);
        }
    }
}
