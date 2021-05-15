using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMana : MonoBehaviour
{
    // Start is called before the first frame update
    public Animator characterAnimator;
    public void TriggerSit()
    {
        Debug.Log("sit");
        characterAnimator.SetTrigger("SitTrigger");
    }

    public void TriggerSeeArounnd()
    {
        Debug.Log("see around");
        //characterAnimator.SetTrigger("SeeAroundTrigger");
        characterAnimator.Play("SeeAround_Mech");   //播放动画
        characterAnimator.Update(0);         //刷新0层的动画，默认新建的动画在0层。
    }

    public void TriggerNod()
    {
        Debug.Log("Nod");
        characterAnimator.Play("Nod");   //播放动画
        characterAnimator.Update(0);         //刷新0层的动画，默认新建的动画在0层。
    }

    public void TriggerNegative()
    {
        Debug.Log("TriggerNegative");
        characterAnimator.Play("Negative_Mech"); 
        characterAnimator.Update(0);       
    }

    public void TriggerPositive()
    {
        Debug.Log("TriggerPositive");
        characterAnimator.Play("Positive"); 
        characterAnimator.Update(0);       
    }
}
