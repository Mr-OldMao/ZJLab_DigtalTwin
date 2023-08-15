using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Task : MonoBehaviour
{
    // 枚举用于表示任务的状态
    public enum TaskStatus
    {
        Success,
        Running,
        Failure
    }

    // 用于存储任务的当前状态
    private TaskStatus status = TaskStatus.Running;

    // 公共属性，用于获取和设置任务状态
    public TaskStatus Status
    {
        get { return status; }
        set { status = value; }
    }



}
