using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RunePointUI : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
    [Header("References")]
    [SerializeField] private Image runeImage;
    [SerializeField] private TMP_Text orderText;

    [Header("Colors")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color hoverColor = Color.cyan;
    [SerializeField] private Color connectedColor = Color.green;

    [Header("Hover")]
    [Min(1f)] [SerializeField] private float hoverScale = 1.2f;
    [Min(0f)] [SerializeField] private float scaleSpeed = 12f;

    private RuneConstellationMinigame minigame;
    private Vector3 baseScale;
    private Vector3 targetScale;
    private bool isConnected;
    private bool isHovered;

    public RectTransform RectTransform { get; private set; }
    public Vector3 WorldPosition => RectTransform.position;

    private void Awake()
    {
        RectTransform = transform as RectTransform;
        baseScale = transform.localScale;
        targetScale = baseScale;
    }

    private void Update()
    {
        float interpolation = 1f - Mathf.Exp(-scaleSpeed * Time.unscaledDeltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, interpolation);
    }

    public void Initialize(RuneConstellationMinigame owner, int displayOrder)
    {
        minigame = owner;

        if (orderText != null)
            orderText.text = (displayOrder + 1).ToString();

        SetConnected(false);
        SetHovered(false);
    }

    public void SetConnected(bool connected)
    {
        isConnected = connected;
        RefreshVisuals();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        minigame?.BeginDrag(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHovered(true);
        minigame?.EnterPoint(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHovered(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            minigame?.EndDrag();
    }

    private void SetHovered(bool hovered)
    {
        isHovered = hovered;
        targetScale = baseScale * (isHovered ? hoverScale : 1f);
        RefreshVisuals();
    }

    private void RefreshVisuals()
    {
        if (runeImage == null)
            return;

        if (isConnected)
            runeImage.color = connectedColor;
        else if (isHovered)
            runeImage.color = hoverColor;
        else
            runeImage.color = defaultColor;
    }
}