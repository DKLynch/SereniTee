using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class HoleGen : MonoBehaviour
{
    [Header("Tiles")]
    [SerializeField] public Tile[] tiles;
    [SerializeField] public Tile[] holeTiles;
    [SerializeField] public Tile tee;
    [HideInInspector] public List<Tile> weightedTiles, weightedHoleTiles;
    [HideInInspector] public List<Vector3> tilePositions;
    private Tile newTilePrefab;
    private int holeLength;
    private GameObject gameMaster;
    private GameData gameData;

    void Awake()
    {
        ApplyTileWeights();
        gameMaster = GameObject.FindGameObjectWithTag("GameMaster");
    }

    void Start()
    {
        gameData = FindObjectOfType<GameData>();
    }

    /*Generates a new hole and combines the individual tiles into
     *wall and floor submeshes before removing the pool of tiles*/
    public IEnumerator GenerateHole()
    {
        yield return new WaitForSeconds(.05f);

        //Create tile and prop pools.
        var tilePool = GameObject.FindGameObjectWithTag("TilePool");
        if (!tilePool)
        {
            tilePool = new GameObject("TilePool")
            {
                tag = "TilePool"
            };
        }

        var propPool = GameObject.FindGameObjectWithTag("PropPool");
        if (!propPool)
        {
            propPool = new GameObject("PropPool")
            {
                tag = "PropPool"
            };
        }

        var miscPool = GameObject.FindGameObjectWithTag("MiscPool");
        if (!miscPool)
        {
            miscPool = new GameObject("MiscPool")
            {
                tag = "MiscPool"
            };
        }

        //Randomize the length of the hole.
        holeLength = Random.Range(6, 12);
        var placedTiles = new List<Tile>();

        //Select a random rotation for the tee
        int r = Random.Range(0, 4);
        Quaternion rot = new Quaternion();

        switch (r)
        {
            case 0:
                rot = Quaternion.Euler(-90, 0, 0);
                break;
            case 1:
                rot = Quaternion.Euler(-90, 90, 0);
                break;
            case 2:
                rot = Quaternion.Euler(-90, 180, 0);
                break;
            case 3:
                rot = Quaternion.Euler(-90, 270, 0);
                break;
        }

        //Instantiate the tee.
        var startTile = Instantiate(tee, transform.position, rot);
        startTile.transform.parent = tilePool.transform;
        yield return new WaitForSeconds(.02f);
        var openCon = startTile.GetComponentInChildren<TileConnector>();
        placedTiles.Add(startTile);

        yield return new WaitForSeconds(.02f);

        //Hole Body Generation Loop
        do
        {
            var tileColliders = tilePool.GetComponentsInChildren<Collider>();
            //Select Prefab
            if(placedTiles.Count == holeLength - 1)
            {
                newTilePrefab = GetRandom(weightedHoleTiles.ToArray());
            } else
            {
                newTilePrefab = GetRandomExcludingTag(weightedTiles.ToArray(), "Split");
            }

            //Instantiate Tile
            var newTile = Instantiate(newTilePrefab);
            newTile.transform.parent = tilePool.transform;
            var newTileCons = newTile.GetConnections();
            var conToMatch = newTileCons.FirstOrDefault(x => x.isDefault) ?? newTileCons[Random.Range(0, newTileCons.Length)];
            placedTiles.Add(newTile);
            MatchConnections(openCon, conToMatch);
            var newConnection = openCon;

            //Detect the new tile's connection only if the tile is not the last in the generation loop, i.e the hole tile.
            if(placedTiles.Count != holeLength)
            {
                newConnection = newTileCons.First(x => x != conToMatch);
            }

            //Check for tile collisions
            yield return new WaitForSeconds(.02f);
            bool collision = CheckForTileConflicts(tileColliders, newTile.GetComponent<Collider>());
            yield return new WaitForSeconds(.02f);

            if (!collision)
            {
                //Continue as normal if tile is valid.
                openCon = newConnection;
            } else if (collision)
            {
                //Get the open end connector of the 3rd tile back and then destroy all tiles past that point.
                for (int i = 4; i > 0; i--)
                {
                    if (placedTiles.Count > i)
                    {
                        openCon = placedTiles[placedTiles.Count - i].GetConnections().First(x => !x.isMatched);
                        for (int j = 1; j < i; j++)
                        {
                            if (placedTiles[placedTiles.Count - j] != null)
                            {
                                Destroy(placedTiles[placedTiles.Count - j].gameObject);
                            }
                        }

                        placedTiles.RemoveRange(placedTiles.Count - (i - 1), i - 1);

                        break;
                    }
                }
            }

        } while (placedTiles.Count < holeLength);


        //Spawn & Deparent the tile props and miscellaneous objects plus store the tile positions for use in camera flythroughs.
        tilePositions.Clear();
        foreach(Tile t in placedTiles)
        {
            if (t.canSpawnProps)
            {
                bool spawn = Random.value > (1 - t.propChance);
                if (spawn) t.SpawnProps();
            }

            t.DeparentPropsAndGameObjects();
            tilePositions.Add(t.transform.position);
        }

        yield return new WaitForSeconds(.02f);

        //Calculate par for the current hole
        float par = .5f;
        for(int i = 0; i < tilePositions.Count - 2; i++)
        {
            float dist = Vector3.Distance(tilePositions[i], tilePositions[i + 1]) * 1.1f;
            par += dist;
        }

        int finalPar = Mathf.RoundToInt(par / (holeLength * .45f));
        Debug.Log("Hole Par: " + finalPar);

        //Update the game data/stats
        gameData.currentHoleNo++;
        gameData.currentHolePar = finalPar;
        gameData.currentStrokes = 0;
        gameData.totalPar += finalPar;
        gameData.AddPar(finalPar);

        //Split and combine the tile submeshes into a single 'Hole' object followed by removing the original tiles.
        gameMaster.GetComponent<TileMeshCombiner>().SplitTileSubmeshes();
        Invoke("RemoveTilePool", .25f);

        //Fetch Hole Object and center it 
        var holeObj = GameObject.FindGameObjectWithTag("Hole");
        var wallObj = holeObj.transform.Find("Walls");
        var floorObj = holeObj.transform.Find("Floor");
        var floorMeshR = floorObj.GetComponent<MeshRenderer>();

        //Parent the miscellanous object pools to the hole object to avoid a mismatch after moving.
        var ballSpawn = GameObject.FindGameObjectWithTag("BallSpawn");
        ballSpawn.transform.parent = holeObj.transform;
        propPool.transform.parent = holeObj.transform;
        miscPool.transform.parent = holeObj.transform;

        //Calculate the distance between the center of the hole's floor mesh and the hole object's pivot and shift the hole object accordingly.
        Vector3 posCorrect = floorMeshR.bounds.center - holeObj.transform.position;
        holeObj.transform.position -= posCorrect;
        for(int i = 0; i < tilePositions.Count; i++)
        {
            tilePositions[i] -= posCorrect;
        }
    }

    //Remove the tile pool.
    public void RemoveTilePool()
    {
        var tilePool = GameObject.FindGameObjectWithTag("TilePool");

        if (tilePool)
        {
            Destroy(tilePool);
        } else
        {
            Debug.Log("No tile pool exists to be destroyed.");
        }
    }

    /*Reconstruct the tile lists, repeating each tile by its own designated weight. 
    Very crude implementation of weighted randomization, but it works for now.*/
    private void ApplyTileWeights()
    {
        foreach (Tile t in tiles)
        {
            for (int i = 0; i < t.tileWeight; i++)
            {
                weightedTiles.Add(t);
            }
        }

        foreach (Tile t in holeTiles)
        {
            for (int i = 0; i < t.tileWeight; i++)
            {
                weightedHoleTiles.Add(t);
            }
        }
    }

    //Compare each tile's collider against the colliders of the tiles already placed.
    private bool CheckForTileConflicts(Collider[] tileColliders, Collider tileCol)
    {
        bool collision = false;

        foreach (Collider col in tileColliders)
        {
            if (col != null)
            {
                if (tileCol.bounds.Intersects(col.bounds))
                {
                    //Collision detected
                    Debug.Log("Collision detected between " + tileCol.name + " & " + col.name);
                    collision = true;
                }
            }
        }
        return collision;
    }

    //Tile prefab selection methods
    private static Tile GetRandom(Tile[] tiles)
    {
        return tiles[Random.Range(0, tiles.Length)];
    }

    private static Tile GetRandomExcludingTag(IEnumerable<Tile> tiles, string tag)
    {
        var matchingTiles = tiles.Where(m => !m.tags.Contains(tag)).ToArray();
        return matchingTiles[Random.Range(0, matchingTiles.Length)];
    }

    private static Tile GetRandomTagged(IEnumerable<Tile> tiles, string tag) 
    {
        var matchingTiles = tiles.Where(m => m.tags.Contains(tag)).ToArray();
        return matchingTiles[Random.Range(0, matchingTiles.Length)];
    }

    //Calculate the rotation and translation required to match both tile connectors and apply to the latter tile.
    private void MatchConnections(TileConnector oldCon, TileConnector newCon)
    {
        var newTile = newCon.transform.parent;
        var forwardVectorToMatch = -oldCon.transform.forward;
        var correctiveRot = Azimuth(forwardVectorToMatch) - Azimuth(newCon.transform.forward);
        newTile.RotateAround(newCon.transform.position, Vector3.up, correctiveRot);
        var correctiveTranslation = oldCon.transform.position - newCon.transform.position;
        newTile.transform.position += correctiveTranslation;
        newCon.isMatched = true;
    }

    private static float Azimuth(Vector3 vector)
    {
        return Vector3.Angle(Vector3.forward, vector) * Mathf.Sign(vector.x);
    }

}
