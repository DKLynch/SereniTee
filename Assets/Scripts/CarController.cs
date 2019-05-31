using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] public bool randomizeColor;
    [SerializeField] public int paintMatIndex;
    [SerializeField] public MeshRenderer mRenderer;

    [Header("Lights")]
    [SerializeField] public bool useHeadlights;
    [SerializeField] public bool isPoliceCar;
    [SerializeField] public Light[] headLights;
    [SerializeField] public Light[] tailLights;
    [SerializeField] public CopCarLights copLights;

    [Header("Car Activity")]
    [SerializeField] public bool isCarActive;
    [SerializeField] public bool isCarOvertaking,
        isCarYielding,
        carInFront,
        carBehind,
        hasYielded,
        hasOvertaken,
        countdownInProg;
    [SerializeField] public int currentLane;
    [SerializeField] public float speed = 10f;
    [SerializeField] public float overtakeDist = 4f,
        currentLaneMinSpeed,
        currentLaneMaxSpeed;
    [SerializeField] public Vector3 inactivePos;
    [SerializeField] public Quaternion inactiveRot;
    [SerializeField] public CarController lastCarHit;

    [Header("Raycasts")]
    [SerializeField] public Transform frontPoint;
    [SerializeField] public Transform centrePoint,
        rearPoint;

    [Header("Road")]
    [SerializeField] public Transform[] laneStarts;
    [SerializeField] public Transform[] laneEnds;

    private void Awake()
    {
        inactivePos = transform.position;
        inactiveRot = transform.rotation;
    }

    void Start()
    {
        if (isPoliceCar) copLights = GetComponent<CopCarLights>();
        InitiateSpawnCountdown();
    }

    void Update()
    {
        if (isCarActive)
        {
            MoveCar();
            ForwardRaycast();

            if (carInFront)
            {
                //Overtake Raycasting
                //Still to be implemented
            }

            if (carBehind)
            {
                //Yield Raycasting
                //Still to be implemented
            }

            //Reset when the car has reached the end of the lane
            if (Vector3.Distance(transform.position, laneEnds[currentLane].position) < 0.5f) DeactivateCar();
        }
    }

    //Spawn the car on the road after a random delay
    private void InitiateSpawnCountdown()
    {
        if (!countdownInProg)
        {
            countdownInProg = true;
            float t = Random.Range(0f, 15f);
            Invoke("ActivateCar", t);
        }
    }

    //Spawns our car 
    private void ActivateCar()
    {
        if (randomizeColor) RandomizeCarColor();
        if (isPoliceCar) copLights.ActivateLights();

        //Select a lane
        int l = Random.Range(0, 4);
        currentLane = l;

        //Move the car to the start of the selected lane, and rotate it to face the end point of the lane
        transform.position = laneStarts[currentLane].position;
        var rawTargetRot = Quaternion.LookRotation(laneEnds[currentLane].position).eulerAngles;
        var adjTargetRot = Quaternion.Euler(0, rawTargetRot.y, 0);
        transform.rotation = adjTargetRot;

        RandomizeSpeed(.15f);

        isCarActive = true;
        countdownInProg = false;
    }

    //Deactivates the car and moves it back to its position in the pool
    private void DeactivateCar()
    {
        isCarActive = false;
        ResetBooleans();
        transform.position = inactivePos;
        transform.rotation = inactiveRot;
        if (isPoliceCar) copLights.DeactivateLights();
        InitiateSpawnCountdown();
    }

    //Moves the car from point A to point B
    private void MoveCar()
    {
        float step = speed * Time.deltaTime;
        this.transform.position = Vector3.MoveTowards(this.transform.position, laneEnds[currentLane].position, step);
    }

    //Selects a random speed within the current lane's parameters
    private void RandomizeSpeed(float overSeconds)
    {
        //Determine which lane and set min/max accordingly
        switch (currentLane)
        {
            case 0:
                currentLaneMinSpeed = 28f;
                currentLaneMaxSpeed = 35f;
                break;
            case 1:
                currentLaneMinSpeed = 23f;
                currentLaneMaxSpeed = 27.5f;
                break;
            case 2:
                currentLaneMinSpeed = 23f;
                currentLaneMaxSpeed = 27.5f;
                break;
            case 3:
                currentLaneMinSpeed = 28f;
                currentLaneMaxSpeed = 35f;
                break;
        }

        //Randomize between the min/max
        var s = Random.Range(currentLaneMinSpeed, currentLaneMaxSpeed);
        StartCoroutine(LerpSpeed(speed, s, overSeconds));
    }

    //Limit our speedto the car in front if our forward raycast hits another car
    private void LimitSpeed(RaycastHit hit)
    {
        var carHit = hit.transform.GetComponentInParent<CarController>();
        if (carHit != null)
        {
            carHit.carBehind = true;
            lastCarHit = carHit;

            //If we're really close, we slow down faster to try and avoid collisions
            if (hit.distance <= 3f) StartCoroutine(LerpSpeed(speed, carHit.speed - 2.5f, .1f));
            else StartCoroutine(LerpSpeed(speed, carHit.speed - 1.5f, .4f));

        } else
        {
            Debug.Log("We hit: " + hit.transform.name);
        }
    }

    //Lerps our speed from a given speed to another target speed over time
    private IEnumerator LerpSpeed(float fromSpeed, float toSpeed, float time)
    {
        //Activate our taillights if we're moving from a faster speed to a slower one
        if (toSpeed < fromSpeed) ToggleTaillights(true);

        //Gradually increase or decrease our speed
        for (float t = 0f; t <= 1f; t += Time.deltaTime / time)
        {
            speed = Mathf.Lerp(fromSpeed, toSpeed, t);
            yield return null;
        }

        //Snap our speed to the target speed and turn off our taillights 
        speed = toSpeed;
        ToggleTaillights(false);
    }

    //Forward raycasting from the car to detect impending collisions
    private void ForwardRaycast()
    {
        //Constant raycasts to detect for impending collisions
        RaycastHit frontHit;
        if (Physics.Raycast(frontPoint.position, frontPoint.TransformDirection(Vector3.forward), out frontHit, overtakeDist))
        {
            //Store that we've caught up to another car 
            carInFront = true;

            //Draw the ray that hit
            Debug.DrawRay(frontPoint.position, frontPoint.TransformDirection(Vector3.forward) * frontHit.distance, Color.white);

            //Slow down to the car hit's speed to prevent collision
            LimitSpeed(frontHit);
        }
        else
        {
            //Draw the ray 
            Debug.DrawRay(frontPoint.position, frontPoint.TransformDirection(Vector3.forward) * overtakeDist, Color.cyan);
        }
    }

    private void ToggleHeadlights(bool turnOn)
    {
        if (turnOn)
        {
            foreach (Light l in headLights) l.enabled = true;
        }
        else if (!turnOn)
        {
            foreach (Light l in headLights) l.enabled = false;
        }
    }

    private void ToggleTaillights(bool turnOn)
    {
        if (turnOn)
        {
            foreach (Light l in tailLights) l.enabled = true;
        }
        else if (!turnOn)
        {
            foreach (Light l in tailLights) l.enabled = false;
        }
    }

    private void RandomizeCarColor()
    {
        mRenderer.materials[paintMatIndex].color = Random.ColorHSV(0f, 1f, 1f, 1f, 0f, 1f);
    }

    private void ResetBooleans()
    {
        carInFront = false;
        carBehind = false;
        isCarYielding = false;
        isCarOvertaking = false;
        hasYielded = false;
    }

    //Worst case collision detection
    private void OnTriggerEnter(Collider other)
    {
        //Deactivate the car if we collide with another
        if (other.CompareTag("Car"))
        {
            DeactivateCar();
        }
    }
}
