using System.Collections;
using UnityEngine;

public class FighterAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform fighterRoot;
    [SerializeField] private Transform entrancePoint;
    [SerializeField] private Transform stagePoint;
    [SerializeField] private Transform exitPoint;

    [Header("Movement")]
    [Min(0f)]
    [SerializeField] private float walkInDuration = 1.5f;

    [Min(0f)]
    [SerializeField] private float walkOutDuration = 1.5f;

    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public bool IsMoving { get; private set; }
    public bool IsOnStage { get; private set; }

    private void Awake()
    {
        PlaceAtEntrance();
        SetFighterVisible(false);
    }

    public IEnumerator WalkIn()
    {
        if (fighterRoot == null || entrancePoint == null || stagePoint == null)
            yield break;

        IsMoving = true;
        IsOnStage = false;

        fighterRoot.SetPositionAndRotation(entrancePoint.position, entrancePoint.rotation);
        SetFighterVisible(true);

        yield return MoveFighter(stagePoint, walkInDuration);

        IsMoving = false;
        IsOnStage = true;
    }

    public IEnumerator WalkOut()
    {
        if (fighterRoot == null || exitPoint == null)
            yield break;

        IsMoving = true;
        IsOnStage = false;

        yield return MoveFighter(exitPoint, walkOutDuration);

        SetFighterVisible(false);
        IsMoving = false;
    }

    public void PlaceAtEntrance()
    {
        if (fighterRoot == null || entrancePoint == null)
            return;

        fighterRoot.SetPositionAndRotation(entrancePoint.position, entrancePoint.rotation);
        IsMoving = false;
        IsOnStage = false;
    }

    public void PlaceOnStage()
    {
        if (fighterRoot == null || stagePoint == null)
            return;

        fighterRoot.SetPositionAndRotation(stagePoint.position, stagePoint.rotation);
        SetFighterVisible(true);
        IsMoving = false;
        IsOnStage = true;
    }

    public void HideFighter()
    {
        SetFighterVisible(false);
        IsMoving = false;
        IsOnStage = false;
    }

    private IEnumerator MoveFighter(Transform destination, float duration)
    {
        Vector3 startingPosition = fighterRoot.position;
        Quaternion startingRotation = fighterRoot.rotation;

        if (duration <= 0f)
        {
            fighterRoot.SetPositionAndRotation(destination.position, destination.rotation);
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / duration);
            float curvedTime = movementCurve.Evaluate(normalizedTime);

            fighterRoot.position = Vector3.LerpUnclamped(startingPosition, destination.position, curvedTime);
            fighterRoot.rotation = Quaternion.SlerpUnclamped(startingRotation, destination.rotation, curvedTime);

            yield return null;
        }

        fighterRoot.SetPositionAndRotation(destination.position, destination.rotation);
    }

    private void SetFighterVisible(bool visible)
    {
        if (fighterRoot != null)
            fighterRoot.gameObject.SetActive(visible);
    }
}
