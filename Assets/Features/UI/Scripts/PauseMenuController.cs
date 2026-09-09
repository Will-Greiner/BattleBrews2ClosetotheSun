using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup pauseCanvasGroup;
    [SerializeField] private GrabController grabController;
    [SerializeField] private SettingsMenuUI settingsMenu;

    [Header("Request Information")]
    [SerializeField] private TMP_Text encounterNameText;
    [SerializeField] private TMP_Text opponentText;
    [SerializeField] private TMP_Text potionText;
    [SerializeField] private TMP_Text fullDialogueText;
    [SerializeField] private Image potionIcon;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    private bool isPaused;
    private float previousTimeScale = 1f;
    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisible;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (settingsButton != null) settingsButton.onClick.AddListener(ShowSettings);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
        if (settingsMenu != null) settingsMenu.Closed += HandleSettingsClosed;
        SetVisible(false);
    }

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        if (settingsMenu != null && settingsMenu.IsOpen)
        {
            settingsMenu.Hide();
            return;
        }

        if (isPaused)
            Resume();
        else if (CanPause())
            Pause();
    }

    private void OnDisable()
    {
        if (isPaused)
            Resume();
    }

    private void OnDestroy()
    {
        if (resumeButton != null) resumeButton.onClick.RemoveListener(Resume);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(ShowSettings);
        if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
        if (quitButton != null) quitButton.onClick.RemoveListener(QuitGame);
        if (settingsMenu != null) settingsMenu.Closed -= HandleSettingsClosed;
    }

    public void Pause()
    {
        if (isPaused || !CanPause())
            return;

        isPaused = true;
        previousTimeScale = Time.timeScale;
        previousCursorLockMode = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        grabController?.AcquireInputLock(this);
        RefreshRequestInformation();
        SetVisible(true);

        if (EventSystem.current != null && resumeButton != null)
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
    }

    public void Resume()
    {
        if (!isPaused)
            return;

        settingsMenu?.Hide();
        SetVisible(false);
        Time.timeScale = previousTimeScale;
        Cursor.lockState = previousCursorLockMode;
        Cursor.visible = previousCursorVisible;
        grabController?.ReleaseInputLock(this);
        isPaused = false;
    }

    public void ShowSettings()
    {
        if (!isPaused || settingsMenu == null)
            return;

        SetVisible(false);
        settingsMenu?.Show();
    }

    public void ReturnToMainMenu()
    {
        ProgressionManager.Instance?.SaveNow();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        ProgressionManager.Instance?.SaveNow();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private bool CanPause()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.RoundActive)
            return false;

        return grabController == null || grabController.InputEnabled;
    }

    private void HandleSettingsClosed()
    {
        if (!isPaused)
            return;

        SetVisible(true);

        if (EventSystem.current != null && settingsButton != null)
            EventSystem.current.SetSelectedGameObject(settingsButton.gameObject);
    }

    private void RefreshRequestInformation()
    {
        EncounterData encounter = GameManager.Instance != null ? GameManager.Instance.CurrentEncounter : null;
        PotionData potion = GameManager.Instance != null ? GameManager.Instance.RequestedPotion : null;

        if (encounterNameText != null) encounterNameText.text = encounter != null ? encounter.EncounterName : string.Empty;
        if (opponentText != null) opponentText.text = encounter != null ? $"Opponent: {encounter.OpponentName}" : string.Empty;
        if (potionText != null) potionText.text = potion != null ? $"Brew: {potion.PotionName}" : string.Empty;
        if (fullDialogueText != null) fullDialogueText.text = encounter != null ? encounter.BuildRequestDialogue(potion) : string.Empty;

        if (potionIcon != null)
        {
            potionIcon.sprite = potion != null ? potion.Icon : null;
            potionIcon.enabled = potionIcon.sprite != null;
        }
    }

    private void SetVisible(bool visible)
    {
        if (pauseCanvasGroup == null)
            return;

        pauseCanvasGroup.alpha = visible ? 1f : 0f;
        pauseCanvasGroup.interactable = visible;
        pauseCanvasGroup.blocksRaycasts = visible;
    }
}
