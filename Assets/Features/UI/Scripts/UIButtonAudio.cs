using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonAudio : MonoBehaviour
{
    [SerializeField] private AudioCue clickCue;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlayClick);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(PlayClick);
    }

    private void PlayClick()
    {
        AudioManager.Instance?.Play(clickCue);
    }
}