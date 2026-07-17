using UnityEngine;

[RequireComponent(typeof(GrabbableItem))]
public class PotionItem : MonoBehaviour
{
    [SerializeField] private PotionData potionData;

    public PotionData Data => potionData;

    public void Initialize(PotionData data)
    {
        potionData = data;
    }
}