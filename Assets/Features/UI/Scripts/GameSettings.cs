using UnityEngine;

public static class GameSettings
{
    private const string MasterVolumeKey = "settings.masterVolume";
    private const string CameraSpeedKey = "settings.cameraSpeed";
    private const string FullscreenKey = "settings.fullscreen";
    private const string QualityKey = "settings.quality";

    public static float MasterVolume { get; private set; } = 1f;
    public static float CameraSpeedMultiplier { get; private set; } = 1f;
    public static bool Fullscreen { get; private set; } = true;
    public static int QualityLevel { get; private set; }

    public static void LoadAndApply()
    {
        MasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        CameraSpeedMultiplier = PlayerPrefs.GetFloat(CameraSpeedKey, 1f);
        Fullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
        QualityLevel = Mathf.Clamp(PlayerPrefs.GetInt(QualityKey, QualitySettings.GetQualityLevel()), 0, Mathf.Max(0, QualitySettings.names.Length - 1));
        Apply();
    }

    public static void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        AudioListener.volume = MasterVolume;
        PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
        PlayerPrefs.Save();
    }

    public static void SetCameraSpeed(float value)
    {
        CameraSpeedMultiplier = Mathf.Clamp(value, 0.25f, 2f);
        PlayerPrefs.SetFloat(CameraSpeedKey, CameraSpeedMultiplier);
        PlayerPrefs.Save();
    }

    public static void SetFullscreen(bool value)
    {
        Fullscreen = value;
        Screen.fullScreen = Fullscreen;
        PlayerPrefs.SetInt(FullscreenKey, Fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void SetQualityLevel(int value)
    {
        QualityLevel = Mathf.Clamp(value, 0, Mathf.Max(0, QualitySettings.names.Length - 1));
        QualitySettings.SetQualityLevel(QualityLevel, true);
        PlayerPrefs.SetInt(QualityKey, QualityLevel);
        PlayerPrefs.Save();
    }

    private static void Apply()
    {
        AudioListener.volume = MasterVolume;
        Screen.fullScreen = Fullscreen;

        if (QualitySettings.names.Length > 0)
            QualitySettings.SetQualityLevel(QualityLevel, true);
    }
}
