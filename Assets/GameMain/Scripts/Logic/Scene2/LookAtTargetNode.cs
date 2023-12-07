using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFramework;
/// <summary>
/// 标题：ui面板朝向目标点
/// 功能：
/// 作者：毛俊峰
/// 时间：2023.12.03
/// </summary>
public class LookAtTargetNode : MonoBehaviour
{
    public Transform targetNode;
    void Start()
    {
        if (targetNode == null)
        {
            targetNode = Camera.main.transform;
        }
        if (targetNode == null)
        {
            Debugger.LogError("targetNode is null");
        }

    }

    // Update is called once per frame
    void Update()
    {
        if(targetNode != null)
        {
            transform.LookAt(targetNode);
        }
    }
}
