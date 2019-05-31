using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileConnector : MonoBehaviour
{
    public bool isDefault;
    public bool isMatched;

    /*The tile connectors define where each tile can be connected to and from,
     * These gizmos show the directions and angle each connector sits at to prevent faulty connections*/
    private void OnDrawGizmos()
    {
        var scale = 0.45f;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * scale);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position - transform.right * scale);
        Gizmos.DrawLine(transform.position, transform.position + transform.right * scale);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * scale);

        Gizmos.color = Color.grey;
        Gizmos.DrawSphere(transform.position, 0.075f);
    }
}
