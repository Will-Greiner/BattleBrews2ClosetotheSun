using UnityEngine;

[CreateAssetMenu(fileName = "ItemPropertyData", menuName = "Scriptable Objects/ItemPropertyData")]
public class ItemPropertyData : ScriptableObject
{
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [TextArea] [SerializeField] private string description;

    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public string Description => description;
}
