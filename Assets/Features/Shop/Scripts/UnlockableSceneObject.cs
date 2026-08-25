using UnityEngine;

public class UnlockableSceneObject : MonoBehaviour
{
    [SerializeField] private string unlockId;
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private MonoBehaviour[] functionality = new MonoBehaviour[0];
    [SerializeField] private ParticleSystem unlockParticles;

    private void OnEnable()
    {
        Subscribe();
        Refresh(false);
    }

    private void Start()
    {
        Subscribe();
        Refresh(false);
    }

    private void OnDisable()
    {
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.Instance.ContentUnlocked -= HandleContentUnlocked;
            ProgressionManager.Instance.ProgressionLoaded -= HandleProgressionLoaded;
        }
    }

    private void Subscribe()
    {
        if (ProgressionManager.Instance == null)
            return;

        ProgressionManager.Instance.ContentUnlocked -= HandleContentUnlocked;
        ProgressionManager.Instance.ProgressionLoaded -= HandleProgressionLoaded;
        ProgressionManager.Instance.ContentUnlocked += HandleContentUnlocked;
        ProgressionManager.Instance.ProgressionLoaded += HandleProgressionLoaded;
    }

    private void HandleContentUnlocked(string id)
    {
        if (id == unlockId)
            Refresh(true);
    }

    private void HandleProgressionLoaded() => Refresh(false);

    private void Refresh(bool playEffects)
    {
        bool unlocked = ProgressionManager.Instance != null && ProgressionManager.Instance.IsContentUnlocked(unlockId);

        if (visualRoot != null)
            visualRoot.SetActive(unlocked);

        foreach (MonoBehaviour behaviour in functionality)
            if (behaviour != null) behaviour.enabled = unlocked;

        if (unlocked && playEffects && unlockParticles != null)
            unlockParticles.Play();
    }
}
