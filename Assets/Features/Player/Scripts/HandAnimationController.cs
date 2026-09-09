using System.Collections.Generic;
using UnityEngine;

public enum HandEmote
{
    Wave = 0,
    ThumbsUp = 1,
    Point = 2,
    Celebrate = 3,
    Disgust = 4,
    Angry = 5
}

public class HandAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Animator Parameters")]
    [SerializeField] private string idleParameter = "IsIdle";
    [SerializeField] private string hoveringParameter = "IsHovering";
    [SerializeField] private string holdingParameter = "IsHolding";
    [SerializeField] private string emoteIndexParameter = "EmoteIndex";
    [SerializeField] private string playEmoteTrigger = "PlayEmote";

    private readonly HashSet<int> availableParameters = new();
    private bool isHolding;
    private bool isHovering;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        CacheParameters();
        ApplyInteractionState();
    }

    public void SetInteractionState(bool holding, bool hovering)
    {
        if (isHolding == holding && isHovering == hovering)
            return;

        isHolding = holding;
        isHovering = hovering;
        ApplyInteractionState();
    }

    public void PlayEmote(HandEmote emote)
    {
        if (animator == null)
            return;

        SetInteger(emoteIndexParameter, (int)emote);
        SetTrigger(playEmoteTrigger);
    }

    public void PlayWave() => PlayEmote(HandEmote.Wave);
    public void PlayThumbsUp() => PlayEmote(HandEmote.ThumbsUp);
    public void PlayPoint() => PlayEmote(HandEmote.Point);
    public void PlayCelebrate() => PlayEmote(HandEmote.Celebrate);
    public void PlayDisgust() => PlayEmote(HandEmote.Disgust);
    public void PlayAngry() => PlayEmote(HandEmote.Angry);

    private void ApplyInteractionState()
    {
        SetBool(idleParameter, !isHolding && !isHovering);
        SetBool(hoveringParameter, isHovering);
        SetBool(holdingParameter, isHolding);
    }

    private void CacheParameters()
    {
        availableParameters.Clear();

        if (animator == null)
            return;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
            availableParameters.Add(parameter.nameHash);
    }

    private void SetBool(string parameterName, bool value)
    {
        int parameterHash = Animator.StringToHash(parameterName);

        if (animator != null && availableParameters.Contains(parameterHash))
            animator.SetBool(parameterHash, value);
    }

    private void SetInteger(string parameterName, int value)
    {
        int parameterHash = Animator.StringToHash(parameterName);

        if (animator != null && availableParameters.Contains(parameterHash))
            animator.SetInteger(parameterHash, value);
    }

    private void SetTrigger(string parameterName)
    {
        int parameterHash = Animator.StringToHash(parameterName);

        if (animator != null && availableParameters.Contains(parameterHash))
            animator.SetTrigger(parameterHash);
    }
}
