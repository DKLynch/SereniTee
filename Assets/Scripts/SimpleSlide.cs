using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SimpleSlide : MonoBehaviour
{
    public float seconds;
    public float delay;
    [Range(0, 0.5f)]
    public float offset;
    public float distance;
    public Ease easeMode;
    private Tween animTween;

    private void Awake()
    {
        transform.position += (-transform.forward * distance);
    }

    private void Start()
    {
        InitiateSlide();
    }

    private void InitiateSlide()
    {
        animTween = transform.DOLocalMoveZ(distance, seconds).
            SetEase(easeMode).
            SetLoops(-1, LoopType.Yoyo).
            OnStepComplete(TriggerDelay);
    }

    public void TriggerDelay()
    {
        StartCoroutine("DelayLoop");
    }

    private IEnumerator DelayLoop()
    {
        var tempDelay = delay;

        if (delay > 0)
        {
            if (offset > 0) delay = delay + Random.Range(-offset, offset);
            animTween.Pause();
            yield return new WaitForSeconds(delay);
            animTween.Play();
        }
        delay = tempDelay;
    }

    private void OnDestroy()
    {
        if(animTween != null) animTween.Kill();
    }
}
