using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PotionRequestUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text encounterNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image potionIcon;
    [SerializeField] private RoundObjectiveHUD roundObjectiveHUD;

    [Header("Typewriter")]
    [SerializeField, Min(0.001f)] private float secondsPerCharacter = 0.025f;
    [SerializeField, Min(0f)] private float punctuationPause = 0.12f;
    [SerializeField] private AudioSource typingAudioSource;
    [SerializeField] private AudioClip[] typingSounds;

    [Header("Continue")]
    [SerializeField] private Button continueButton;

    private bool continueRequested;
    private string currentDialogue;

    private Coroutine typingRoutine;

    public bool IsTyping { get; private set; }
    public bool IsComplete { get; private set; }

    private void Awake()
    {
        if (continueButton != null)
        continueButton.onClick.AddListener(HandleContinuePressed);

        Hide();
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(HandleContinuePressed);
    }

    public void ShowRequest(EncounterData encounter, PotionData requestedPotion)
    {
        if (encounter == null || requestedPotion == null)
        {
            Hide();
            return;
        }

        continueRequested = false;
        currentDialogue = encounter.BuildRequestDialogue(requestedPotion);

        if (continueButton != null)
            continueButton.interactable = true;

        typingRoutine = StartCoroutine(TypeDialogue(currentDialogue));

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
            }

    public void Hide()
    {
        if (encounterNameText != null)
            encounterNameText.text = string.Empty;

        if (dialogueText != null)
            dialogueText.text = string.Empty;

        if (potionIcon != null)
        {
            potionIcon.sprite = null;
            potionIcon.enabled = false;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        IsTyping = false;
        IsComplete = false;

        if (dialogueText != null)
        {
            dialogueText.text = string.Empty;
            dialogueText.maxVisibleCharacters = int.MaxValue;
        }

        if (roundObjectiveHUD != null)
            roundObjectiveHUD.Hide();
    }

    private IEnumerator TypeDialogue(string dialogue)
    {
        IsTyping = true;
        IsComplete = false;

        dialogueText.text = dialogue;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();

        int totalCharacters = dialogueText.textInfo.characterCount;

        for (int i = 0; i < totalCharacters; i++)
        {
            dialogueText.maxVisibleCharacters = i + 1;

            char character = dialogueText.textInfo.characterInfo[i].character;

            if (!char.IsWhiteSpace(character))
                PlayTypingSound();

            float delay = IsPunctuation(character)
                ? secondsPerCharacter + punctuationPause
                : secondsPerCharacter;

            yield return new WaitForSecondsRealtime(delay);
        }

        IsTyping = false;
        IsComplete = true;
        typingRoutine = null;
    }

    private bool IsPunctuation(char character)
    {
        return character == '.' ||
            character == ',' ||
            character == '!' ||
            character == '?' ||
            character == ':' ||
            character == ';';
    }

    private void PlayTypingSound()
    {
        if (typingAudioSource == null ||
            typingSounds == null ||
            typingSounds.Length == 0)
            return;

        AudioClip clip = typingSounds[Random.Range(0, typingSounds.Length)];

        if (clip != null)
            typingAudioSource.PlayOneShot(clip);
    }
    private void HandleContinuePressed()
    {
        if (IsTyping)
        {
            FinishTypingImmediately();
            return;
        }

        continueRequested = true;

        if (continueButton != null)
            continueButton.interactable = false;
    }

    private void FinishTypingImmediately()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        dialogueText.text = currentDialogue;
        dialogueText.maxVisibleCharacters = int.MaxValue;

        IsTyping = false;
        IsComplete = true;
    }

    public IEnumerator WaitForContinue()
    {
        yield return new WaitUntil(() => continueRequested);
    }
}