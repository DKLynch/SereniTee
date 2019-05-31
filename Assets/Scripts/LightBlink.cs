using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LightBlink : MonoBehaviour
{
    public float blinkEvery;
    public float timeActive;
    private Light l;
    private Sequence blinkSeq;

    void Awake()
    {
        l = this.GetComponent<Light>();
        l.enabled = true;
        BlinkLight();
    }

    void BlinkLight()
    {
        blinkSeq = DOTween.Sequence();
        blinkSeq.AppendInterval(timeActive).
            AppendCallback(DisableLight).
            AppendInterval(blinkEvery).
            AppendCallback(EnableLight).
            SetLoops(-1, LoopType.Restart);
    }

    void EnableLight()
    {
        l.enabled = true;
    }

    void DisableLight()
    {
        l.enabled = false;
    }

    private void OnDestroy()
    {
        blinkSeq.Kill();
    }
}
