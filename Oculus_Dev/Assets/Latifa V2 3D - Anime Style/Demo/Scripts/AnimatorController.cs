using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    private Animator animator;
    private AnimationClip[] animationClip;
    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        animator = GetComponent<Animator>();
        animationClip = animator.runtimeAnimatorController.animationClips;
        Debug.Log("该对象有" + animationClip.Length + "个动画");
        foreach(AnimationClip a in animationClip)//遍历获取所有该对象的动画名
        {
            Debug.Log(a.name);
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("按下A");
            animator.Play(animationClip[4].name);   //播放动画
            animator.Update(0);         //刷新0层的动画，默认新建的动画在0层。
            //GetAnimatorInfo();
        }
        // if (Input.GetKeyDown(KeyCode.B))
        // {
        //     Debug.Log("按下B");
        //     animator.Play(animationClip[1].name);
        //     animator.Update(0);
        //     GetAnimatorInfo();
        // }
    }
    void GetAnimatorInfo()
    {
        string name = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;//获取当前播放动画的名称
        Debug.Log("当前播放的动画名为：" + name);
        float length = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;//获取当前动画的时间长度
        Debug.Log("播放动画的长度：" + length);
    }
}
