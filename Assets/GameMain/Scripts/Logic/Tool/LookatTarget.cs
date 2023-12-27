using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using MFramework;
using Debugger = MFramework.Debugger;
using UnityEngine.UI;
/// <summary>
/// 标题：朝向目标点
/// 功能：
/// 作者：毛俊峰
/// 时间：2023.
/// </summary>
public class LookatTarget : MonoBehaviour
{
    private GameObject target;

    private void Awake()
    {
       
    }
    // Start is called before the first frame update
    void Start()
    {
        //target = FindObjectOfType<AIRobotMove>()?.gameObject;
        target = CameraControl.GetInstance.GetCameraEntity(CameraControl.CameraType.Free)?.gameObject;
        if (target == null)
        {
            Debugger.LogError("target is null,");
        }
        string parentName = transform.parent.name;
        transform.GetComponentInChildren<Text>().text = parentName;
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            transform.LookAt(new Vector3(target.transform.position.x, 2, target.transform.position.z));
        }
    }
}
