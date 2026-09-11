using System;
using System.Collections.Generic;
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
    [SerializeField] private Slider ambienceVolumeSlider;
    [SerializeField] private Slider cameraSpeedSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown displayModeDropdown;
    [SerializeField] private Toggle vSyncToggle;
    [SerializeField] private Toggle subtitlesToggle;
    [SerializeField] private Toggle screenShakeToggle;
    [SerializeField] private Toggle reduceMotionToggle;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Button closeButton;

    public bool IsOpen => canvasGroup != null && canvasGroup.alpha > 0.99f;
    public event Action Closed;

    private readonly List<Vector2Int> availableResolutions = new();

    private void Awake()
    {
        GameSettings.LoadAndApply();
        PopulateResolutionOptions();
        PopulateDisplayModeOptions();
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
        if (ambienceVolumeSlider != null) ambienceVolumeSlider.onValueChanged.RemoveListener(GameSettings.SetAmbienceVolume);
        if (cameraSpeedSlider != null) cameraSpeedSlider.onValueChanged.RemoveListener(GameSettings.SetCameraSpeed);
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.RemoveListener(GameSettings.SetFullscreen);
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.RemoveListener(SetResolutionFromDropdown);
        if (displayModeDropdown != null) displayModeDropdown.onValueChanged.RemoveListener(GameSettings.SetDisplayMode);
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
        if (ambienceVolumeSlider != null) ambienceVolumeSlider.onValueChanged.AddListener(GameSettings.SetAmbienceVolume);
        if (cameraSpeedSlider != null) cameraSpeedSlider.onValueChanged.AddListener(GameSettings.SetCameraSpeed);
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(GameSettings.SetFullscreen);
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.AddListener(SetResolutionFromDropdown);
        if (displayModeDropdown != null) displayModeDropdown.onValueChanged.AddListener(GameSettings.SetDisplayMode);
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

    private void PopulateResolutionOptions()
    {
        if (resolutionDropdown == null)
            return;

        availableResolutions.Clear();
        HashSet<Vector2Int> uniqueResolutions = new();

        foreach (Resolution resolution in Screen.resolutions)
            uniqueResolutions.Add(new Vector2Int(resolution.width, resolution.height));

        uniqueResolutions.Add(new Vector2Int(Screen.width, Screen.height));
        availableResolutions.AddRange(uniqueResolutions);
        availableResolutions.Sort((left, right) => left.x != right.x ? left.x.CompareTo(right.x) : left.y.CompareTo(right.y));

        List<string> labels = new();

        foreach (Vector2Int resolution in availableResolutions)
            labels.Add($"{resolution.x} x {resolution.y}");

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(labels);
    }

    private void PopulateDisplayModeOptions()
    {
        if (displayModeDropdown == null)
            return;

        displayModeDropdown.ClearOptions();
        displayModeDropdown.AddOptions(new List<string> { "Fullscreen", "Borderless Window", "Windowed" });
    }

    private void SetResolutionFromDropdown(int index)
    {
        if (index < 0 || index >= availableResolutions.Count)
            return;

        Vector2Int resolution = availableResolutions[index];
        GameSettings.SetResolution(resolution.x, resolution.y);
    }

    private void RefreshControls()
    {
        if (masterVolumeSlider != null) masterVolumeSlider.SetValueWithoutNotify(GameSettings.MasterVolume);
        if (musicVolumeSlider != null) musicVolumeSlider.SetValueWithoutNotify(GameSettings.MusicVolume);
        if (soundEffectsVolumeSlider != null) soundEffectsVolumeSlider.SetValueWithoutNotify(GameSettings.SoundEffectsVolume);
        if (userInterfaceVolumeSlider != null) userInterfaceVolumeSlider.SetValueWithoutNotify(GameSettings.UserInterfaceVolume);
        if (ambienceVolumeSlider != null) ambienceVolumeSlider.SetValueWithoutNotify(GameSettings.AmbienceVolume);
        if (cameraSpeedSlider != null) cameraSpeedSlider.SetValueWithoutNotify(GameSettings.CameraSpeedMultiplier);
        if (fullscreenToggle != null) fullscreenToggle.SetIsOnWithoutNotify(GameSettings.Fullscreen);
        if (resolutionDropdown != null) resolutionDropdown.SetValueWithoutNotify(FindCurrentResolutionIndex());
        if (displayModeDropdown != null) displayModeDropdown.SetValueWithoutNotify((int)GameSettings.CurrentDisplayMode);
        if (vSyncToggle != null) vSyncToggle.SetIsOnWithoutNotify(GameSettings.VSync);
        if (subtitlesToggle != null) subtitlesToggle.SetIsOnWithoutNotify(GameSettings.Subtitles);
        if (screenShakeToggle != null) screenShakeToggle.SetIsOnWithoutNotify(GameSettings.ScreenShake);
        if (reduceMotionToggle != null) reduceMotionToggle.SetIsOnWithoutNotify(GameSettings.ReduceMotion);
        if (qualityDropdown != null) qualityDropdown.SetValueWithoutNotify(GameSettings.QualityLevel);
    }

    private int FindCurrentResolutionIndex()
    {
        Vector2Int selectedResolution = new(GameSettings.ResolutionWidth, GameSettings.ResolutionHeight);
        int exactIndex = availableResolutions.IndexOf(selectedResolution);

        if (exactIndex >= 0)
            return exactIndex;

        int nearestIndex = 0;
        int nearestDifference = int.MaxValue;

        for (int i = 0; i < availableResolutions.Count; i++)
        {
            Vector2Int resolution = availableResolutions[i];
            int difference = Mathf.Abs(resolution.x - selectedResolution.x) + Mathf.Abs(resolution.y - selectedResolution.y);

            if (difference < nearestDifference)
            {
                nearestDifference = difference;
                nearestIndex = i;
            }
        }

        return nearestIndex;
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
