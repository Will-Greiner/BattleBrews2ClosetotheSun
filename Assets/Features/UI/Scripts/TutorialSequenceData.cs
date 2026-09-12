using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TutorialStep
{
    [TextArea] [SerializeField] private string instruction;
    [SerializeField] private TutorialTrigger completionTrigger;
    [Tooltip("Optional. The reported action ID must match this value when it is not empty.")]
    [SerializeField] private string requiredId;
    [Tooltip("Optional ID of a TutorialTarget to highlight while this step is active.")]
    [SerializeField] private string highlightTargetId;

    public string Instruction => instruction;
    public TutorialTrigger CompletionTrigger => completionTrigger;
    public string RequiredId => requiredId;
    public string HighlightTargetId => highlightTargetId;

    public TutorialStep(string instruction, TutorialTrigger completionTrigger, string requiredId, string highlightTargetId)
    {
        this.instruction = instruction;
        this.completionTrigger = completionTrigger;
        this.requiredId = requiredId;
        this.highlightTargetId = highlightTargetId;
    }
}

[CreateAssetMenu(fileName = "TutorialSequence", menuName = "Battle Brews/Tutorial Sequence")]
public class TutorialSequenceData : ScriptableObject
{
    [SerializeField] private List<TutorialStep> steps = new();

    public IReadOnlyList<TutorialStep> Steps => steps;
}
