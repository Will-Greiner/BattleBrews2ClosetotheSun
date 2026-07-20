using System.Collections;
using UnityEngine;

public class RoundPresentationController : MonoBehaviour
{
    [Header("Fighter")]
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private FighterAnimationController fighterAnimationController;

    [Header("UI")]
    [SerializeField] private PotionRequestUI potionRequestUI;
    [SerializeField] private RoundReportUI roundReportUI;

    [Header("Player Input")]
    [SerializeField] private GrabController grabController;

    [Header("Outcome Effects")]
    [SerializeField] private ParticleSystem winParticles;
    [SerializeField] private ParticleSystem loseParticles;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip winAudio;
    [SerializeField] private AudioClip loseAudio;

    [Header("Timing")]
    [Min(0f)]
    [SerializeField] private float reportDelay = 1.5f;

    private Coroutine presentationRoutine;
    private bool isSubscribed;

    private void Awake()
    {
        if (potionRequestUI != null)
            potionRequestUI.Hide();

        if (roundReportUI != null)
            roundReportUI.Hide();

        if (grabController != null)
            grabController.SetInputEnabled(false);
    }

    private void OnEnable()
    {
        SubscribeToGameManager();
    }

    private void Start()
    {
        SubscribeToGameManager();

        if (GameManager.Instance != null && GameManager.Instance.State == GameState.RoundStarting && GameManager.Instance.CurrentEncounter != null)
            BeginRoundStart(GameManager.Instance.CurrentEncounter, GameManager.Instance.RequestedPotion);
    }

    private void OnDisable()
    {
        UnsubscribeFromGameManager();

        if (presentationRoutine != null)
        {
            StopCoroutine(presentationRoutine);
            presentationRoutine = null;
        }
    }

    private void SubscribeToGameManager()
    {
        if (isSubscribed || GameManager.Instance == null)
            return;

        GameManager.Instance.RoundStarted += HandleRoundStarted;
        GameManager.Instance.RoundResolved += HandleRoundResolved;
        GameManager.Instance.GameEnded += HandleGameEnded;
        isSubscribed = true;
    }

    private void UnsubscribeFromGameManager()
    {
        if (!isSubscribed || GameManager.Instance == null)
            return;

        GameManager.Instance.RoundStarted -= HandleRoundStarted;
        GameManager.Instance.RoundResolved -= HandleRoundResolved;
        GameManager.Instance.GameEnded -= HandleGameEnded;
        isSubscribed = false;
    }

    private void HandleRoundStarted(EncounterData encounter, PotionData requestedPotion)
    {
        BeginRoundStart(encounter, requestedPotion);
    }

    private void HandleRoundResolved(BattleOutcome outcome, EncounterData encounter, PotionData requestedPotion, PotionData deliveredPotion)
    {
        BeginRoundResolution(outcome, encounter, requestedPotion, deliveredPotion);
    }

    private void HandleGameEnded()
    {
        if (presentationRoutine != null)
        {
            StopCoroutine(presentationRoutine);
            presentationRoutine = null;
        }

        if (potionRequestUI != null)
            potionRequestUI.Hide();

        if (roundReportUI != null)
            roundReportUI.Hide();

        if (fighterAnimationController != null)
            fighterAnimationController.HideFighter();

        if (characterManager != null)
            characterManager.ClearCharacter();

        if (grabController != null)
            grabController.SetInputEnabled(false);
    }

    private void BeginRoundStart(EncounterData encounter, PotionData requestedPotion)
    {
        if (presentationRoutine != null)
            StopCoroutine(presentationRoutine);

        presentationRoutine = StartCoroutine(RoundStartRoutine(encounter, requestedPotion));
    }

    private void BeginRoundResolution(BattleOutcome outcome, EncounterData encounter, PotionData requestedPotion, PotionData deliveredPotion)
    {
        if (presentationRoutine != null)
            StopCoroutine(presentationRoutine);

        presentationRoutine = StartCoroutine(RoundResolutionRoutine(outcome, encounter, requestedPotion, deliveredPotion));
    }

    private IEnumerator RoundStartRoutine(EncounterData encounter, PotionData requestedPotion)
    {
        if (grabController != null)
            grabController.SetInputEnabled(false);

        if (roundReportUI != null)
            roundReportUI.Hide();

        if (potionRequestUI != null)
            potionRequestUI.Hide();

        if (characterManager != null)
            characterManager.GenerateCharacter();

        if (fighterAnimationController != null)
            yield return fighterAnimationController.WalkIn();

        if (potionRequestUI != null)
            potionRequestUI.ShowRequest(encounter, requestedPotion);

        if (GameManager.Instance != null)
            GameManager.Instance.ActivateCurrentRound();

        if (grabController != null)
            grabController.SetInputEnabled(true);

        presentationRoutine = null;
    }

    private IEnumerator RoundResolutionRoutine(BattleOutcome outcome, EncounterData encounter, PotionData requestedPotion, PotionData deliveredPotion)
    {
        if (grabController != null)
            grabController.SetInputEnabled(false);

        if (potionRequestUI != null)
            potionRequestUI.Hide();

        if (fighterAnimationController != null)
            yield return fighterAnimationController.WalkOut();

        if (characterManager != null)
            characterManager.ClearCharacter();

        PlayOutcomeEffects(outcome);

        if (reportDelay > 0f)
            yield return new WaitForSeconds(reportDelay);

        if (roundReportUI != null)
            roundReportUI.ShowReport(outcome, encounter, requestedPotion, deliveredPotion);

        presentationRoutine = null;
    }

    private void PlayOutcomeEffects(BattleOutcome outcome)
    {
        bool didWin = outcome == BattleOutcome.Win;
        ParticleSystem particles = didWin ? winParticles : loseParticles;
        AudioClip clip = didWin ? winAudio : loseAudio;

        if (particles != null)
            particles.Play();

        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}