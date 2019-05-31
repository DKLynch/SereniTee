using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FlagRaise : MonoBehaviour
{
    public Transform flagMesh;
    public float timeToMove;
    public float height;
    private Tween raiseTween, lowerTween;
    private bool flagRaised = false;
    private float startingHeight;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball") && !flagRaised) RaiseFlag();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ball") && flagRaised) LowerFlag();
    }

    private void RaiseFlag()
    {
        startingHeight = flagMesh.position.y;
        if(lowerTween != null) lowerTween.Kill();

        raiseTween = flagMesh.DOMoveY(startingHeight + height, timeToMove)
            .SetEase(Ease.InOutCubic);

        flagRaised = true;
    }

    private void LowerFlag()
    {
        if (raiseTween != null) raiseTween.Kill();

        lowerTween = flagMesh.DOMoveY(startingHeight, timeToMove)
            .SetEase(Ease.InOutCubic);

        flagRaised = false;
    }
}
