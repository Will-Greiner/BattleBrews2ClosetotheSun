using UnityEngine;

public class HeadTrackCursor : MonoBehaviour
{
    [SerializeField] Transform targetTransform;
    [SerializeField] Vector3 RotationOffset;
    void Update()
    {
        // Calculate direction from head to hand
        Vector3 direction = targetTransform.position - transform.position;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation * Quaternion.Euler(RotationOffset);
        }
    }
}
