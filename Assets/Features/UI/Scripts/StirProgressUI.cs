using UnityEngine;
using UnityEngine.UI;

public class StirProgressUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StirringStick stirringStick;
    [SerializeField] private GameObject progressRoot;
    [SerializeField] private Image progressFill;

    private void Awake()
    {
        if (progressFill != null)
            progressFill.fillAmount = 0f;

        if (progressRoot != null)
            progressRoot.SetActive(false);
    }

    private void LateUpdate()
    {
        if (stirringStick == null || progressRoot == null || progressFill == null)
            return;

        bool shouldShow = stirringStick.IsStirring;

        if (progressRoot.activeSelf != shouldShow)
            progressRoot.SetActive(shouldShow);

        if (!shouldShow)
        {
            progressFill.fillAmount = 0f;
            return;
        }

        progressFill.fillAmount = stirringStick.StirProgress;
    }
}