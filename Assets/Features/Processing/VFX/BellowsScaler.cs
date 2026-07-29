using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using System;
public class BellowsScaler : MonoBehaviour
{
    [SerializeField] GameObject mainFire;
    [SerializeField] ParticleSystem bellowsEffect;
    public float firePercentage;
    public float timetoFade = 4f;
    public float timetoFlare = 1f;

    private Coroutine activeFadeCoroutine;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Stoke()
    {
        if (activeFadeCoroutine != null)
        {
            StopCoroutine(activeFadeCoroutine);
        }
        StartCoroutine(FireStoked());
        bellowsEffect.Emit(50);
    }
    
    private IEnumerator FireFade()
    {
        float timeSpent = 0f;
        float startPercentage = firePercentage;

        while(timeSpent < timetoFade)
        {
            timeSpent = timeSpent + Time.deltaTime;
            firePercentage = Mathf.Lerp(startPercentage, 0, timeSpent/timetoFade) / 100f;
            mainFire.transform.localScale = new Vector3(firePercentage, firePercentage, firePercentage);
            yield return null;
        }
        firePercentage = 0f;
        mainFire.transform.localScale = Vector3.zero;
        activeFadeCoroutine = null;
    }

    private IEnumerator FireStoked()
    {
        float timeSpent = 0f;

        while(timeSpent < timetoFlare)
        {
            timeSpent = timeSpent + Time.deltaTime;
            firePercentage = Mathf.Lerp(firePercentage, 1, timeSpent/timetoFade) / 100f;
            mainFire.transform.localScale = new Vector3(firePercentage, firePercentage, firePercentage);
            yield return null;
        }
        mainFire.transform.localScale = new Vector3(firePercentage, firePercentage, firePercentage);
        activeFadeCoroutine = StartCoroutine(FireFade());
    }
}
