using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider cameraSpeedSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Button closeButton;

    public bool IsOpen => canvasGroup != null && canvasGroup.alpha > 0.99f;

    private void Awake()
    {
        GameSettings.LoadAndApply();
        PopulateQualityOptions();
        RegisterListeners();
        RefreshControls();
        Hide();
    }

    private void OnDestroy()
    {
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveListener(GameSettings.SetMasterVolume);
        if (cameraSpeedSlider != null) cameraSpeedSlider.onValueChanged.RemoveListener(GameSettings.SetCameraSpeed);
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.RemoveListener(GameSettings.SetFullscreen);
        if (qualityDropdown != null) qualityDropdown.onValueChanged.RemoveListener(GameSettings.SetQualityLevel);
        if (closeButton != null) closeButton.onClick.RemoveListener(Hide);
    }

    public void Show()
    {
        RefreshControls();
        SetVisible(true);
    }

    public void Hide()
    {
        SetVisible(false);
    }

    private void RegisterListeners()
    {
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(GameSettings.SetMasterVolume);
        if (cameraSpeedSlider != null) cameraSpeedSlider.onValueChanged.AddListener(GameSettings.SetCameraSpeed);
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(GameSettings.SetFullscreen);
        if (qualityDropdown != null) qualityDropdown.onValueChanged.AddListener(GameSettings.SetQualityLevel);
        if (closeButton != null) closeButton.onClick.AddListener(Hide);
    }

    private void PopulateQualityOptions()
    {
        if (qualityDropdown == null)
            return;

        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
    }

    private void RefreshControls()
    {
        if (masterVolumeSlider != null) masterVolumeSlider.SetValueWithoutNotify(GameSettings.MasterVolume);
        if (cameraSpeedSlider != null) cameraSpeedSlider.SetValueWithoutNotify(GameSettings.CameraSpeedMultiplier);
        if (fullscreenToggle != null) fullscreenToggle.SetIsOnWithoutNotify(GameSettings.Fullscreen);
        if (qualityDropdown != null) qualityDropdown.SetValueWithoutNotify(GameSettings.QualityLevel);
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
