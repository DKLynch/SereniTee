using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    //Speed
    public bool randomizeSpeed;
    public float speed;

    //Colour & Appearance
    public bool randomizeColour;
    public int matIndex;
    public bool enableTrail;
    public TrailRenderer trail;

    //Travel
    public List<Vector3> startPoints;
    public List<Vector3> endPoints;
    private bool active;
    private Vector3 originalPosition;
    private int laneIndex;
    

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = this.transform.position;
        StartCoroutine(StartSpawnCountdown());
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            float step = speed * Time.deltaTime;
            this.transform.position = Vector3.MoveTowards(this.transform.position, endPoints[laneIndex], step);
        }

        if (this.transform.position == endPoints[laneIndex])
        {
            ReturnToPool();
        }
    }

    private void StartPath()
    {
        if (randomizeSpeed) SelectNewSpeed();
        if (randomizeColour) SelectNewColour();
        if (enableTrail) trail.enabled = true;

        laneIndex = Random.Range(0, startPoints.Count);
        this.transform.position = startPoints[laneIndex];
        this.transform.LookAt(endPoints[laneIndex]);

        active = true;
    }

    private void SelectNewSpeed()
    {
        speed = 6f;
        float maxSpeed = speed + speed / 4;
        float minSpeed = speed - speed / 4;

        speed = Random.Range(minSpeed, maxSpeed);
    }

    private void SelectNewColour()
    {
        var matToChange = this.GetComponent<MeshRenderer>().materials[matIndex];
        matToChange.color = Random.ColorHSV(0f, 1f, .25f, .5f, 0f, 1f);
    }

    private void ReturnToPool()
    {
        active = false;
        this.transform.position = originalPosition;
        StartCoroutine(StartSpawnCountdown());
    }

    private IEnumerator StartSpawnCountdown()
    {
        float r = Random.Range(15f, 60f);
        yield return new WaitForSeconds(r);
        StartPath();
    }
}
