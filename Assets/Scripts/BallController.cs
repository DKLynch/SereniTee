using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BallController : MonoBehaviour
{
    [Header("Booleans")]
    [SerializeField] private bool isMouseDown;
    [SerializeField] private bool 
        isBallGrounded,
        isBallStopped,
        isBallOutOfBounds,
        isBallOnWall,
        isBallOnProp,
        isHopAvailable,
        isShotReady,
        stopCheckInProg,
        envCheckInProg,
        wallCheckInProg,
        propCheckInProg;
    public bool isBallInHole;

    [Header("Positions")]
    [SerializeField] public Vector3 mouseCurrentPos;
    [SerializeField] public Vector3 
        mouseReleasePos, 
        ballSpawnPos, 
        prevShotPos, 
        tempShotPos, 
        stopCheckPos;

    [Header("Shooting Values")]
    [SerializeField] private float clampFactor = 17.5f;
    [SerializeField] private float 
        shotMulti = 3.5f,
        gravity = -9.5f;

    [Header("Materials")]
    [SerializeField] public Material ballMat;
    [SerializeField] public Material ballEmissiveMat;

    [Header("Miscellaneous")]
    [HideInInspector] private Collider shotPlane;
    [HideInInspector] private Camera cam;
    [HideInInspector] public MeshRenderer ballRenderer;
    [HideInInspector] private TrailRenderer ballTrail;
    [HideInInspector] public Rigidbody ballRbody;
    [HideInInspector] public Light ballLight;
    [HideInInspector] private LineRenderer shotLine;
    [HideInInspector] private SpriteRenderer shotCircle;
    [HideInInspector] private GameData gameData;
    [HideInInspector] private TextMeshProUGUI shotPower;
    [HideInInspector] private GameController gameCont;

    void Awake()
    {
        ballRbody = GetComponent<Rigidbody>();
        ballRenderer = GetComponent<MeshRenderer>();
        cam = FindObjectOfType<Camera>();
        gameCont = FindObjectOfType<GameController>();
        shotPlane = GameObject.FindGameObjectWithTag("ShotPlane").GetComponent<Collider>();
        ballSpawnPos = GameObject.FindGameObjectWithTag("BallSpawn").GetComponent<Transform>().position;
        var ballExtras = GameObject.FindGameObjectWithTag("BallExtras");
        ballLight = ballExtras.GetComponentInChildren<Light>();
        ballTrail = ballExtras.GetComponentInChildren<TrailRenderer>();
        shotCircle = ballExtras.GetComponentInChildren<SpriteRenderer>();
        shotLine = ballExtras.GetComponentInChildren<LineRenderer>();
        shotPower = GameObject.Find("ShotPower").GetComponent<TextMeshProUGUI>();

        //Re-set the light's colour to match the ball's
        ballLight.color = ballRenderer.material.color;
    }

    void Start()
    {
        //Fetch our persistent data
        gameData = FindObjectOfType<GameData>();
    }

    void FixedUpdate()
    {
        //Apply faux gravity to the ball's rigidbody
        ballRbody.AddForce(new Vector3(0, gravity, 0), ForceMode.Acceleration); 
    }

    void Update()
    {
        //Clamp velocity and start ball stop check when the ball is both grounded and at very low speed
        if (isBallGrounded && !isShotReady && ballRbody.velocity.magnitude < 0.1f) 
        {
            ClampVelocity();
            if (!stopCheckInProg)
            {
                StartCoroutine(StopCheck(.75f));
            }
        }

        //Prevent shots being taken and hide shot UI while ball is still in motion
        if(ballRbody.velocity.magnitude > 0.5f)
        {
            gravity = -10f;
            Physics.bounceThreshold = 0.1f;
            isShotReady = false;
            isBallStopped = false;
            shotCircle.enabled = false;
            shotPower.enabled = false;
        }

        //Display the shot circle to signify that the player can take a shot
        if(isBallGrounded && isShotReady && !isMouseDown)
        {
            shotCircle.enabled = true;
            shotCircle.color = new Color(shotCircle.color.r, shotCircle.color.g, shotCircle.color.b, 0.1f);
        }

        //Update the shot UI when the mouse is held
        if (isMouseDown)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if(shotPlane.Raycast(ray, out RaycastHit hit, 150f))
            {
                mouseCurrentPos = hit.point;
            }

            Vector3 lineDir = new Vector3(ballRbody.position.x - mouseCurrentPos.x, 0, ballRbody.position.z - mouseCurrentPos.z);
            Vector3 normLineDir = Vector3.Normalize(lineDir);

            //Clamp the lineDir vector to the radius of our shotcircle and set the second position accordingly
            Vector3 clampedLineDir = Vector3.ClampMagnitude(lineDir, 5f);
            shotLine.SetPosition(1, ballRbody.position - clampedLineDir);

            shotPower.text = ((Vector3.ClampMagnitude(normLineDir * (lineDir.magnitude * shotMulti), clampFactor).magnitude / clampFactor) * 100).ToString("0.#") + "%";
        }

        //Ball Check Raycasts
        if (envCheckInProg)
        {
            //Raycast downward to see if we're still out of bounds
            if(Physics.Raycast(ballRbody.position, Vector3.down, out RaycastHit hit, 0.2f))
            {
                Debug.DrawRay(ballRbody.position, Vector3.down, Color.white);
                if (hit.collider.CompareTag("Environment")) isBallOutOfBounds = true;
                else isBallOutOfBounds = false;
            }
        }

        if (stopCheckInProg)
        {
            //Raycast downward to check for any lateral movement
            if (Physics.Raycast(ballRbody.position, Vector3.down, out RaycastHit hit, 0.2f))
            {
                Debug.DrawRay(ballRbody.position, Vector3.down, Color.white);
                stopCheckPos = hit.point;
            }
        }

        if (wallCheckInProg)
        {
            //Raycast downward to check if we're still on the wall
            if (Physics.Raycast(ballRbody.position, Vector3.down, out RaycastHit hit, 0.2f))
            {
                if (hit.collider.CompareTag("HoleWalls")) isBallOnWall = true;
                else isBallOnWall = false;
            }
        }

        if (propCheckInProg)
        {
            //Raycast downward to check if we're still on the prop
            if (Physics.Raycast(ballRbody.position, Vector3.down, out RaycastHit hit, 0.2f))
            {
                if (hit.collider.CompareTag("TileProp")) isBallOnProp = true;
                else isBallOnProp = false;
            }
        }

    }

    //Start the player's shot on mouse down
    public void StartShot()
    {
        if (isBallGrounded && isBallStopped && isShotReady)
        {
            isMouseDown = true;

            //Cast a ray through the mouse position to the shot plane's collider
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (shotPlane.Raycast(ray, out RaycastHit hit, 150f))
            {
                //Reset LineRenderer points
                shotLine.positionCount = 0;
                shotLine.positionCount = 2;

                //Update and enable shot UI
                shotLine.SetPosition(0, ballRbody.position);
                shotLine.enabled = true;
                shotCircle.enabled = true;
                shotCircle.color = new Color(shotCircle.color.r, shotCircle.color.g, shotCircle.color.b, 0.2f);
                shotPower.enabled = true;
            }
        }
    }

    //Take the player's shot on mouse release
    public void TakeShot()
    {
        if(isBallGrounded && isBallStopped && isMouseDown && isShotReady)
        {
            //Cast a ray through the mouse's position upon release
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if(shotPlane.Raycast(ray, out RaycastHit hit, 150f))
            {
                mouseReleasePos = hit.point;
            }

            //Create the direction vector using the release position and the ball's position and normalize it
            Vector3 shotDir = new Vector3(ballRbody.position.x - mouseReleasePos.x, 0, ballRbody.position.z - mouseReleasePos.z);
            Vector3 normShotDir = Vector3.Normalize(shotDir);

            //Set gravity back to its normal value once shot has been taken
            gravity = -10f;
            
            //Add the respective force to the ball.
            ballRbody.AddForce(Vector3.ClampMagnitude(normShotDir * (shotDir.magnitude * shotMulti), clampFactor) , ForceMode.Impulse);
            Debug.Log("Hitting the ball with " + Vector3.ClampMagnitude(normShotDir * (shotDir.magnitude * shotMulti), clampFactor).magnitude + " force");

            isHopAvailable = true;

            //Clear user's input and update the stroke values in game data.
            ClearInput();
            gameData.currentStrokes++;
            gameData.totalStrokes++;
            gameCont.UpdateGameUI();
        }
    }

    //Clear any current input when user cancels with right click or takes a shot
    public void ClearInput()
    {
        if (isMouseDown)
        {
            //Hide UI and clear original click position on right click
            isMouseDown = false;
            mouseReleasePos = Vector3.zero;
            shotLine.enabled = false;
            shotCircle.enabled = false;
            shotPower.enabled = false;
        }
    }

    //Applies upward force to the ball to hop over obstacles at the cost of a stroke, can be used once per shot
    public void HopBall()
    {
        if (isHopAvailable)
        {
            isHopAvailable = false;
            ballRbody.AddForce(Vector3.up * 7.5f, ForceMode.Impulse);
            gameData.currentStrokes++;
            gameData.totalStrokes++;
            gameCont.UpdateGameUI();
        }
    }

    //Toggle the ball's light on/off and change the ball's material to its emissive counterpart on key press
    public void ToggleBallLight()
    {
        if (ballLight.enabled)
        {
            ballLight.enabled = false;
            ballRenderer.material = ballMat;
        }
        else
        {
            ballLight.enabled = true;
            ballRenderer.material = ballEmissiveMat;
        }

    }

    //Check if the ball is still stopped after a given time value
    private IEnumerator StopCheck(float timeToWait)
    {
        stopCheckInProg = true;
        Physics.Raycast(ballRbody.position, Vector3.down, out RaycastHit hit, 0.15f);

        yield return new WaitForSeconds(timeToWait);

        if (Vector3.Distance(hit.point, stopCheckPos) < 0.01f)
        {
            prevShotPos = ballRbody.position;
            isBallStopped = true;
            isShotReady = true;
            isHopAvailable = false;
        }

        stopCheckInProg = false;
        yield return null;
    }

    //Check if the ball is stopped on any objects tagged environment after a given time value
    private IEnumerator EnvironmentCheck(float timeToWait)
    {
        envCheckInProg = true;

        yield return new WaitForSeconds(timeToWait);

        if (isBallOutOfBounds)
        {
            Debug.Log("Respawning out of bounds ball.");
            RespawnBall(true, prevShotPos);
            isBallOutOfBounds = false;
        }

        envCheckInProg = false;
        yield return null;
    }

    //Check if the ball is stopped on the hole's walls after a given time value
    private IEnumerator WallCheck(float timeToWait)
    {
        wallCheckInProg = true;

        yield return new WaitForSeconds(timeToWait);

        if (isBallOnWall)
        {
            RespawnBall(true, prevShotPos);
            isBallOnWall = false;
        }

        wallCheckInProg = false;
        yield return null;
    }

    //Check if the ball is stopped on any of the hole's spawned props after a given time value
    private IEnumerator PropCheck(float timeToWait)
    {
        propCheckInProg = true;

        yield return new WaitForSeconds(timeToWait);

        if (isBallOnProp)
        {
            RespawnBall(true, prevShotPos);
            isBallOnProp = false;
        }

        propCheckInProg = false;
        yield return null;
    }

    //Ball velocity clamping at low speeds to prevent infinite rolling or physics jitter
    private void ClampVelocity()
    {
        //Set to softer values to combat jitter
        gravity = -1f;
        Physics.bounceThreshold = .3f;

        //Stop the ball
        ballRbody.velocity = Vector3.zero;
        ballRbody.angularVelocity = Vector3.zero;
    }

    //Respawn the ball to the passed position (with or without stroke penalty), defaults to the ball spawn if user hasn't taken a shot yet
    public void RespawnBall(bool penalty, Vector3 position)
    {
        ballTrail.Clear();
        ballRbody.velocity = Vector3.zero;
        ballRbody.angularVelocity = Vector3.zero;

        if(gameData.currentStrokes == 0)
        {
            ballRbody.position = ballSpawnPos;
            penalty = false;
        } else
        {
            ballRbody.position = position;
        }

        gravity = -10f;

        if (penalty)
        {
            gameData.currentStrokes++;
            gameData.totalStrokes++;
            gameCont.UpdateGameUI();
        }
    }

    //Updates the stored ball spawn position, called if we move the spawn point on hole generation or collision
    public void UpdateBallSpawnPos()
    {
        ballSpawnPos = GameObject.FindGameObjectWithTag("BallSpawn").transform.position;
    }

    //Stop ball velocity and disable gravity
    public void PauseBallActivity()
    {
        ballRbody.velocity = Vector3.zero;
        ballRbody.angularVelocity = Vector3.zero;
        gravity = 0f;
    }

    //Grounding and collision checks
    private void OnCollisionEnter(Collision collision)
    {
        //Define the ball as grounded when colliding with the hole object
        if (collision.gameObject.CompareTag("HoleFloor"))
        {
            isBallGrounded = true;
        }

        //Start the WallCheck coroutine if one is not in progress and the ball is stopped or at a very slow velocity while colliding with the walls
        if (collision.gameObject.CompareTag("HoleWalls") && !wallCheckInProg && ballRbody.velocity.magnitude < 0.5f && !isBallInHole)
        {
            if(Physics.Raycast(ballRbody.position, Vector3.down, out RaycastHit hit, 0.15f))
            {
                if (hit.collider.CompareTag("HoleWalls"))
                {
                    isBallOnWall = true;
                    StartCoroutine(WallCheck(1.5f));
                }
            }
        }

        //Start the PropCheck coroutine if one is not in progress and the ball is stopped or at a very slow velocity while colliding with a prop
        if (collision.gameObject.CompareTag("TileProp") && !propCheckInProg && ballRbody.velocity.magnitude < 0.5f)
        {
            if (Physics.Raycast(ballRbody.position, Vector3.down, out RaycastHit hit, 0.15f))
            {
                if (hit.collider.CompareTag("TileProp"))
                {
                    isBallOnProp = true;
                    StartCoroutine(PropCheck(1.5f));
                }
            }
        }

        //Start the EnvironmentCheck coroutine if one is not in progress and the ball is colliding with any objects tagged as environment
        if (collision.gameObject.CompareTag("Environment") && !envCheckInProg)
        {
            isBallOutOfBounds = true;
            StartCoroutine(EnvironmentCheck(2.5f));
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        //Define the ball as ungrounded when no longer colliding with the hole object
        if (collision.gameObject.CompareTag("HoleFloor"))
        {
            isBallGrounded = false;
        }
    }
}
