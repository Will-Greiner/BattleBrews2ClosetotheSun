using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    private readonly HashSet<object> uiOwners = new();

    private void Awake()
    {
        Instance = this;
        ApplyState();
    }

    public void ShowForUI(object owner)
    {
        if (owner == null)
            return;

        uiOwners.Add(owner);
        ApplyState();
    }

    public void HideForUI(object owner)
    {
        if (owner == null)
            return;

        uiOwners.Remove(owner);
        ApplyState();
    }

    private void ApplyState()
    {
        bool interactingWithUI = uiOwners.Count > 0;
        Cursor.visible = interactingWithUI;
        Cursor.lockState = interactingWithUI ? CursorLockMode.None : CursorLockMode.Confined;
    }
}