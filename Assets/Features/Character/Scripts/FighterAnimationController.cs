using System.Collections;
using UnityEngine;

public class FighterAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform fighterRoot;
    [SerializeField] private Transform entrancePoint;
    [SerializeField] private Transform stagePoint;
    [SerializeField] private Transform exitPoint;

    [Header("Animation")]
    [Tooltip("Animator controlling the fighter. When empty, one is found beneath Fighter Root at runtime.")]
    [SerializeField] private Animator fighterAnimator;

    [Tooltip("Animator Bool that transitions between idle and walking.")]
    [SerializeField] private string walkingParameter = "IsWalking";

    [Tooltip("Optional Animator Float driven by the fighter's world-space movement speed. Leave blank if unused.")]
    [SerializeField] private string movementSpeedParameter = "MovementSpeed";

    [Min(0.01f)]
    [SerializeField] private float referenceWalkSpeed = 1f;

    [Header("Movement")]
    [Min(0f)]
    [SerializeField] private float walkInDuration = 1.5f;

    [Min(0f)]
    [SerializeField] private float walkOutDuration = 1.5f;

    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public bool IsMoving { get; private set; }
    public bool IsOnStage { get; private set; }

    private int walkingParameterHash;
    private int movementSpeedParameterHash;
    private bool hasWalkingParameter;
    private bool hasMovementSpeedParameter;

    private void Awake()
    {
        CacheAnimator();
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

        SetWalking(true);

        yield return MoveFighter(stagePoint, walkInDuration);

        SetWalking(false);
        IsMoving = false;
        IsOnStage = true;
    }

    public IEnumerator WalkOut()
    {
        if (fighterRoot == null || exitPoint == null)
            yield break;

        IsMoving = true;
        IsOnStage = false;

        SetWalking(true);

        yield return MoveFighter(exitPoint, walkOutDuration);

        SetWalking(false);
        SetFighterVisible(false);
        IsMoving = false;
    }

    public void PlaceAtEntrance()
    {
        if (fighterRoot == null || entrancePoint == null)
            return;

        fighterRoot.SetPositionAndRotation(entrancePoint.position, entrancePoint.rotation);
        SetWalking(false);
        IsMoving = false;
        IsOnStage = false;
    }

    public void PlaceOnStage()
    {
        if (fighterRoot == null || stagePoint == null)
            return;

        fighterRoot.SetPositionAndRotation(stagePoint.position, stagePoint.rotation);
        SetFighterVisible(true);
        SetWalking(false);
        IsMoving = false;
        IsOnStage = true;
    }

    public void HideFighter()
    {
        SetWalking(false);
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
            SetMovementSpeed(0f);
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

            float curveSpeed = Mathf.Abs(movementCurve.Evaluate(Mathf.Clamp01(normalizedTime + 0.01f)) - curvedTime) / 0.01f;
            float averageSpeed = Vector3.Distance(startingPosition, destination.position) / duration;
            SetMovementSpeed(averageSpeed * curveSpeed);

            yield return null;
        }

        fighterRoot.SetPositionAndRotation(destination.position, destination.rotation);
        SetMovementSpeed(0f);
    }

    private void CacheAnimator()
    {
        if (fighterAnimator == null && fighterRoot != null)
            fighterAnimator = fighterRoot.GetComponentInChildren<Animator>(true);

        hasWalkingParameter = TryCacheParameter(walkingParameter, AnimatorControllerParameterType.Bool, out walkingParameterHash);
        hasMovementSpeedParameter = TryCacheParameter(movementSpeedParameter, AnimatorControllerParameterType.Float, out movementSpeedParameterHash);
    }

    private bool TryCacheParameter(string parameterName, AnimatorControllerParameterType expectedType, out int parameterHash)
    {
        parameterHash = 0;

        if (fighterAnimator == null || string.IsNullOrWhiteSpace(parameterName))
            return false;

        parameterHash = Animator.StringToHash(parameterName);

        foreach (AnimatorControllerParameter parameter in fighterAnimator.parameters)
        {
            if (parameter.nameHash == parameterHash && parameter.type == expectedType)
                return true;
        }

        Debug.LogWarning($"{name}: Animator does not contain a {expectedType} parameter named '{parameterName}'.", this);
        return false;
    }

    private void SetWalking(bool walking)
    {
        if (hasWalkingParameter)
            fighterAnimator.SetBool(walkingParameterHash, walking);

        if (!walking)
            SetMovementSpeed(0f);
    }

    private void SetMovementSpeed(float worldSpeed)
    {
        if (hasMovementSpeedParameter)
            fighterAnimator.SetFloat(movementSpeedParameterHash, worldSpeed / referenceWalkSpeed, 0.1f, Time.deltaTime);
    }

    private void SetFighterVisible(bool visible)
    {
        if (fighterRoot != null)
            fighterRoot.gameObject.SetActive(visible);
    }
}
