using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    private const string LastSelectedSlotKey = "save.lastSelectedSlot";

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GrabController grabController;
    [SerializeField] private CanvasGroup menuCanvasGroup;
    [SerializeField] private Button playButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private SaveSlotMenuUI saveSlotMenu;
    [SerializeField] private SettingsMenuUI settingsMenu;

    [Header("Camera Positions")]
    [SerializeField] private Transform menuCameraPoint;
    private Vector3 gameplayCameraPosition;
    private Quaternion gameplayCameraRotation;

    [Header("Transition")]
    [SerializeField, Min(0f)] private float transitionDuration = 2f;
    [SerializeField] private AnimationCurve transitionCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private bool isStarting;

    private void Awake()
    {
        GameSettings.LoadAndApply();

        if (playerCamera != null)
        {
            gameplayCameraPosition = playerCamera.transform.position;
            gameplayCameraRotation = playerCamera.transform.rotation;
        }

        if (playButton != null) playButton.onClick.AddListener(ShowNewGameSlots);
        if (continueButton != null) continueButton.onClick.AddListener(ContinueGame);
        if (loadButton != null) loadButton.onClick.AddListener(ShowLoadSlots);
        if (settingsButton != null) settingsButton.onClick.AddListener(ShowSettings);
        if (saveSlotMenu != null) saveSlotMenu.SlotsChanged += RefreshContinueButton;

        if (grabController != null)
            grabController.AcquireInputLock(this);

        if (playerCamera != null && menuCameraPoint != null)
        {
            playerCamera.transform.SetPositionAndRotation(
                menuCameraPoint.position,
                menuCameraPoint.rotation);
        }

        ShowMenu();
        RefreshContinueButton();
    }

    private void OnDestroy()
    {
        if (playButton != null) playButton.onClick.RemoveListener(ShowNewGameSlots);
        if (continueButton != null) continueButton.onClick.RemoveListener(ContinueGame);
        if (loadButton != null) loadButton.onClick.RemoveListener(ShowLoadSlots);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(ShowSettings);
        if (saveSlotMenu != null) saveSlotMenu.SlotsChanged -= RefreshContinueButton;

        if (grabController != null)
            grabController.ReleaseInputLock(this);
    }

    public void BeginGame()
    {
        ShowNewGameSlots();
    }

    public void ContinueGame()
    {
        int slot = PlayerPrefs.GetInt(LastSelectedSlotKey, 0);

        if (slot >= 1 && slot <= SaveManager.SlotCount && SaveManager.SlotExists(slot))
            LoadGameFromSlot(slot);
    }

    public void ShowLoadSlots()
    {
        saveSlotMenu?.Show(SaveSlotMenuMode.Load, LoadGameFromSlot);
    }

    public void ShowNewGameSlots()
    {
        saveSlotMenu?.Show(SaveSlotMenuMode.NewGame, StartNewGameInSlot);
    }

    public void ShowSettings()
    {
        settingsMenu?.Show();
    }

    private void LoadGameFromSlot(int slot)
    {
        if (isStarting)
            return;

        if (ProgressionManager.Instance != null)
            ProgressionManager.Instance.LoadSlot(slot);

        RememberSelectedSlot(slot);
        saveSlotMenu?.Hide();
        StartCoroutine(StartGameRoutine());
    }

    private void StartNewGameInSlot(int slot)
    {
        if (isStarting)
            return;

        if (ProgressionManager.Instance != null)
            ProgressionManager.Instance.CreateNewSlot(slot);

        RememberSelectedSlot(slot);
        saveSlotMenu?.Hide();
        StartCoroutine(StartGameRoutine());
    }

    private void RememberSelectedSlot(int slot)
    {
        PlayerPrefs.SetInt(LastSelectedSlotKey, slot);
        PlayerPrefs.Save();
    }

    private void RefreshContinueButton()
    {
        int slot = PlayerPrefs.GetInt(LastSelectedSlotKey, 0);
        if (continueButton != null) continueButton.interactable = slot >= 1 && slot <= SaveManager.SlotCount && SaveManager.SlotExists(slot);
    }

    private IEnumerator StartGameRoutine()
    {
        isStarting = true;

        HideMenu();

        if (playButton != null)
            playButton.interactable = false;

        if (playerCamera == null)
        {
            FinishTransition();
            yield break;
        }

        Transform cameraTransform = playerCamera.transform;

        Vector3 startingPosition = cameraTransform.position;
        Quaternion startingRotation = cameraTransform.rotation;

        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float normalizedTime = transitionDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsedTime / transitionDuration);

            float curvedTime = transitionCurve.Evaluate(normalizedTime);

            cameraTransform.position = Vector3.LerpUnclamped(
                startingPosition,
                gameplayCameraPosition,
                               curvedTime);

            cameraTransform.rotation = Quaternion.SlerpUnclamped(
                startingRotation,
                gameplayCameraRotation,
                curvedTime);

            yield return null;
        }

        cameraTransform.SetPositionAndRotation(
            gameplayCameraPosition,
            gameplayCameraRotation);

        FinishTransition();
    }

    private void FinishTransition()
    {

        // StartGame raises RoundStarted. RoundPresentationController then
        // generates the fighter, walks them in, and begins the dialogue.
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
        else
            Debug.LogError(
                $"{name}: Cannot start because no GameManager exists.",
                this);

        // Remove only the menu's lock. RoundPresentationController maintains
        // its own lock during the fighter entrance and dialogue.
        if (grabController != null)
            grabController.ReleaseInputLock(this);

        isStarting = false;
    }

    private void ShowMenu()
    {
        if (menuCanvasGroup == null)
            return;

        menuCanvasGroup.alpha = 1f;
        menuCanvasGroup.interactable = true;
        menuCanvasGroup.blocksRaycasts = true;
    }

    private void HideMenu()
    {
        if (menuCanvasGroup == null)
            return;

        menuCanvasGroup.alpha = 0f;
        menuCanvasGroup.interactable = false;
        menuCanvasGroup.blocksRaycasts = false;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
