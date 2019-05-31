using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public Camera cam;

    void Awake()
    {
        cam = FindObjectOfType<Camera>();
    }

    void Update()
    {
        //Rotate the object to face the designated camera at all times
        Vector3 dir = cam.transform.position - transform.position;
        dir.x = dir.z = 0.0f;
        transform.LookAt(cam.transform.position - dir);
        transform.rotation = (cam.transform.rotation);
    }
}
