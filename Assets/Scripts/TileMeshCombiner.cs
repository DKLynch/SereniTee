using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using B83.MeshHelper;

public class TileMeshCombiner : MonoBehaviour
{
    public void SplitTileSubmeshes()
    {
        var tilePool = GameObject.FindGameObjectWithTag("TilePool");
        var meshes = tilePool.GetComponentsInChildren<MeshFilter>();

        List<MeshFilter> tileMeshes = new List<MeshFilter>();

        //Rule out any meshes that don't belong to the tile, i.e props and accessories.
        foreach (MeshFilter m in meshes)
        {
            if (!m.CompareTag("TileProp"))
            {
                tileMeshes.Add(m);
            }
        }

        List<Mesh> newWallMeshes = new List<Mesh>();
        List<Mesh> newFloorMeshes = new List<Mesh>();

        for (int i = 0; i < tileMeshes.Count; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                Mesh tempMesh = CreateMesh(tileMeshes[i].sharedMesh, j);

                //Discern which submesh we're working with and add it to the respective list.
                if (j == 0)
                {
                    newWallMeshes.Add(tempMesh);

                } else if (j == 1)
                {
                    newFloorMeshes.Add(tempMesh);

                }
            }

        }

        List<MeshFilter> scaledWMeshes = new List<MeshFilter>();
        List<MeshFilter> scaledFMeshes = new List<MeshFilter>();

        //Assign mesh filters to temporary tiles in order to scale the meshes before combining
        for(int i = 0; i < newWallMeshes.Count; i++)
        {
            var tempTile = new GameObject("TempTile");

            MeshFilter wMeshFilter = tempTile.AddComponent(typeof(MeshFilter)) as MeshFilter;
            wMeshFilter.mesh = newWallMeshes[i];
            tempTile.transform.position = meshes[i].transform.position;
            tempTile.transform.rotation = meshes[i].transform.rotation;
            tempTile.transform.localScale *= 100;
            scaledWMeshes.Add(wMeshFilter);

            Destroy(tempTile);
        }

        for(int i = 0; i < newFloorMeshes.Count; i++)
        {
            var tempTile = new GameObject("TempTile");

            MeshFilter fMeshFilter = tempTile.AddComponent(typeof(MeshFilter)) as MeshFilter;
            fMeshFilter.mesh = newFloorMeshes[i];
            tempTile.transform.position = meshes[i].transform.position;
            tempTile.transform.rotation = meshes[i].transform.rotation;
            tempTile.transform.localScale *= 100;
            scaledFMeshes.Add(fMeshFilter);

            Destroy(tempTile);
        }

        CombineMeshInstances(scaledWMeshes, scaledFMeshes);

    }


    public void CombineMeshInstances(List<MeshFilter> wMeshes, List<MeshFilter> fMeshes)
    {
        CombineInstance[] wCombine = new CombineInstance[wMeshes.Count];
        CombineInstance[] fCombine = new CombineInstance[fMeshes.Count];

        for (int i = 0; i < wMeshes.Count; i++)
        {
            wCombine[i].mesh = wMeshes[i].sharedMesh;
            fCombine[i].mesh = fMeshes[i].sharedMesh;

            wCombine[i].transform = wMeshes[i].transform.localToWorldMatrix;
            fCombine[i].transform = fMeshes[i].transform.localToWorldMatrix;
        }

        CreateMergedCourse(wCombine, fCombine);
    }


    public GameObject CreateMergedCourse(CombineInstance[] wInstance, CombineInstance[] fInstance)
    {
        var defMats = GameObject.FindObjectOfType<DefaultMats>();
        var holeObj = new GameObject("Hole") { tag = "Hole", layer = 10 };
        var wallObj = new GameObject("Walls") { tag = "HoleWalls", layer = 10 };
        var floorObj = new GameObject("Floor") { tag = "HoleFloor", layer = 10 };
        List<GameObject> objs = new List<GameObject>() { wallObj, floorObj };

        foreach(GameObject obj in objs)
        {
            //Parent to the hole object
            obj.transform.parent = holeObj.transform;

            //Add Components
            //Mesh Filter
            MeshFilter mFilter = obj.AddComponent(typeof(MeshFilter)) as MeshFilter;
            mFilter.mesh = new Mesh();
            switch (obj.name)
            {
                case "Walls":
                    mFilter.mesh.CombineMeshes(wInstance);
                    break;
                case "Floor":
                    mFilter.mesh.CombineMeshes(fInstance);
                    break;
            }

            //Mesh Renderer
            MeshRenderer mRenderer = obj.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
            mRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

            //Rigidbody
            Rigidbody rBody = obj.AddComponent(typeof(Rigidbody)) as Rigidbody;
            rBody.isKinematic = true;
            rBody.useGravity = false;

            //Collider(s)
            MeshCollider collider = obj.AddComponent(typeof(MeshCollider)) as MeshCollider;
            MeshCollider trigger = obj.AddComponent(typeof(MeshCollider)) as MeshCollider;
            trigger.convex = true;
            trigger.isTrigger = true;

            //Collision Checks
            obj.AddComponent(typeof(EnvironmentCollisionCheck));

            //Set the relevant materials
            switch (obj.name)
            {
                case "Walls":
                    mRenderer.material = defMats.defaultWalls;
                    collider.material = defMats.defaultPhysWalls;
                    break;
                case "Floor":
                    mRenderer.material = defMats.defaultFloors;
                    collider.material = defMats.defaultPhysFloors;
                    break;
            }

            //Double vertice welding
            var welder = new MeshWelder(mFilter.mesh);
            welder.Weld();
            mFilter.mesh.RecalculateNormals();
        }
        return holeObj;
    }


    public Mesh CreateMesh(Mesh oldMesh, int subIndex)
    {
        Mesh newMesh = new Mesh();

        List<int> triangles = new List<int>();
        triangles.AddRange(oldMesh.GetTriangles(subIndex)); // the triangles of the sub mesh

        List<Vector3> newVertices = new List<Vector3>();

        // Mark's method. 
        Dictionary<int, int> oldToNewIndices = new Dictionary<int, int>();
        int newIndex = 0;

        // Collect the vertices
        for (int i = 0; i < oldMesh.vertices.Length; i++)
        {
            if (triangles.Contains(i))
            {
                newVertices.Add(oldMesh.vertices[i]);
                oldToNewIndices.Add(i, newIndex);
                ++newIndex;
            }
        }

        int[] newTriangles = new int[triangles.Count];

        // Collect the new triangles indecies
        for (int i = 0; i < newTriangles.Length; i++)
        {
            newTriangles[i] = oldToNewIndices[triangles[i]];
        }
        // Assemble the new mesh with the new vertices/uv/triangles.
        newMesh.vertices = newVertices.ToArray();
        newMesh.triangles = newTriangles;

        // Re-calculate bounds and normals for the renderer.
        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();

        return newMesh;
    }
}
