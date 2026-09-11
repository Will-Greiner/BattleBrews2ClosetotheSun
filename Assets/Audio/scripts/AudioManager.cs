using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField, Min(1)] private int initialPoolSize = 16;
    [SerializeField] private AudioMixer audioMixer;

    private readonly List<AudioSource> sources = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        for (int i = 0; i < initialPoolSize; i++)
            sources.Add(CreateSource());

        ApplyVolumeSettings();
        GameSettings.Changed += ApplyVolumeSettings;
    }

    private void OnDestroy()
    {
        GameSettings.Changed -= ApplyVolumeSettings;

        if (Instance == this)
            Instance = null;
    }

    public void Play(AudioCue cue)
    {
        PlayInternal(cue, transform.position, false);
    }

    public void PlayAtPosition(AudioCue cue, Vector3 position)
    {
        PlayInternal(cue, position, true);
    }

    private void PlayInternal(AudioCue cue, Vector3 position, bool useWorldPosition)
    {
        if (cue == null)
            return;

        AudioClip clip = cue.RandomClip;

        if (clip == null)
            return;

        AudioSource source = GetAvailableSource();
        source.transform.position = position;
        source.outputAudioMixerGroup = cue.MixerGroup;
        source.clip = clip;
        source.volume = cue.Volume;
        source.pitch = cue.RandomPitch;
        source.spatialBlend = useWorldPosition && cue.Spatial ? 1f : 0f;
        source.minDistance = cue.MinimumDistance;
        source.maxDistance = cue.MaximumDistance;
        source.Play();
    }

    private AudioSource GetAvailableSource()
    {
        foreach (AudioSource source in sources)
        {
            if (!source.isPlaying)
                return source;
        }

        AudioSource newSource = CreateSource();
        sources.Add(newSource);
        return newSource;
    }

    private AudioSource CreateSource()
    {
        GameObject sourceObject = new GameObject($"AudioSource {sources.Count + 1}");
        sourceObject.transform.SetParent(transform);
        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        return source;
    }

    private void ApplyVolumeSettings()
    {
        if (audioMixer == null)
            return;

        SetMixerVolume("MasterVolume", GameSettings.MasterVolume);
        SetMixerVolume("MusicVolume", GameSettings.MusicVolume);
        SetMixerVolume("SFXVolume", GameSettings.SoundEffectsVolume);
        SetMixerVolume("UIVolume", GameSettings.UserInterfaceVolume);
        SetMixerVolume("AmbienceVolume", GameSettings.AmbienceVolume);
    }

    private void SetMixerVolume(string parameterName, float linearVolume)
    {
        float decibels = linearVolume <= 0.0001f ? -80f : Mathf.Log10(linearVolume) * 20f;
        audioMixer.SetFloat(parameterName, decibels);
    }
}
