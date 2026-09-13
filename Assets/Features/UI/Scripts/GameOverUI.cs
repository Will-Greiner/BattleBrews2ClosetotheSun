using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private Button retryRoundButton;
    [SerializeField] private Button mainMenuButton;

    private bool subscribed;

    private void Awake()
    {
        if (retryRoundButton != null)
            retryRoundButton.onClick.AddListener(RetryRound);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);

        SetVisible(false);
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void Start()
    {
        Subscribe();

        if (GameManager.Instance != null && GameManager.Instance.State == GameState.GameOver)
            Show();
    }

    private void LateUpdate()
    {
        if (!subscribed)
            Subscribe();
    }

    private void OnDisable()
    {
        if (subscribed && GameManager.Instance != null)
            GameManager.Instance.GameEnded -= Show;

        subscribed = false;
        CursorManager.Instance?.HideForUI(this);
    }

    private void OnDestroy()
    {
        if (retryRoundButton != null)
            retryRoundButton.onClick.RemoveListener(RetryRound);

        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
    }

    public void Show()
    {
        if (roundText != null && GameManager.Instance != null)
            roundText.text = $"You reached round {GameManager.Instance.CurrentRound}";

        SetVisible(true);
        CursorManager.Instance?.ShowForUI(this);
    }

    public void RetryRound()
    {
        if (GameManager.Instance == null || !GameManager.Instance.RetryCurrentRound())
            return;

        SetVisible(false);
        CursorManager.Instance?.HideForUI(this);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        CursorManager.Instance?.HideForUI(this);
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    private void Subscribe()
    {
        if (subscribed || GameManager.Instance == null)
            return;

        GameManager.Instance.GameEnded += Show;
        subscribed = true;
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }
}
