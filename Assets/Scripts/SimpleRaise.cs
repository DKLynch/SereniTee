using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SimpleRaise : MonoBehaviour
{
    public float seconds;
    public float distance;
    public float delay;
    [Range(0f, 0.75f)]
    public float offset;
    public Ease easeMode;
    private Tween animTween;

    private void Awake()
    {
    }

    private void Start()
    {
        InitializeRaise();   
    }

    private void InitializeRaise()
    {
        animTween = transform.DOLocalMoveY(distance, seconds).
            SetEase(easeMode).
            SetLoops(-1, LoopType.Yoyo).
            OnStepComplete(TriggerDelay);
    }

    private void TriggerDelay()
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
        if (animTween != null) animTween.Kill();
    }
}
