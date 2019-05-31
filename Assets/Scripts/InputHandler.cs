using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class InputHandler : MonoBehaviour
{
    public bool allInputLocked;
    public bool camInputLocked;

    private CameraController camCont;
    private HoleGen holeGen;
    private BallController ballCont;
    private GameController gameCont;
    private DefaultMats defMats;
    private PauseHandler pauseHandler;

    void Start()
    {
        camCont = FindObjectOfType<CameraController>();
        holeGen = FindObjectOfType<HoleGen>();
        ballCont = FindObjectOfType<BallController>();
        gameCont = FindObjectOfType<GameController>();
        defMats = FindObjectOfType<DefaultMats>();
        pauseHandler = FindObjectOfType<PauseHandler>();
    }

    void Update()
    {
        if (!allInputLocked)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //Start the shot upon left click
                ballCont.StartShot();
            }

            if (Input.GetMouseButtonUp(0))
            {
                //Take the shot upon left click release
                ballCont.TakeShot();
            }

            if (Input.GetMouseButtonDown(1))
            {
                //Clear the current shot input upon right click while left mouse button is down
                ballCont.ClearInput();
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                //Return to previous shot position
                ballCont.RespawnBall(true, ballCont.prevShotPos);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                //Return to the start of the hole
                ballCont.RespawnBall(true, ballCont.ballSpawnPos);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                //Jump the ball if the ball is in motion and hop has not been used already this shot
                ballCont.HopBall();
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                //Toggle the ball's light on/off
                ballCont.ToggleBallLight();
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                //Dev/Debug tool for skipping holes
                StartCoroutine(gameCont.MoveToNextHole());
            }

            if (Input.GetKeyDown(KeyCode.T) && !defMats.xRayChanging)
            {
                //Toggle semi-transparent 'Xray' mode for the hole, in order to see when vision is otherwise blocked by the hole itself
                StartCoroutine(defMats.ToggleXRay());
            }

            if (!camInputLocked)
            {
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    camCont.RotateCameraCounterClockwise();
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    camCont.RotateCameraClockwise();
                }

                if (Input.GetKeyDown(KeyCode.Z))
                {
                    camCont.ZoomCamIn();
                }

                if (Input.GetKeyDown(KeyCode.X))
                {
                    camCont.ZoomCamOut();
                }

                if (Input.GetKeyDown(KeyCode.V))
                {
                    //Fly through the hole from tee-finish, giving the player an overview of what to come
                    camCont.HoleFlythrough(holeGen.tilePositions, (holeGen.tilePositions.Count / 4f), false, Ease.Linear);
                }

                if (Input.GetKeyDown(KeyCode.C))
                {
                    //Toggle Cam Target
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) && pauseHandler.isPausable)
        {
            //Pause the game
            pauseHandler.TogglePause();
        }

    }
}
