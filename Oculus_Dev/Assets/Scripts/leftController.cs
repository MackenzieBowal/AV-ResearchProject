using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class leftController : MonoBehaviour
{

    public GameObject camera;
    public const float WALK_SPEED = 1f;
    private GameObject menu;
    // Update is called once per frame
    void Start()
    {
        menu = GameObject.Find("Menu");
    }
    void Update()
    {
        //moving Controllers
        Vector2 input = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        Vector3 movement = camera.transform.TransformDirection(input.x,0,input.y);
        movement.y = 0;
        movement = movement.magnitude == 0 ? Vector3.zero : movement / movement.magnitude;
        movement *= Time.deltaTime * WALK_SPEED;
        this.transform.Translate(movement);

        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            Debug.Log("SccondHandTrigger pushed");
            menu.SetActive(true);
        }

    }
}
