using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameData : MonoBehaviour
{
    //GameData script is our persistent script that maintains our scores/pars and other variables across scenes.

    //Game Variables
    public string gameMode;
    public int noHoles = 0;
    public int scoreboardResets;

    //Current Hole Variables
    public int currentHoleNo = 0;
    public int currentHolePar = 0;
    public int currentStrokes = 0;

    //Total Variables
    public int totalPar = 0;
    public int totalStrokes = 0;

    //Course Lists
    public List<int> holeScores;
    public List<int> holePars;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void AddScore(int score)
    {
        holeScores.Add(score);
    }

    public void AddPar(int par)
    {
        holePars.Add(par);
    }
}
