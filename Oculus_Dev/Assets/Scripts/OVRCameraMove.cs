using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OVRCameraMove : MonoBehaviour
{
    private Vector3 OriginPos = new Vector3(-319.5f, 71.4f, 0.72f); 
    public void ResetPos()
    {
        this.transform.position = OriginPos;
    }
}
