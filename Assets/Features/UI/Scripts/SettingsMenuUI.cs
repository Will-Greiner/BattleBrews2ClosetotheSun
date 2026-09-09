using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider soundEffectsVolumeSlider;
    [SerializeField] private Slider userInterfaceVolumeSlider;
    [SerializeField] private Slider cameraSpeedSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vSyncToggle;
    [SerializeField] private Toggle subtitlesToggle;
    [SerializeField] private Toggle screenShakeToggle;
    [SerializeField] private Toggle reduceMotionToggle;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Button closeButton;

    public bool IsOpen => canvasGroup != null && canvasGroup.alpha > 0.99f;
    public event Action Closed;

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
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.RemoveListener(GameSettings.SetMusicVolume);
        if (soundEffectsVolumeSlider != null) soundEffectsVolumeSlider.onValueChanged.RemoveListener(GameSettings.SetSoundEffectsVolume);
        if (userInterfaceVolumeSlider != null) userInterfaceVolumeSlider.onValueChanged.RemoveListener(GameSettings.SetUserInterfaceVolume);
        if (cameraSpeedSlider != null) cameraSpeedSlider.onValueChanged.RemoveListener(GameSettings.SetCameraSpeed);
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.RemoveListener(GameSettings.SetFullscreen);
        if (vSyncToggle != null) vSyncToggle.onValueChanged.RemoveListener(GameSettings.SetVSync);
        if (subtitlesToggle != null) subtitlesToggle.onValueChanged.RemoveListener(GameSettings.SetSubtitles);
        if (screenShakeToggle != null) screenShakeToggle.onValueChanged.RemoveListener(GameSettings.SetScreenShake);
        if (reduceMotionToggle != null) reduceMotionToggle.onValueChanged.RemoveListener(GameSettings.SetReduceMotion);
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
        bool wasOpen = IsOpen;
        SetVisible(false);

        if (wasOpen)
            Closed?.Invoke();
    }

    private void RegisterListeners()
    {
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(GameSettings.SetMasterVolume);
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(GameSettings.SetMusicVolume);
        if (soundEffectsVolumeSlider != null) soundEffectsVolumeSlider.onValueChanged.AddListener(GameSettings.SetSoundEffectsVolume);
        if (userInterfaceVolumeSlider != null) userInterfaceVolumeSlider.onValueChanged.AddListener(GameSettings.SetUserInterfaceVolume);
        if (cameraSpeedSlider != null) cameraSpeedSlider.onValueChanged.AddListener(GameSettings.SetCameraSpeed);
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(GameSettings.SetFullscreen);
        if (vSyncToggle != null) vSyncToggle.onValueChanged.AddListener(GameSettings.SetVSync);
        if (subtitlesToggle != null) subtitlesToggle.onValueChanged.AddListener(GameSettings.SetSubtitles);
        if (screenShakeToggle != null) screenShakeToggle.onValueChanged.AddListener(GameSettings.SetScreenShake);
        if (reduceMotionToggle != null) reduceMotionToggle.onValueChanged.AddListener(GameSettings.SetReduceMotion);
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
        if (musicVolumeSlider != null) musicVolumeSlider.SetValueWithoutNotify(GameSettings.MusicVolume);
        if (soundEffectsVolumeSlider != null) soundEffectsVolumeSlider.SetValueWithoutNotify(GameSettings.SoundEffectsVolume);
        if (userInterfaceVolumeSlider != null) userInterfaceVolumeSlider.SetValueWithoutNotify(GameSettings.UserInterfaceVolume);
        if (cameraSpeedSlider != null) cameraSpeedSlider.SetValueWithoutNotify(GameSettings.CameraSpeedMultiplier);
        if (fullscreenToggle != null) fullscreenToggle.SetIsOnWithoutNotify(GameSettings.Fullscreen);
        if (vSyncToggle != null) vSyncToggle.SetIsOnWithoutNotify(GameSettings.VSync);
        if (subtitlesToggle != null) subtitlesToggle.SetIsOnWithoutNotify(GameSettings.Subtitles);
        if (screenShakeToggle != null) screenShakeToggle.SetIsOnWithoutNotify(GameSettings.ScreenShake);
        if (reduceMotionToggle != null) reduceMotionToggle.SetIsOnWithoutNotify(GameSettings.ReduceMotion);
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
