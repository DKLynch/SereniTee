using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeonFlicker : MonoBehaviour
{
    private Material mat;
    public int matIndex = 0;
    [SerializeField] private Light l = new Light();

    // Start is called before the first frame update
    void Start()
    {
        var r = this.GetComponent<MeshRenderer>();
        mat = r.materials[matIndex];

        DisableLight();
        StartCoroutine(RandomFlicker());
    }

    private IEnumerator RandomFlicker()
    {
        EnableLight();
        yield return new WaitForSeconds(Random.Range(2, 6));
        DisableLight();
        yield return new WaitForSeconds(Random.Range(.1f, .5f));

        bool b = (Random.value > 0.5f);
        if (b) StartCoroutine(RapidFlicker());
        else StartCoroutine(RandomFlicker());
    }

    private IEnumerator RapidFlicker()
    {
        EnableLight();
        int r = Random.Range(3, 7);

        for (int i = 0; i < r; i++)
        {
            DisableLight();
            yield return new WaitForSeconds(0.075f);
            EnableLight();
            yield return new WaitForSeconds(.03f);
        }
        StartCoroutine(RandomFlicker());
    }

    private void EnableLight()
    {
        mat.EnableKeyword("_EMISSION");
        l.enabled = true;
    }

    private void DisableLight()
    {
        mat.DisableKeyword("_EMISSION");
        l.enabled = false;
    }
}
