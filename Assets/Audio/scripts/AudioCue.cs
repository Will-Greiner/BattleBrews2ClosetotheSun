using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "New Audio Cue", menuName = "Battle Brews/Audio Cue")]
public class AudioCue : ScriptableObject
{
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private AudioMixerGroup mixerGroup;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);
    [SerializeField] private bool spatial;
    [SerializeField, Min(0f)] private float minimumDistance = 1f;
    [SerializeField, Min(0f)] private float maximumDistance = 15f;

    public AudioClip RandomClip => clips != null && clips.Length > 0 ? clips[Random.Range(0, clips.Length)] : null;
    public AudioMixerGroup MixerGroup => mixerGroup;
    public float Volume => volume;
    public float RandomPitch => Random.Range(pitchRange.x, pitchRange.y);
    public bool Spatial => spatial;
    public float MinimumDistance => minimumDistance;
    public float MaximumDistance => maximumDistance;
}