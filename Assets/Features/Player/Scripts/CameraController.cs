using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private GrabController grabController;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 45f;
    [SerializeField] private float rotationLimit = 60f;
    [SerializeField] private float acceleration = 8f;

    [Header("Screen Edge")]
    [SerializeField, Range(0.01f, 0.5f)] private float edgeSize = 0.15f;
    [SerializeField] private float edgePower = 1.5f;

    private float currentYaw;
    private float currentSpeed;
    private float targetSpeed;

    private void Start()
    {
        if (grabController == null)
            grabController = FindFirstObjectByType<GrabController>();

        currentYaw = NormalizeAngle(transform.localEulerAngles.y);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (grabController != null && !grabController.InputEnabled)
        {
            targetSpeed = 0f;
            currentSpeed = 0f;
            currentYaw = NormalizeAngle(transform.localEulerAngles.y);
            return;
        }

        if (Mouse.current == null || Screen.width <= 0)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        float normalizedX = mousePosition.x / Screen.width;

        targetSpeed = CalculateTargetSpeed(normalizedX);
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, 1f - Mathf.Exp(-acceleration * Time.deltaTime));

        currentYaw += currentSpeed * Time.deltaTime;
        currentYaw = Mathf.Clamp(currentYaw, -rotationLimit, rotationLimit);

        transform.localRotation = Quaternion.Euler(0f, currentYaw, 0f);
    }

    private float CalculateTargetSpeed(float normalizedX)
    {
        if (normalizedX < edgeSize)
        {
            float amount = 1f - normalizedX / edgeSize;
            amount = Mathf.Pow(Mathf.Clamp01(amount), edgePower);
            return -rotationSpeed * amount;
        }

        if (normalizedX > 1f - edgeSize)
        {
            float amount = (normalizedX - (1f - edgeSize)) / edgeSize;
            amount = Mathf.Pow(Mathf.Clamp01(amount), edgePower);
            return rotationSpeed * amount;
        }

        return 0f;
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
}
