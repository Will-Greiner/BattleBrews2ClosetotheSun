using UnityEngine;

public class EnchantmentAnimHelper : MonoBehaviour
{
    [SerializeField] private ParticleSystem swirlParticle;

    private void Awake()
    {
        StopSwirl();
    }

    public void PlaySwirl()
    {
        if (swirlParticle == null)
            return;

        swirlParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        swirlParticle.Play(true);
    }

    public void StopSwirl()
    {
        if (swirlParticle != null)
            swirlParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
