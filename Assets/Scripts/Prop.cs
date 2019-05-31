using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : MonoBehaviour
{
    [Header("Materials")]
    public DefaultMats defMats;
    public bool overwriteMats;
    public bool switchSubmeshes;

    [Header("Mesh")]
    public MeshRenderer[] mRenderers;
    public bool randomRotation;

    void Awake()
    {
        defMats = FindObjectOfType<DefaultMats>();
    }

    void Start()
    {
        if (overwriteMats)
        {
            UpdateMats();
        }
    }

    private void UpdateMats()
    {
        List<Material> defMatsList = new List<Material>
        {
            defMats.defaultWalls,
            defMats.defaultFloors,
            defMats.fallbackMat
        };

        foreach(MeshRenderer mRenderer in mRenderers)
        {
            List<Material> tempMats = new List<Material>();

            for (int i = 0; i < mRenderer.materials.Length; i++)
            {
                tempMats.Add(defMatsList[i]);
            }

            if (switchSubmeshes) tempMats.Reverse();
            mRenderer.materials = tempMats.ToArray();
        }
    }
}
