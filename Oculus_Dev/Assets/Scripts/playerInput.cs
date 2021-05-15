using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerInput : MonoBehaviour
{

    public float Acceleration
    {
        get {return m_Acceleration;}
    }

    public float Steering
    {
        get {return m_Steering;}
    }

    float m_Acceleration;
    float m_Steering;

    // bool m_FixedUpdaredHappened;

    // private bool accelerating = false;
    // private bool breaking = false;
    // private bool turningLeft = false;
    // private bool turningRight = false;

     public float wheelDampening;
    // // Update is called once per frame
    // void Update()
    // {
    //    GetPlayInput();
    //    if (accelerating)
    //    {
    //        m_Acceleration = 1f;
    //        wheelDampening = 500f;
    //    } 
    //    else if (breaking)
    //    {
    //        m_Acceleration = -1f;
    //        wheelDampening = 1000f;
    //    } 
    //    else
    //    {
    //        m_Acceleration = 0f;
    //        wheelDampening = 5f;
    //    } 

    //    if (turningLeft)
    //         m_Steering = -1f;
    //     else if (!turningLeft && turningRight)
    //         m_Steering = 1f;
    //     else
    //         m_Steering = 0f;
    // }

    // private void GetPlayInput()
    // {
    //     //whether accelerating
    //     if(OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickUp,OVRInput.Controller.RTouch))
    //     {
    //         accelerating = true;
    //     }    
    //     if(OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickUp,OVRInput.Controller.RTouch))
    //     {
    //         accelerating = false;
    //     }

    //     //whether breaking
    //     if(OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickDown,OVRInput.Controller.RTouch))
    //     {
    //         breaking = true;
    //     }    
    //     if(OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickDown,OVRInput.Controller.RTouch))
    //     {
    //         breaking = false;
    //     }

    //     //whether turining left
    //     if(OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft,OVRInput.Controller.RTouch))
    //     {
    //         turningLeft = true;
    //     }    
    //     if(OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickLeft,OVRInput.Controller.RTouch))
    //     {
    //         turningLeft = false;
    //     }

    //     //whether turining right
    //     if(OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickRight,OVRInput.Controller.RTouch))
    //     {
    //         turningRight = true;
    //     }    
    //     if(OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickRight,OVRInput.Controller.RTouch))
    //     {
    //         turningRight = false;
    //     }
    // }

    public bool SeeAround
     {
         get {return seeAround;}
     }
    bool m_FixedUpdaredHappened;

    private bool accelerating = false;
    private bool seeAround = false;
    private bool breaking = false;
    private bool turningLeft = false;
    private bool turningRight = false;

    // Update is called once per frame
    void Update()
    {
        OVRInput.Update();
        if(OVRInput.GetDown(OVRInput.Button.One))
        {
            seeAround = true;
        }   
    }

    private void GetPlayInput()
    {
        //whether accelerating
        if(OVRInput.GetDown(OVRInput.Button.One))
        {
            seeAround = true;
        }    

        //whether breaking
        if(OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickDown,OVRInput.Controller.RTouch))
        {
            breaking = true;
        }    
        if(OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickDown,OVRInput.Controller.RTouch))
        {
            breaking = false;
        }

        //whether turining left
        if(OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft,OVRInput.Controller.RTouch))
        {
            turningLeft = true;
        }    
        if(OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickLeft,OVRInput.Controller.RTouch))
        {
            turningLeft = false;
        }

        //whether turining right
        if(OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickRight,OVRInput.Controller.RTouch))
        {
            turningRight = true;
        }    
        if(OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickRight,OVRInput.Controller.RTouch))
        {
            turningRight = false;
        }
    }

}
