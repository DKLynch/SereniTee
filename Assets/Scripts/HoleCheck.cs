using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleCheck : MonoBehaviour
{
    public float timeToWait;
    private bool goalCheckInProg = false;   
    private GameController gameCont;
    private BallController ballCont;
    private GameData gameData;
    
    void Awake()
    {
        gameCont = FindObjectOfType<GameController>();
        ballCont = FindObjectOfType<BallController>();
        gameData = FindObjectOfType<GameData>();
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Ball") && !goalCheckInProg)
        {
            Debug.Log("Ball Entered Hole");
            ballCont.isBallInHole = true;
            StartCoroutine(GoalCheck(timeToWait, other));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            Debug.Log("Ball Exited Hole");
            ballCont.isBallInHole = false;
        }
    }

    public IEnumerator GoalCheck(float timeToWait, Collider ball)
    {
        //Check if the ball is still overlapping the 'goal' collider after the designated time.
        goalCheckInProg = true;
        yield return new WaitForSeconds(timeToWait);

        if (this.GetComponent<Collider>().bounds.Intersects(ball.bounds)){
            //Hole Complete
            Debug.Log("Hole Complete");

            //Deparent the object until we manually destroy it at a later time to prevent Coroutine cancellation.
            this.transform.parent = gameCont.transform;
            StartCoroutine(gameCont.MoveToNextHole());
        }

        goalCheckInProg = false;
    }
}
