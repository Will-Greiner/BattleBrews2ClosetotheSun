using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class IngredientContributionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text ingredientNameText;
    [SerializeField] private TMP_Text selectedOptionText;
    [SerializeField] private Camera playerCamera;

    [Header("Position")]
    [SerializeField] private Vector3 handOffset = new Vector3(0f, 0.35f, 0f);
    [SerializeField] private bool faceCamera = true;

    private readonly List<CauldronContribution> options = new();

    private Transform followTarget;
    private Action<CauldronContribution> confirmAction;
    private Action cancelAction;
    private int selectedIndex;

    public bool IsOpen => panel != null && panel.activeSelf;

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void Update()
    {
        if (!IsOpen)
            return;

        UpdatePosition();
        ReadSelectionInput();
    }

    public void Show(IngredientData ingredient, Transform handTransform, Action<CauldronContribution> onConfirm, Action onCancel)
    {
        if (ingredient == null)
            return;

        BuildOptions(ingredient);

        if (options.Count == 0)
            return;

        followTarget = handTransform;
        confirmAction = onConfirm;
        cancelAction = onCancel;
        selectedIndex = 0;

        if (ingredientNameText != null)
            ingredientNameText.text = ingredient.IngredientName;

        UpdateSelectedOption();

        if (panel != null)
            panel.SetActive(true);

        UpdatePosition();
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);

        options.Clear();
        followTarget = null;
        confirmAction = null;
        cancelAction = null;
        selectedIndex = 0;
    }

    private void BuildOptions(IngredientData ingredient)
    {
        options.Clear();
        options.Add(new CauldronContribution(ingredient));

        foreach (ItemPropertyData property in ingredient.Properties)
        {
            if (property != null)
                options.Add(new CauldronContribution(ingredient, property));
        }
    }

    private void ReadSelectionInput()
    {
        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;

            if (scroll > 0.01f)
                ChangeSelection(-1);
            else if (scroll < -0.01f)
                ChangeSelection(1);

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                ConfirmSelection();
                return;
            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                CancelSelection();
                return;
            }
        }

        if (Gamepad.current != null)
        {
            if (Gamepad.current.leftShoulder.wasPressedThisFrame)
                ChangeSelection(-1);

            if (Gamepad.current.rightShoulder.wasPressedThisFrame)
                ChangeSelection(1);

            if (Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                ConfirmSelection();
                return;
            }

            if (Gamepad.current.buttonEast.wasPressedThisFrame)
                CancelSelection();
        }
    }

    private void ChangeSelection(int direction)
    {
        if (options.Count == 0)
            return;

        selectedIndex = (selectedIndex + direction + options.Count) % options.Count;
        UpdateSelectedOption();
    }

    private void UpdateSelectedOption()
    {
        if (selectedOptionText == null || options.Count == 0)
            return;

        selectedOptionText.text = $"<  {options[selectedIndex].DisplayName}  >";
    }

    private void ConfirmSelection()
    {
        if (options.Count == 0)
            return;

        CauldronContribution selectedContribution = options[selectedIndex];
        Action<CauldronContribution> callback = confirmAction;

        Hide();
        callback?.Invoke(selectedContribution);
    }

    private void CancelSelection()
    {
        Action callback = cancelAction;

        Hide();
        callback?.Invoke();
    }

    private void UpdatePosition()
    {
        if (panel == null || followTarget == null)
            return;

        panel.transform.position = followTarget.position + handOffset;

        if (faceCamera && playerCamera != null)
            panel.transform.rotation = Quaternion.LookRotation(panel.transform.position - playerCamera.transform.position, playerCamera.transform.up);
    }
}