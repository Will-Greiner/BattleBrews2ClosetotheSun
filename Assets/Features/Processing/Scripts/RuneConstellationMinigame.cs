using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RuneConstellationMinigame : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform pointArea;
    [SerializeField] private List<RunePointUI> runePoints = new();
    [SerializeField] private LineRenderer connectionLine;
    [SerializeField] private Canvas constellationCanvas;

    [Header("Sequence")]
    [SerializeField] private bool randomizeSequence = true;

    [Header("Point Placement")]
    [SerializeField] private bool randomizePositions = true;
    [Min(0f)] [SerializeField] private float edgePadding = 35f;
    [Min(0f)] [SerializeField] private float minimumPointDistance = 80f;
    [Min(1)] [SerializeField] private int placementAttemptsPerPoint = 30;

    [Header("Difficulty")]
    [Min(2)] [SerializeField] private int levelOnePointCount = 3;
    [Min(2)] [SerializeField] private int levelTwoPointCount = 5;
    [Min(2)] [SerializeField] private int levelThreePointCount = 7;

    private readonly List<RunePointUI> sequence = new();
    private readonly List<RunePointUI> connectedPoints = new();
    private readonly List<Vector2> usedPositions = new();
    private readonly List<RunePointUI> activePoints = new();

    private Action completedCallback;
    private int nextPointIndex;
    private bool isDragging;
    private bool isActive;

    private void Awake()
    {
        HideImmediate();
    }

    private void Update()
    {
        if (!isActive || !isDragging)
            return;

        DetectRuneUnderCursor();
        RefreshLine();
    }

    public void Begin(int propertyLevel, Action onCompleted)
    {
        completedCallback = onCompleted;
        SetVisible(true);
        Canvas.ForceUpdateCanvases();

        PrepareActivePoints(propertyLevel);

        if (randomizePositions)
            RandomizePointPositions();

        BuildSequence();
        ResetAttempt();
        isActive = true;

        CursorManager.Instance?.ShowForUI(this);
    }

    public void BeginDrag(RunePointUI point)
    {
        if (!isActive || point == null)
            return;

        ResetAttempt();

        if (sequence.Count == 0 || point != sequence[0])
            return;

        isDragging = true;
        ConnectPoint(point);
    }

    public void EnterPoint(RunePointUI point)
    {
        if (!isActive || !isDragging || point == null || nextPointIndex >= sequence.Count)
            return;

        if (point == sequence[nextPointIndex])
            ConnectPoint(point);
    }

    public void EndDrag()
    {
        if (!isActive || !isDragging)
            return;

        isDragging = false;

        if (nextPointIndex < sequence.Count)
            ResetAttempt();
    }

    public void Cancel()
    {
        completedCallback = null;
        isActive = false;
        isDragging = false;
        ResetAttempt();
        SetVisible(false);

        CursorManager.Instance?.HideForUI(this);
    }

    private void ConnectPoint(RunePointUI point)
    {
        connectedPoints.Add(point);
        point.SetConnected(true);
        nextPointIndex++;
        RefreshLine();

        if (nextPointIndex >= sequence.Count)
            Complete();
    }

    private void Complete()
    {
        isDragging = false;
        isActive = false;

        Action callback = completedCallback;
        completedCallback = null;

        SetVisible(false);
        callback?.Invoke();

        CursorManager.Instance?.HideForUI(this);
    }

    private void PrepareActivePoints(int propertyLevel)
    {
        activePoints.Clear();

        List<RunePointUI> availablePoints = new();

        foreach (RunePointUI point in runePoints)
        {
            if (point == null)
                continue;

            point.gameObject.SetActive(false);
            availablePoints.Add(point);
        }

        for (int i = availablePoints.Count - 1; i > 0; i--)
        {
            int swapIndex = UnityEngine.Random.Range(0, i + 1);
            (availablePoints[i], availablePoints[swapIndex]) = (availablePoints[swapIndex], availablePoints[i]);
        }

        int desiredPointCount = GetPointCountForLevel(propertyLevel);
        int activePointCount = Mathf.Min(desiredPointCount, availablePoints.Count);

        for (int i = 0; i < activePointCount; i++)
        {
            RunePointUI point = availablePoints[i];
            point.gameObject.SetActive(true);
            activePoints.Add(point);
        }
    }

    private int GetPointCountForLevel(int propertyLevel)
    {
        switch (propertyLevel)
        {
            case 1:
                return levelOnePointCount;

            case 2:
                return levelTwoPointCount;

            case 3:
                return levelThreePointCount;

            default:
                return levelOnePointCount;
        }
    }

    private void BuildSequence()
    {
        sequence.Clear();

        foreach (RunePointUI point in activePoints)
        {
            if (point != null)
                sequence.Add(point);
        }

        if (randomizeSequence)
        {
            for (int i = sequence.Count - 1; i > 0; i--)
            {
                int swapIndex = UnityEngine.Random.Range(0, i + 1);
                (sequence[i], sequence[swapIndex]) = (sequence[swapIndex], sequence[i]);
            }
        }

        for (int i = 0; i < sequence.Count; i++)
            sequence[i].Initialize(this, i);
    }

    private void RandomizePointPositions()
    {
        if (pointArea == null)
            return;

        usedPositions.Clear();
        Rect areaRect = pointArea.rect;

        foreach (RunePointUI point in activePoints)
        {
            if (point == null || point.RectTransform == null)
                continue;

            Vector2 halfSize = point.RectTransform.rect.size * 0.5f;
            float minimumX = areaRect.xMin + edgePadding + halfSize.x;
            float maximumX = areaRect.xMax - edgePadding - halfSize.x;
            float minimumY = areaRect.yMin + edgePadding + halfSize.y;
            float maximumY = areaRect.yMax - edgePadding - halfSize.y;

            Vector2 bestPosition = Vector2.zero;
            float bestDistance = -1f;

            for (int attempt = 0; attempt < placementAttemptsPerPoint; attempt++)
            {
                Vector2 candidate = new Vector2(
                    UnityEngine.Random.Range(minimumX, maximumX),
                    UnityEngine.Random.Range(minimumY, maximumY));

                float nearestDistance = GetNearestPointDistance(candidate);

                if (nearestDistance > bestDistance)
                {
                    bestDistance = nearestDistance;
                    bestPosition = candidate;
                }

                if (nearestDistance >= minimumPointDistance)
                    break;
            }

            point.RectTransform.anchoredPosition = bestPosition;
            usedPositions.Add(bestPosition);
        }
    }

    private float GetNearestPointDistance(Vector2 candidate)
    {
        if (usedPositions.Count == 0)
            return float.MaxValue;

        float nearestDistance = float.MaxValue;

        foreach (Vector2 usedPosition in usedPositions)
        {
            float distance = Vector2.Distance(candidate, usedPosition);
            nearestDistance = Mathf.Min(nearestDistance, distance);
        }

        return nearestDistance;
    }

    private bool IsPositionFarEnoughFromOtherPoints(Vector2 candidate)
    {
        foreach (Vector2 usedPosition in usedPositions)
        {
            if (Vector2.Distance(candidate, usedPosition) < minimumPointDistance)
                return false;
        }

        return true;
    }

    private void ResetAttempt()
    {
        nextPointIndex = 0;
        connectedPoints.Clear();

        foreach (RunePointUI point in activePoints)
        {
            if (point != null)
                point.SetConnected(false);
        }

        RefreshLine();
    }

    private void RefreshLine()
    {
        if (connectionLine == null)
            return;

        bool showCursorSegment = isDragging && connectedPoints.Count > 0;
        int positionCount = connectedPoints.Count + (showCursorSegment ? 1 : 0);
        connectionLine.positionCount = positionCount;

        for (int i = 0; i < connectedPoints.Count; i++)
            connectionLine.SetPosition(i, connectedPoints[i].WorldPosition);

        if (showCursorSegment && TryGetCursorWorldPosition(out Vector3 cursorWorldPosition))
            connectionLine.SetPosition(positionCount - 1, cursorWorldPosition);
    }

    private bool TryGetCursorWorldPosition(out Vector3 worldPosition)
    {
        worldPosition = Vector3.zero;

        if (Mouse.current == null || pointArea == null)
            return false;

        Camera eventCamera = null;

        if (constellationCanvas != null && constellationCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            eventCamera = constellationCanvas.worldCamera;

        return RectTransformUtility.ScreenPointToWorldPointInRectangle(pointArea, Mouse.current.position.ReadValue(), eventCamera, out worldPosition);
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        if (connectionLine != null)
        {
            connectionLine.enabled = visible;

            if (!visible)
                connectionLine.positionCount = 0;
        }
    }

    private void HideImmediate()
    {
        isActive = false;
        isDragging = false;

        if (connectionLine != null)
            connectionLine.positionCount = 0;

        SetVisible(false);
    }

    private void DetectRuneUnderCursor()
    {
        if (Mouse.current == null)
            return;

        Vector2 pointerPosition = Mouse.current.position.ReadValue();
        Camera eventCamera = GetEventCamera();

        foreach (RunePointUI point in activePoints)
        {
            if (point == null || point.RectTransform == null)
                continue;

            bool containsPointer = RectTransformUtility.RectangleContainsScreenPoint(
                point.RectTransform,
                pointerPosition,
                eventCamera);

            point.SetPointerHovered(containsPointer);

            if (containsPointer)
                EnterPoint(point);
        }
    }

    private Camera GetEventCamera()
    {
        if (constellationCanvas == null ||
            constellationCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }

        return constellationCanvas.worldCamera;
    }
}