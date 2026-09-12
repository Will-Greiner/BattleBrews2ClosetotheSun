using System.Collections.Generic;
using UnityEngine;

public class TutorialTarget : MonoBehaviour
{
    private static readonly Dictionary<string, TutorialTarget> Targets = new();

    [SerializeField] private string targetId;
    [SerializeField] private ObjectHighlight highlight;

    public string TargetId => targetId;

    private void Awake()
    {
        if (highlight == null)
            highlight = GetComponent<ObjectHighlight>();
    }

    private void OnEnable()
    {
        if (!string.IsNullOrWhiteSpace(targetId))
            Targets[targetId] = this;
    }

    private void OnDisable()
    {
        if (!string.IsNullOrWhiteSpace(targetId) && Targets.TryGetValue(targetId, out TutorialTarget current) && current == this)
            Targets.Remove(targetId);

        Hide();
    }

    public void Show()
    {
        highlight?.Show();
    }

    public void Hide()
    {
        highlight?.Hide();
    }

    public static TutorialTarget Find(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        Targets.TryGetValue(id, out TutorialTarget target);
        return target;
    }

    public void SetTargetId(string value)
    {
        if (!string.IsNullOrWhiteSpace(targetId) && Targets.TryGetValue(targetId, out TutorialTarget current) && current == this)
            Targets.Remove(targetId);

        targetId = value;

        if (isActiveAndEnabled && !string.IsNullOrWhiteSpace(targetId))
            Targets[targetId] = this;
    }
}
