using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    private Transform camTarget;
    private Rigidbody camTargetRb;
    public int rotCounter = 1;
    private Camera cam;
    private InputHandler inputHandler;
    private BallFollow follow;

    void Awake()
    {
        camTarget = GameObject.FindGameObjectWithTag("CamTarget").transform;
        camTargetRb = camTarget.GetComponent<Rigidbody>();
        cam = this.GetComponent<Camera>();
        inputHandler = FindObjectOfType<InputHandler>();
        follow = FindObjectsOfType<BallFollow>().First(b => b.name == "CamTarget");
    }

    public void LockCamInput()
    {
        inputHandler.camInputLocked = true;
        inputHandler.allInputLocked = true;
    }

    public void FreeCamInput()
    {
        inputHandler.camInputLocked = false;
        inputHandler.allInputLocked = false;
    }

    //Flies the camera through a list of waypoints (our hole tiles)
    public void HoleFlythrough(List<Vector3> waypoints, float seconds, bool isReversed, Ease easeType)
    {
        //Detach the camTarget from following the ball
        DetachCamFromBall();

        //Reverse the waypoints if necessary and cast the list to an array
        Vector3[] wpArray = new Vector3[waypoints.Count];
        if (isReversed)
        {
            waypoints.Reverse();
            wpArray = waypoints.ToArray();
            waypoints.Reverse();
        } else
        {
            wpArray = waypoints.ToArray();
        }

        camTargetRb.DOPath(wpArray, seconds, PathType.CatmullRom, PathMode.Ignore, 10, Color.green)
            .SetEase(easeType)
            .OnStart(LockCamInput)
            .OnComplete(ReturnToBall);
            
    }

    public void ZoomCamIn()
    {
        if (cam.fieldOfView > 10)
        {
            cam.DOFieldOfView(cam.fieldOfView - 5, .5f)
           .SetEase(Ease.InOutCubic)
           .OnStart(LockCamInput)
           .OnComplete(FreeCamInput);
        }
    }

    public void ZoomCamOut()
    {
        if (cam.fieldOfView < 25)
        {
            cam.DOFieldOfView(cam.fieldOfView + 5, .5f)
                .SetEase(Ease.InOutCubic)
                .OnStart(LockCamInput)
                .OnComplete(FreeCamInput);
        }
    }

    public void RotateCameraClockwise()
    {
        if (rotCounter == 1)
        {
            rotCounter = 8;
        }
        else
        {
            rotCounter--;
        }

        camTarget.DORotate(new Vector3(0, camTarget.rotation.y - 45, 0), 0.75f, RotateMode.FastBeyond360)
            .SetRelative(true)
            .SetEase(Ease.OutBack, 1.5f)
            .OnStart(LockCamInput)
            .OnComplete(SnapCamRotation);

        Invoke("FreeCamInput", .75f);
    }

    public void RotateCameraCounterClockwise()
    {
        if (rotCounter == 8)
        {
            rotCounter = 1;
        }
        else
        {
            rotCounter++;
        }

        camTarget.DORotate(new Vector3(0, camTarget.rotation.y + 45, 0), 0.75f, RotateMode.FastBeyond360)
            .SetRelative(true)
            .SetEase(Ease.OutBack, 1.5f)
            .OnStart(LockCamInput)
            .OnComplete(SnapCamRotation);

        Invoke("FreeCamInput", .75f);
    }

    //Snaps camera rotation to whole integer values after a rotation to prevent floating point errors
    private void SnapCamRotation()
    {
        switch (rotCounter)
        {
            case 1:
                camTarget.rotation = Quaternion.Euler(30, 45, 0);
                break;
            case 2:
                camTarget.rotation = Quaternion.Euler(30, 90, 0);
                break;
            case 3:
                camTarget.rotation = Quaternion.Euler(30, 135, 0);
                break;
            case 4:
                camTarget.rotation = Quaternion.Euler(30, 180, 0);
                break;
            case 5:
                camTarget.rotation = Quaternion.Euler(30, 225, 0);
                break;
            case 6:
                camTarget.rotation = Quaternion.Euler(30, 270, 0);
                break;
            case 7:
                camTarget.rotation = Quaternion.Euler(30, 315, 0);
                break;
            case 8:
                camTarget.rotation = Quaternion.Euler(30, 0, 0);
                break;
        }

    }

    public void AttachCamToBall()
    {
        follow.enabled = true;
    }

    public void DetachCamFromBall()
    {
        follow.enabled = false;
    }

    public void ReturnToBall()
    {
        camTargetRb.DOMove(GameObject.FindGameObjectWithTag("Ball").transform.position, 1f, false)
            .SetEase(Ease.InOutQuart)
            .OnComplete(FreeCamInput);

        Invoke("AttachCamToBall", 1f);
    }
}
