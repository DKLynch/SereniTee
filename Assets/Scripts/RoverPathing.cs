using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RoverPathing : MonoBehaviour
{
    public List<Transform> waypoints;
    public float seconds;
    public List<Vector3> positions;
    public Rigidbody rB;

    void Awake()
    {
        rB = this.GetComponent<Rigidbody>();

        foreach(Transform w in waypoints)
        {
            positions.Add(w.position);
        }
    }

    void Start()
    {
        bool reversed = (Random.value > 0.5f);
        if (reversed) positions.Reverse();
        var p = positions.ToArray();

        //Simply paths the rover through the set list of waypoints
        rB.DOPath(p, seconds, PathType.CatmullRom, PathMode.Full3D, 10, Color.cyan)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart)
            .SetLookAt(0.01f, Vector3.back, Vector3.up)
            .SetOptions(true);
    }
}
