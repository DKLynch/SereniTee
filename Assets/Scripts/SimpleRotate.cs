using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SimpleRotate : MonoBehaviour
{
    public float rotTime;
    public Vector3 angleToAdd;
    public bool flipFlop = false;

    // Start is called before the first frame update
    void Start()
    {
        var rb = this.GetComponent<Rigidbody>();
        rb.DORotate(angleToAdd, rotTime, RotateMode.LocalAxisAdd)
            .SetLoops(-1, flipFlop ? LoopType.Yoyo : LoopType.Restart)
            .SetEase(Ease.Linear);        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
