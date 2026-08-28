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

    private static readonly int DrainId = Shader.PropertyToID("_Drain");

    private Renderer liquidRenderer;
    private float nextHandSplashTime;

    private void Awake()
    {
        liquidRenderer = FindLiquidRenderer();

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

        Animator animator = splash.GetComponentInChildren<Animator>();

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
}
