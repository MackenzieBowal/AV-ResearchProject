using UnityEngine;
using static OVRBoundary;

public class GuardianScripts : MonoBehaviour
{
    private Vector3[] boundaryPoints;
    private Vector3 dim;


    // Update is called once per frame
    void Update()
    {

    }

public Vector3[] getBoundaryInfo()
    {
            //Check if the boundary is configured
            bool configured = OVRManager.boundary.GetConfigured();
            if (configured)
            {
                //Grab all the boundary points. Setting BoundaryType to OuterBoundary is necessary
                boundaryPoints = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.OuterBoundary);
            // for (int i = 0; i < boundaryPoints.Length; i++)
            // {
            //     if (i % boardDistance == 0)
            //     {
            //         var newBoard = Instantiate(wallBoard, boundaryPoints[i], Quaternion.identity);

            //         Vector3 forward = Vector3.zero;
            //         if (i < boundaryPoints.Length - 1)
            //             forward = boundaryPoints[i] - boundaryPoints[i + 1];

            //         newBoard.transform.forward = forward;
            //     }
            // 
                dim = OVRManager.boundary.GetDimensions(OVRBoundary.BoundaryType.OuterBoundary);
            }
            return boundaryPoints;
            
    }
    

}

