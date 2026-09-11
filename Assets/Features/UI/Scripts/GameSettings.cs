using System;
using UnityEngine;

public static class GameSettings
{
    public enum DisplayMode
    {
        Fullscreen,
        BorderlessWindow,
        Windowed
    }

    private const string MasterVolumeKey = "settings.masterVolume";
    private const string MusicVolumeKey = "settings.musicVolume";
    private const string SoundEffectsVolumeKey = "settings.soundEffectsVolume";
    private const string UserInterfaceVolumeKey = "settings.userInterfaceVolume";
    private const string AmbienceVolumeKey = "settings.ambienceVolume";
    private const string CameraSpeedKey = "settings.cameraSpeed";
    private const string FullscreenKey = "settings.fullscreen";
    private const string DisplayModeKey = "settings.displayMode";
    private const string ResolutionWidthKey = "settings.resolutionWidth";
    private const string ResolutionHeightKey = "settings.resolutionHeight";
    private const string QualityKey = "settings.quality";
    private const string VSyncKey = "settings.vSync";
    private const string SubtitlesKey = "settings.subtitles";
    private const string ScreenShakeKey = "settings.screenShake";
    private const string ReduceMotionKey = "settings.reduceMotion";

    public static float MasterVolume { get; private set; } = 1f;
    public static float MusicVolume { get; private set; } = 1f;
    public static float SoundEffectsVolume { get; private set; } = 1f;
    public static float UserInterfaceVolume { get; private set; } = 1f;
    public static float AmbienceVolume { get; private set; } = 1f;
    public static float CameraSpeedMultiplier { get; private set; } = 1f;
    public static DisplayMode CurrentDisplayMode { get; private set; } = DisplayMode.BorderlessWindow;
    public static int ResolutionWidth { get; private set; }
    public static int ResolutionHeight { get; private set; }
    public static bool Fullscreen => CurrentDisplayMode != DisplayMode.Windowed;
    public static int QualityLevel { get; private set; }
    public static bool VSync { get; private set; } = true;
    public static bool Subtitles { get; private set; } = true;
    public static bool ScreenShake { get; private set; } = true;
    public static bool ReduceMotion { get; private set; }

    public static event Action Changed;

    public static void LoadAndApply()
    {
        MasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        SoundEffectsVolume = PlayerPrefs.GetFloat(SoundEffectsVolumeKey, 1f);
        UserInterfaceVolume = PlayerPrefs.GetFloat(UserInterfaceVolumeKey, 1f);
        AmbienceVolume = PlayerPrefs.GetFloat(AmbienceVolumeKey, 1f);
        CameraSpeedMultiplier = PlayerPrefs.GetFloat(CameraSpeedKey, 1f);
        int defaultDisplayMode = Screen.fullScreenMode == FullScreenMode.Windowed ? (int)DisplayMode.Windowed : (int)DisplayMode.BorderlessWindow;
        CurrentDisplayMode = (DisplayMode)Mathf.Clamp(PlayerPrefs.GetInt(DisplayModeKey, defaultDisplayMode), 0, 2);
        ResolutionWidth = Mathf.Max(640, PlayerPrefs.GetInt(ResolutionWidthKey, Screen.width));
        ResolutionHeight = Mathf.Max(480, PlayerPrefs.GetInt(ResolutionHeightKey, Screen.height));
        QualityLevel = Mathf.Clamp(PlayerPrefs.GetInt(QualityKey, QualitySettings.GetQualityLevel()), 0, Mathf.Max(0, QualitySettings.names.Length - 1));
        VSync = PlayerPrefs.GetInt(VSyncKey, QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;
        Subtitles = PlayerPrefs.GetInt(SubtitlesKey, 1) == 1;
        ScreenShake = PlayerPrefs.GetInt(ScreenShakeKey, 1) == 1;
        ReduceMotion = PlayerPrefs.GetInt(ReduceMotionKey, 0) == 1;
        Apply();
    }

    public static void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        AudioListener.volume = MasterVolume;
        PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
        SaveAndNotify();
    }

