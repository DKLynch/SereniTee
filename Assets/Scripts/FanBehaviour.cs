using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanBehaviour : MonoBehaviour
{
    [SerializeField] private float fanStrength = 25f;
    private Rigidbody ballRBody;

    private void Awake()
    {
        ballRBody = GameObject.FindGameObjectWithTag("Ball").GetComponent<Rigidbody>();
    }

    //Adds an upward acceleration to the ball while in the fan's range
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            ballRBody.AddForce(transform.up * fanStrength, ForceMode.Acceleration);
        }
    }
}
