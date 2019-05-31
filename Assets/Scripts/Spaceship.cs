using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spaceship : MonoBehaviour
{
    //Speed
    public bool randomizeSpeed;
    public float speed;

    //Colours
    public bool randomizeColor;
    public bool randomizeTrailColor;
    public int matIndex;
    public List<TrailRenderer> trails;
    public List<Material> trailMats;

    //Travel
    private bool flying;
    private Vector3 pointA,
        pointB,
        travelVector,
        step,
        poolPosition;

    // Start is called before the first frame update
    void Start()
    {
        poolPosition = this.transform.position;
        StartCoroutine(StartSpawnCountdown(Random.Range(3f, 15f)));
    }

    // Update is called once per frame
    void Update()
    {
        if (flying)
        {
            step = this.transform.position + (travelVector * speed);
            this.transform.position = step;
        }
    }

    private void StartFlyBy()
    {
        if (randomizeSpeed) SelectNewSpeed();
        if (randomizeColor) SelectNewColor();
        if (randomizeTrailColor) SelectNewTrailColor();

        //Select two random points within a radius around the center of the scene.
        Vector3 vertOffset = new Vector3(0, -150, 0);
        pointA = (Random.insideUnitSphere * 1000) + vertOffset;
        pointB = (Random.insideUnitSphere * 250) + vertOffset;
    
        //Move the ship to the first point and rotate it to face the second.
        this.transform.position = pointA;
        ActivateTrail();
        this.transform.LookAt(pointB, Vector3.up);
        this.transform.Rotate(-90, 0, 0);

        //Get the Vector of the two points (Destination - Start, normalized for speed multiplication later).
        travelVector = Vector3.Normalize(pointB - pointA);

        flying = true;
        StartCoroutine(StartDespawnTimer(8f));
    }

    private void SelectNewSpeed()
    {
        speed = 6f;
        float maxSpeed = speed + speed / 4;
        float minSpeed = speed - speed / 4;

        speed = Random.Range(minSpeed, maxSpeed);
    }

    //Randomize the colour of the ship's primary material
    private void SelectNewColor()
    {
        var matToChange = this.GetComponent<MeshRenderer>().materials[matIndex];
        matToChange.color = Random.ColorHSV(0f, 1f, .25f, .5f, 0f, 1f);
    }

    //Randomize the trail's colour from a list of presets
    private void SelectNewTrailColor()
    {
        int r = Random.Range(0, trailMats.Count);
        foreach(TrailRenderer trail in trails)
        {
            trail.material = trailMats[r];
        }
    }

    //Deactivate and return the ship to its initial position
    private void ReturnToPool()
    {
        flying = false;
        this.transform.position = poolPosition;
        StartCoroutine(StartSpawnCountdown(Random.Range(3f, 12f)));
    }

    private void DeactivateTrail()
    {
        foreach (TrailRenderer trail in trails)
        {
            trail.enabled = false;
        }
    }

    private void ActivateTrail()
    {
        foreach (TrailRenderer trail in trails)
        {
            trail.Clear();
            trail.enabled = true;
        }
    }

    public IEnumerator StartDespawnTimer(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        DeactivateTrail();
        yield return new WaitForSeconds(.25f);
        ReturnToPool();
    }

    public IEnumerator StartSpawnCountdown(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        StartFlyBy();
        yield return new WaitForSeconds(.25f);
        ActivateTrail();
    }
}