    public static void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
        SaveAndNotify();
    }

    public static void SetSoundEffectsVolume(float value)
    {
        SoundEffectsVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SoundEffectsVolumeKey, SoundEffectsVolume);
        SaveAndNotify();
    }

    public static void SetUserInterfaceVolume(float value)
    {
        UserInterfaceVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(UserInterfaceVolumeKey, UserInterfaceVolume);
        SaveAndNotify();
    }

    public static void SetAmbienceVolume(float value)
    {
        AmbienceVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(AmbienceVolumeKey, AmbienceVolume);
        SaveAndNotify();
    }

    public static void SetCameraSpeed(float value)
    {
        CameraSpeedMultiplier = Mathf.Clamp(value, 0.25f, 2f);
        PlayerPrefs.SetFloat(CameraSpeedKey, CameraSpeedMultiplier);
        SaveAndNotify();
    }

    public static void SetFullscreen(bool value)
    {
        SetDisplayMode(value ? (int)DisplayMode.BorderlessWindow : (int)DisplayMode.Windowed);
    }

    public static void SetDisplayMode(int value)
    {
        CurrentDisplayMode = (DisplayMode)Mathf.Clamp(value, 0, 2);
        PlayerPrefs.SetInt(DisplayModeKey, (int)CurrentDisplayMode);
        PlayerPrefs.SetInt(FullscreenKey, CurrentDisplayMode == DisplayMode.Windowed ? 0 : 1);
        ApplyResolutionAndDisplayMode();
        SaveAndNotify();
    }

    public static void SetResolution(int width, int height)
    {
        ResolutionWidth = Mathf.Max(640, width);
        ResolutionHeight = Mathf.Max(480, height);
        PlayerPrefs.SetInt(ResolutionWidthKey, ResolutionWidth);
        PlayerPrefs.SetInt(ResolutionHeightKey, ResolutionHeight);
        ApplyResolutionAndDisplayMode();
        SaveAndNotify();
    }

    public static void SetQualityLevel(int value)
    {
        QualityLevel = Mathf.Clamp(value, 0, Mathf.Max(0, QualitySettings.names.Length - 1));
        QualitySettings.SetQualityLevel(QualityLevel, true);
        PlayerPrefs.SetInt(QualityKey, QualityLevel);
        SaveAndNotify();
    }

    public static void SetVSync(bool value)
    {
        VSync = value;
        QualitySettings.vSyncCount = VSync ? 1 : 0;
        PlayerPrefs.SetInt(VSyncKey, VSync ? 1 : 0);
        SaveAndNotify();
    }

    public static void SetSubtitles(bool value)
    {
        Subtitles = value;
        PlayerPrefs.SetInt(SubtitlesKey, Subtitles ? 1 : 0);
        SaveAndNotify();
    }

    public static void SetScreenShake(bool value)
    {
        ScreenShake = value;
        PlayerPrefs.SetInt(ScreenShakeKey, ScreenShake ? 1 : 0);
        SaveAndNotify();
    }

    public static void SetReduceMotion(bool value)
    {
        ReduceMotion = value;
        PlayerPrefs.SetInt(ReduceMotionKey, ReduceMotion ? 1 : 0);
        SaveAndNotify();
    }

    private static void Apply()
    {
        AudioListener.volume = MasterVolume;
        ApplyResolutionAndDisplayMode();

        if (QualitySettings.names.Length > 0)
            QualitySettings.SetQualityLevel(QualityLevel, true);

        QualitySettings.vSyncCount = VSync ? 1 : 0;

        Changed?.Invoke();
    }

    private static void ApplyResolutionAndDisplayMode()
    {
        FullScreenMode unityMode = CurrentDisplayMode switch
        {
            DisplayMode.Fullscreen => FullScreenMode.ExclusiveFullScreen,
            DisplayMode.BorderlessWindow => FullScreenMode.FullScreenWindow,
            _ => FullScreenMode.Windowed
        };

        Screen.SetResolution(ResolutionWidth, ResolutionHeight, unityMode);
    }

    private static void SaveAndNotify()
    {
        PlayerPrefs.Save();
        Changed?.Invoke();
    }
}
