using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentCollisionCheck : MonoBehaviour
{
    private GameObject holeObj;
    private bool colliding;
    private BallController ballCont;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Environment"))
        {
            Debug.Log("Colliding");
            colliding = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Environment"))
        {
            colliding = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Environment"))
        {
            colliding = true;
        }
    }

    private void Start()
    {
        holeObj = GameObject.FindGameObjectWithTag("Hole");
        ballCont = FindObjectOfType<BallController>();
    }

    private void Update()
    {
        if (colliding)
        {
            holeObj.transform.position += new Vector3(0, 1, 0);
            ballCont.UpdateBallSpawnPos();
        }
    }
}
