using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkBoundary : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Vector3[] boundaryPoints = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.OuterBoundary);
        Debug.Log("#####start getting boundary");
        Debug.Log(boundaryPoints[0]);
        /*foreach ( Vector3 pos in boundaryPoints)
        {
            Debug.Log("#####each boundary value");
            Debug.Log(pos.x + ", " + pos.y + ", " + pos.z);
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
