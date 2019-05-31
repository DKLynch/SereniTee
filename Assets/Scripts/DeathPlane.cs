using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathPlane : MonoBehaviour
{
    private BallController ballCont;

    void Start()
    {
        ballCont = FindObjectOfType<BallController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //If the colliding object is our ball, respawn the ball at the previous shot position
        if (other.CompareTag("Ball"))
        {
            Debug.Log("Ball hit death plane");
            ballCont.RespawnBall(true, ballCont.prevShotPos);
        }
    }
}
