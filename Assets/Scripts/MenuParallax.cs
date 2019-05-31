using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuParallax : MonoBehaviour
{
    public RectTransform canvas;
    public float smoothTime = 0.3f;
    public float minXAxis, minYAxis;
    public float maxXAxis, maxYAxis;
    public float mouseSensitivity = 50.0f;
    private float xVelocity = 0.0f;
    private float yVelocity = 0.0f;
    private float x;
    private float y;
    private float xSmooth;
    private float ySmooth;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        x -= Input.GetAxis("Mouse X") * mouseSensitivity * 0.02f;
        y -= Input.GetAxis("Mouse Y") * mouseSensitivity * 0.02f;

        if (y > maxYAxis) y = maxYAxis;
        if (y < minYAxis) y = minYAxis;
        if (x > maxXAxis) x = maxXAxis;
        if (x < minXAxis) x = minXAxis;

        xSmooth = Mathf.SmoothDamp(xSmooth, x, ref xVelocity, smoothTime);
        ySmooth = Mathf.SmoothDamp(ySmooth, y, ref yVelocity, smoothTime);

        Quaternion parallax = new Quaternion();
        parallax = Quaternion.Euler(ySmooth / 7, xSmooth / 7, 0);

        canvas.transform.localPosition = new Vector3(xSmooth, ySmooth, 0f);
        canvas.transform.localRotation = parallax;
    }
}
