using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Planet : MonoBehaviour
{
    //Randomization
    [SerializeField] public NestedArray[] matsOptions;
    [SerializeField] public bool randomizeColor;
    [SerializeField] public bool randomizeSize;
    [SerializeField][Range(0, 1)] public float maxSizeVariation;

    //Orbiting
    [SerializeField] public bool orbit;
    [SerializeField] public float orbitSpeed;
    [SerializeField] public Transform orbitPoint;

    //Rotation
    [SerializeField] public bool rotate;
    [SerializeField] public float rotationSpeed;
    [SerializeField] public Vector3 rotationAngle;

    //Nested Array Class to allow serialization of 2D Array of Material Combinations
    [System.Serializable]
    public class NestedArray
    {
        public Material[] matsArr;
    }

    private void Awake()
    {
        if (randomizeColor) SelectNewColors();
        if (randomizeSize) SelectNewSize();
    }

    private void FixedUpdate()
    {
        if (orbit)
        {
            AdvanceOrbit();
        }

        RotatePlanet();
    }

    private void RotatePlanet()
    {
        this.transform.Rotate(rotationAngle * (rotationSpeed * Time.deltaTime));
    }

    private void AdvanceOrbit()
    {
        this.transform.RotateAround(orbitPoint.position, Vector3.up, orbitSpeed * Time.deltaTime);
    }

    private void SelectNewColors()
    {
        var rend = this.GetComponent<MeshRenderer>();
        int index = Random.Range(0, matsOptions.Length);
        var newMats = matsOptions[index].matsArr;
        rend.materials = newMats;
    }

    private void SelectNewSize()
    {
        float scaleFactor = Random.Range(0, maxSizeVariation);
        bool invert = Random.value > 0.5;

        if (invert) {
            scaleFactor *= -1;
        } 
        else
        {
            scaleFactor *= 1.5f;
        }

        this.transform.localScale += this.transform.localScale * scaleFactor;
    }

}
