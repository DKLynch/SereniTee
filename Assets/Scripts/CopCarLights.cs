using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopCarLights : MonoBehaviour
{
    public Light redLight;
    public Light blueLight;

    // Start is called before the first frame update
    void Start()
    {
        DeactivateLights();
    }

    public void ActivateLights()
    {
        bool activateLights = (Random.value > 0.25f);
        if (activateLights) StartCoroutine(FlashLights());
    }

    public void DeactivateLights()
    {
        StopAllCoroutines();
        redLight.enabled = false;
        blueLight.enabled = false;
    }

    //Simple sequence to flash the lights mimicking a police car
    public IEnumerator FlashLights()
    {
        for(int i = 0; i<2; i++)
        {
            UpdateLight(blueLight, true);
            yield return new WaitForSeconds(.05f);
            UpdateLight(blueLight, false);
            yield return new WaitForSeconds(.05f);
        }

        yield return new WaitForSeconds(.1f);

        for (int i = 0; i < 2; i++)
        {
            UpdateLight(redLight, true);
            yield return new WaitForSeconds(.05f);
            UpdateLight(redLight, false);
            yield return new WaitForSeconds(.05f);
        }

        yield return new WaitForSeconds(.1f);
        StartCoroutine(FlashLights());
    }

    public void UpdateLight(Light l, bool b)
    {
        l.enabled = b;
    }
}
