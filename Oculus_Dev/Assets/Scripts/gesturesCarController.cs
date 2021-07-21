using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class gesturesCarController : MonoBehaviour
{

    public Animator AVAnimator;
    public Animator ArmAnimator;
    public Animator BatonArmAnimator;

    private AnimatorStateInfo animatorInfo;
    private AnimatorStateInfo armInfo;
    private AnimatorStateInfo batonArmInfo;
    public int setRandomNum = 0;
    public int eHMIType = 0; //0: Word Signal; 1: Traffic Sign; 2: Face; 3: Word + Face; 4: Traffic Sign + Face
    //city
    private GameObject city;
    private GameObject nose;
    private GameObject leftEye;
    private GameObject rightEye;
    private GameObject sign;
    private GameObject stop;

    private GameObject rightArm;
    private int armState = 0;   // 0 = Start, 1 = Open, 2 = Resting, 3 = Go Ahead
    private int gestureNum = 0;
    private int bGestureNum = 0;

    private string[] armAnimations = {"Go Ahead", "Still Palm Out", "Forearm Wave", "Finger Point", "Motion Downward", "One Finger Wait"};
    private string[] batonArmAnimations = {"Sweep Sideways", "Baton Circles"};

    private GameObject[] tips = new GameObject[3];
    private GameObject[] lips = new GameObject[3];
    private Transform zebraLine;
    private Vector3 movement = new Vector3(0.0f, 0.0f, 0.05f);
    private AnimationClip[] animationClip;
    //generate the random number
    private int RandomNum = 0;
    void Start()
    {

        // locate the arms
        rightArm = GameObject.Find("Waymo/RightArm");

        // located the zebraline
        city = GameObject.Find("City");
        Transform props = city.transform.Find("props");
        zebraLine = props.transform.Find("Street 8 Prefab (6)");
        animationClip = AVAnimator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip c in animationClip)
        {
            Debug.Log(c.name);
        }
        nose = GameObject.Find("Waymo/Nose");
        nose.GetComponent<MeshRenderer>().material.color = Color.black;
        rightEye = GameObject.Find("Waymo/RightEye");
        leftEye = GameObject.Find("Waymo/LeftEye");
        sign = GameObject.Find("Waymo/Sign");
        stop = GameObject.Find("Waymo/Stop");
        tips[0] = GameObject.Find("Waymo/FrontTips/Text");
        tips[1] = GameObject.Find("Waymo/SideWindowTips/Right");
        tips[2] = GameObject.Find("Waymo/SideWindowTips/Left");
        lips[0] = GameObject.Find("Waymo/NormalLip");
        lips[1] = GameObject.Find("Waymo/PositiveLip");
        lips[2] = GameObject.Find("Waymo/NegativeLip");
        for (int i = 0; i < 3; i++)
        {
            tips[i].GetComponent<Text>().text = "";
            tips[i].SetActive(eHMIType == 0 || eHMIType == 3);
            lips[i].SetActive(eHMIType > 1);
        }
        RandomNum = setRandomNum < 2 ? setRandomNum : Random.Range(0, 2);

        // Debug.Log("random number is:" + RandomNum);

        nose.SetActive(eHMIType > 1);
        rightEye.SetActive(eHMIType > 1);
        leftEye.SetActive(eHMIType > 1);
        //sign.SetActive(false);
        stop.SetActive(false);


        // Start the Start Animation
        ArmAnimator.Play("Start");
        BatonArmAnimator.Play("StartBaton");
        Debug.Log("Playing Start animation");
    }


    void Update()
    {
        //moving AV
        this.transform.Translate(movement);
        float sqrLength = (zebraLine.position - this.transform.position).sqrMagnitude;  //获取两个物体向量的平方长度
        //play animations
        animatorInfo = AVAnimator.GetCurrentAnimatorStateInfo(0);
        if (sqrLength < 12 * 12 && sqrLength >= 7 * 7)
        {
            //slow down
            movement.z = 0.02f;
            ArmAnimator.Play("Open");
            BatonArmAnimator.Play("OpenBaton");
            //armState = 1;

            nose.GetComponent<MeshRenderer>().material.color = Color.black;
            if (eHMIType > 1)
                TriggerSeeAround();
            
            else
                for (int i = 0; i < 3; i++)
                {
                    tips[i].GetComponent<Text>().text = "";
                }
        }
        else if (sqrLength < 7 * 7)
        {

            armInfo = ArmAnimator.GetCurrentAnimatorStateInfo(0);
            batonArmInfo = BatonArmAnimator.GetCurrentAnimatorStateInfo(0);

            // stop
            movement.z = 0.0f;


           // Play the next animation for non-baton arm
           if (armInfo.normalizedTime >= 1 && gestureNum <= 5) {
                ArmAnimator.Play(armAnimations[gestureNum]);
                gestureNum++;
            } 

            // Play next animation for baton arm
            if (batonArmInfo.normalizedTime >= 1 && bGestureNum <= 1){
                BatonArmAnimator.Play(batonArmAnimations[bGestureNum]);
                bGestureNum++;
            }
           


        }
    }


         void TriggerNegative()
        {
            Debug.Log("TriggerNegative_Car");
            AVAnimator.Play("Negative_Car");
            AVAnimator.Update(0);
        }

         void TriggerPositive()
        {
            Debug.Log("TriggerPositive");
            AVAnimator.Play("Positive_Car");
            AVAnimator.Update(0);
        }
         void TriggerSeeAround()
        {
            Debug.Log("see around");
            //activatedAnimator.SetTrigger("SeeAroundTrigger");
            AVAnimator.Play("Idle_Car");   //播放动画
            AVAnimator.Update(0);
        }
        void GetAnimatorInfo()
        {
            string name = AVAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name;//获取当前播放动画的名称
            Debug.Log("当前播放的动画名为：" + name);
            float length = AVAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length;//获取当前动画的时间长度
            Debug.Log("播放动画的长度：" + length);
        }
    }