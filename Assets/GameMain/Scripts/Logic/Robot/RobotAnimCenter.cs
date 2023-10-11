using MFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 标题：机器人动画管理中心
/// 功能：
/// 作者：毛俊峰
/// 时间：2023.10.07
/// </summary>
public class RobotAnimCenter : MonoBehaviour
{
    private Animator m_RobotAnimator;
     
    public Transform RobotHandleNode
    {
        get; private set;
    }


    private void Awake()
    {
        m_RobotAnimator = transform.Find<Animator>("Mesh1");
        RobotHandleNode = transform.Find<Transform>("HandleNode");
    }

    private void Start()
    {
        PlayAnimByBool("IsMoving", false);
        PlayAnimByBool("CloseDoor", false);
        PlayAnimByBool("OpenDoor", false);
        PlayAnimByBool("CanInteraction", false);

    }
    public float PlayAnimByName(string animStr)
    {
        m_RobotAnimator.Play(animStr);
        return GetAnimatorLength(animStr);
    }

    public float PlayAnimByTrigger(string triggerName)
    {
        m_RobotAnimator.SetTrigger(triggerName);
        return GetAnimatorLength(triggerName);
    }

    public float PlayAnimByBool(string paramName, bool state)
    {
        m_RobotAnimator.SetBool(paramName, state);
        return GetAnimatorLength(paramName);
    }

    /// <summary>
    /// 获得animator下某个动画片段的时长方法
    /// </summary>
    /// <param name="name">要获得的动画片段名字</param>
    /// <returns></returns>
    public float GetAnimatorLength(string name)
    {
        //动画片段时间长度
        float length = 0;

        AnimationClip[] clips = m_RobotAnimator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name.Equals(name))
            {
                length = clip.length;
                break;
            }
        }
        return length;
    }
}
