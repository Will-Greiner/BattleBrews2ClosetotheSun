using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] TMP_Text text;

    [SerializeField] Color normal = Color.white;
    [SerializeField] Color hover = Color.yellow;

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.color = hover;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.color = normal;
    }
}