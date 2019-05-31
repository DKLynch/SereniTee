using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Tile : MonoBehaviour
{
    [Header("Props")]
    public bool canSpawnProps;
    public List<Transform> propPosParents;
    public List<List<Transform>> propPositions = new List<List<Transform>>();
    public List<Transform> propsPrefabs;
    public float propChance;

    [Header("Misc")]
    public string[] tags;
    public int tileWeight;

    public TileConnector[] GetConnections()
    {
        return GetComponentsInChildren<TileConnector>();
    }

    public void Awake()
    {
        var gameController = GameObject.FindGameObjectWithTag("GameMaster");
        var defMats = gameController.GetComponent<DefaultMats>();

        //Sets the tile's materials to the theme's defaults
        var mats = GetComponent<MeshRenderer>().materials;
        mats[0] = defMats.defaultWalls;
        mats[1] = defMats.defaultFloors;
        this.GetComponent<MeshRenderer>().materials = mats;

        //Updates the shadow mode to alleviate light shinethrough on edges.
        this.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
    }

    //Spawns props randomly at the preset positions
    public void SpawnProps()
    {
        for (int i = 0; i < propPosParents.Count; i++)
        {
            propPositions.Add(new List<Transform>());
            var newPositions = propPosParents[i].GetComponentsInChildren<Transform>();
            foreach (Transform pos in newPositions)
            {
                if(pos.GetInstanceID() != propPosParents[i].GetInstanceID()) propPositions[i].Add(pos);
            }
        }

        int posIndex = Random.Range(0, propPositions.Count);
        foreach(Transform pos in propPositions[posIndex])
        {
            //Select and spawn random prop prefab
            int prefabIndex = Random.Range(0, propsPrefabs.Count);
            var rot = pos.rotation.eulerAngles;
            var p = Instantiate(propsPrefabs[prefabIndex], pos.position, Quaternion.Euler(0, rot.y, rot.z)); 
            var prop = p.GetComponent<Prop>();
            if (prop.randomRotation) p.rotation = Quaternion.Euler(0, Random.Range(0, 361), 0);
            p.parent = GameObject.FindGameObjectWithTag("PropPool").transform;
        }
    }

    //Removes the props and miscellaneous objects from the tile and parents them to their respective object pools
    public void DeparentPropsAndGameObjects()
    {
        var children = this.GetComponentsInChildren<Transform>();
        List<Transform> props = new List<Transform>();
        List<Transform> gameplayObjects = new List<Transform>(); 
        var propPool = GameObject.FindGameObjectWithTag("PropPool");
        var miscPool = GameObject.FindGameObjectWithTag("MiscPool");

        if (!propPool)
        {
            propPool = new GameObject("PropPool") { tag = "PropPool" };
        }

        if (!miscPool)
        {
            miscPool = new GameObject("MiscPool") { tag = "MiscPool" };
        }

        foreach (Transform c in children)
        {
            if (c.CompareTag("TileProp"))
            {
                props.Add(c);
            } else if (c.CompareTag("GameplayObject"))
            {
                gameplayObjects.Add(c);
            }
        }

        foreach(Transform p in props)
        {
            p.parent = propPool.transform;

            //Sets the object to the 'Hole' layer
            p.gameObject.layer = 10;
        }

        foreach(Transform g in gameplayObjects)
        {
            g.parent = miscPool.transform;

            //Sets the object to the 'Hole' layer
            g.gameObject.layer = 10;
        }
    }
}
