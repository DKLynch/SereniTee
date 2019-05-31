using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallFollow : MonoBehaviour
{
    public bool useDamping;
    public float dampingTime;
    private Rigidbody ballRbody;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        //Fetch the ball's rigidbody
        ballRbody = GameObject.FindGameObjectWithTag("Ball").GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        //Follow with camera damping
        if (useDamping)
        {
            Vector3 targetPos = ballRbody.position;
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, dampingTime);
        }
    }

    private void Update()
    {
        //Follow immediately
        if (!useDamping) this.transform.position = ballRbody.position;
    }
}
