using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class LiquidWobble : MonoBehaviour
{
    [Header("Renderer")]
    public Renderer targetRenderer;

    [Header("Wobble")]
    public float maxWobble = 0.03f;
    public float wobbleFrequency = 2.5f;
    public float recoveryTime = 2.0f;

    [Header("Influence")]
    public float movementInfluence = 0.15f;
    public float rotationInfluence = 0.2f;

    private Material material;

    private Vector3 lastPosition;
    private Quaternion lastRotation;

    public float impulseX;
    public float impulseZ;

    private float time;

    void Start()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        material = targetRenderer.material;

        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        time += dt;

        // World velocity
        Vector3 worldVelocity = (transform.position - lastPosition) / dt;

        // Convert to local space so wobble always matches bottle orientation
        Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity);

        // Angular velocity
        Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(lastRotation);
        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle > 180f)
            angle -= 360f;

        Vector3 angularVelocity = axis * angle * Mathf.Deg2Rad / dt;

        // Store transforms
        lastPosition = transform.position;
        lastRotation = transform.rotation;

        // Add impulses
        impulseX += (localVelocity.x * movementInfluence
                    - angularVelocity.z * rotationInfluence) * dt;

        impulseZ += (-localVelocity.z * movementInfluence
                    + angularVelocity.x * rotationInfluence) * dt;

        // Clamp maximum wobble
        impulseX = Mathf.Clamp(impulseX, -maxWobble, maxWobble);
        impulseZ = Mathf.Clamp(impulseZ, -maxWobble, maxWobble);

        // Exponential decay
        float decay = Mathf.Exp(-dt / recoveryTime);
        impulseX *= decay;
        impulseZ *= decay;

        // Oscillation
        float wave = Mathf.Sin(time * wobbleFrequency * Mathf.PI * 2f);

        float xWobble = impulseZ * wave;
        float zWobble = impulseX * wave;

        material.SetFloat("_XWobble", xWobble);
        material.SetFloat("_ZWobble", zWobble);
    }
}