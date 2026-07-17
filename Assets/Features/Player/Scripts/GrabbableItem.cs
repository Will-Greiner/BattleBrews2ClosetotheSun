using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrabbableItem : MonoBehaviour
{
    [SerializeField] private string displayName = "Item";
    [SerializeField] private Transform grabPoint;

    [Header("Gravity")]
    [SerializeField] private bool permanentlyEnableGravityWhenGrabbed;

    [Header("Interaction Prompts")]
    [SerializeField] private string grabPromptOverride;
    [SerializeField] private string heldPromptOverride;

    private Rigidbody itemRigidbody;
    private bool defaultUseGravity;
    private bool gravityPermanentlyEnabled;
    private RigidbodyInterpolation defaultInterpolation;
    private CollisionDetectionMode defaultCollisionMode;
    private int defaultSolverIterations;
    private int defaultSolverVelocityIterations;
    public string GrabPromptOverride => grabPromptOverride;
    public string HeldPromptOverride => heldPromptOverride;

    private IngredientItem ingredientItem;
    private PotionItem potionItem;

    public string DisplayName
    {
        get
        {
            if (ingredientItem != null && ingredientItem.Data != null && !string.IsNullOrWhiteSpace(ingredientItem.Data.IngredientName))
            {
                return ingredientItem.Data.IngredientName;
            }

            if (potionItem != null && potionItem.Data != null && !string.IsNullOrWhiteSpace(potionItem.Data.PotionName))
            {
                return potionItem.Data.PotionName;
            }

            return displayName;
        }
    }
    public Rigidbody Rigidbody => itemRigidbody;
    public Transform GrabPoint => grabPoint != null ? grabPoint : transform;
    public bool IsHeld { get; private set; }

    private void Awake()
    {
        itemRigidbody = GetComponent<Rigidbody>();
        ingredientItem = GetComponent<IngredientItem>();
        potionItem = GetComponent<PotionItem>();
        defaultUseGravity = itemRigidbody.useGravity;
        defaultInterpolation = itemRigidbody.interpolation;
        defaultCollisionMode = itemRigidbody.collisionDetectionMode;
        defaultSolverIterations = itemRigidbody.solverIterations;
        defaultSolverVelocityIterations = itemRigidbody.solverVelocityIterations;
    }

    public bool CanGrab()
    {
        return !IsHeld && isActiveAndEnabled;
    }

    public void OnGrabbed()
    {
        IsHeld = true;

        if (permanentlyEnableGravityWhenGrabbed)
            gravityPermanentlyEnabled = true;

        itemRigidbody.useGravity = true;
        itemRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        itemRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        itemRigidbody.WakeUp();
    }

    public void OnReleased()
    {
        IsHeld = false;
        itemRigidbody.useGravity = gravityPermanentlyEnabled ? true : defaultUseGravity;
        itemRigidbody.interpolation = defaultInterpolation;
        itemRigidbody.collisionDetectionMode = defaultCollisionMode;
        itemRigidbody.solverIterations = defaultSolverIterations;
        itemRigidbody.solverVelocityIterations = defaultSolverVelocityIterations;
        itemRigidbody.WakeUp();
    }
}