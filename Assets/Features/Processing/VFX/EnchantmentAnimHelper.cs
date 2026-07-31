using UnityEngine;

public class EnchantmentAnimHelper : MonoBehaviour
{
    public ParticleSystem swirlParticle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Awake()
    {
        swirlParticle.Stop();
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaySwirl()
    {
        if(swirlParticle != null)
        {
            swirlParticle.Play();
        }
    }
    public void StopSwirl()
    {
        if(swirlParticle != null)
        {
            swirlParticle.Stop();
        }
    }
}
