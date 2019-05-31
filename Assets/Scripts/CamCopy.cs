using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamCopy : MonoBehaviour
{
    public Camera mainCam;
    public Camera thisCam;

    private void Awake()
    {
        thisCam = GetComponent<Camera>();
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    void Update()
    {
        //Copy our main camera's field of view whenever it's changed.
        thisCam.fieldOfView = mainCam.fieldOfView;
    }
}
