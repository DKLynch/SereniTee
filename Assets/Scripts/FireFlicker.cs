using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireFlicker : MonoBehaviour
{
    public float maxIntIncrease;
    public float maxIntDecrease;
    public float strength;
    public float rateDamping;
    public bool stopFlicker;

    private Light lightSource;
    private float baseInt;
    private bool flickering;

    private void Awake()
    {
        lightSource = GetComponent<Light>();
    }

    void Start()
    {
        baseInt = lightSource.intensity;
        StartCoroutine(Flicker());
    }

    private void Update()
    {
        if(!stopFlicker && !flickering)
        {
            StartCoroutine(Flicker()); 
        }
    }

    private IEnumerator Flicker()
    {
        flickering = true;
        while (!stopFlicker)
        {
            lightSource.intensity = Mathf.Lerp(lightSource.intensity, Random.Range(baseInt - maxIntDecrease, baseInt + maxIntIncrease), strength * Time.deltaTime);
            yield return new WaitForSeconds(rateDamping);
        }
        flickering = false;
    }
}
