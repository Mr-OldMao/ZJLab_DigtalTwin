using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFramework;
/// <summary>
/// 标题：任务中心
/// 功能：根据指令用于派发任务，处理任务
/// 作者：毛俊峰
/// 时间：2023.09.28
/// </summary>
public class TaskCenter : SingletonByMono<TaskCenter>
{
    /// <summary>
    /// 任务指令集
    /// </summary>
    public List<string> m_ListOrder = new List<string>();

    /// <summary>
    /// 是否允许机器人自动处理任务
    /// </summary>
    public bool CanExecuteTask { get; set; } = true;
    /// <summary>
    /// 机器人是否正在执行任务
    /// </summary>
    public bool IsExecuteTask { get; private set; } = false;

    /// <summary>
    /// 获取当前机器人正在执行的任务，为空则未在执行位置
    /// </summary>
    public ControlCommit GetCurExecuteTask { get; private set; }

    private AIRobotMove aIRobotMove = null;

    /// <summary>
    /// 执行任务限制用时
    /// </summary>
    private const float m_TaskLimitTime = 20f;
    /// <summary>
    /// 执行当前任务用时
    /// </summary>
    private float m_CurTaskExeTime;
    /// <summary>
    /// 限时任务协程
    /// </summary>
    private Coroutine m_CorLimitTask;
    public void Init()
    {
        if (aIRobotMove == null)
        {
            //aIRobotMove = GameObject.FindGameObjectWithTag("Player")?.GetComponent<AIRobotMove>();
            aIRobotMove = FindObjectOfType<AIRobotMove>();
            RegisterEvent();
        }
    }
    private void RegisterEvent()
    {
        MsgEvent.RegisterMsgEvent(MsgEventName.RobotArriveTargetPos, () =>
        {
            TaskExecuteSuc();
        });
    }

    /// <summary>
    /// 解析指令并执行任务
    /// </summary>
    public void ParseOrderExcute(ControlCommit controlCommit)
    {
        IsExecuteTask = true;
        GetCurExecuteTask = controlCommit;
        Vector3 targetPos = new Vector3(controlCommit.position[0], controlCommit.position[1], controlCommit.position[2]);
        //判定是否能到达目标位置
        bool canArrive = aIRobotMove.JudgeCanArrivePos(targetPos);
        if (canArrive)
        {
            aIRobotMove.SetTargetPointObj(targetPos);
            //导航到目标位置
            aIRobotMove.Move(targetPos);

            //限时，未在指定时间内到达指定位置视为无法到达
            m_CorLimitTask = UnityTool.GetInstance.DelayCoroutine(m_TaskLimitTime, () => TaskExecuteFail());
        }
        else
        {
            TaskExecuteFail();
        }
    }

    /// <summary>
    /// 任务执行成功回调
    /// </summary>
    private void TaskExecuteSuc()
    {
        if (GetCurExecuteTask == null)
        {
            return;
        }
        //到达指定位置，与物体交互，播放动画TODO
        Debug.Log("到达指定位置");

        //以上事务处理完毕后返回指令执行结果
        ControlResult controlResult = new ControlResult
        {
            motionId = GetCurExecuteTask.motionId,
            name = GetCurExecuteTask.name,
            task_id = GetCurExecuteTask.task_id,
            simulatorId = "",
            stateCode = 0,
            stateMsg = "suc"
        };
        InterfaceDataCenter.GetInstance.SendMQTTControlResult(controlResult);
        IsExecuteTask = false;
        GetCurExecuteTask = null;
        m_CurTaskExeTime = 0;
        if (m_CorLimitTask != null)
        {
            StopCoroutine(m_CorLimitTask);
            m_CorLimitTask = null;
        }
    }

    /// <summary>
    /// 任务执行失败回调
    /// </summary>
    /// <param name="stateCode"></param>
    private void TaskExecuteFail(int stateCode = 2)
    {
        Debug.LogError("无法导航到目标位置");
        //返回指令执行结果
        ControlResult controlResult = new ControlResult
        {
            motionId = GetCurExecuteTask.motionId,
            name = GetCurExecuteTask.name,
            task_id = GetCurExecuteTask.task_id,
            simulatorId = "",
            stateCode = stateCode,
            stateMsg = "dont arrive target pos"
        };
        InterfaceDataCenter.GetInstance.SendMQTTControlResult(controlResult);
        IsExecuteTask = false;
        GetCurExecuteTask = null;
    }

    private void Update()
    {
        ListenerTask();
    }

    /// <summary>
    /// 监听任务
    /// </summary>
    public void ListenerTask()
    {
        if (CanExecuteTask && !IsExecuteTask)
        {
            if (MainData.controlCommit?.Count > 0)
            {
                ControlCommit controlCommit = MainData.controlCommit.Dequeue();
                ParseOrderExcute(controlCommit);
            }
        }
        if (IsExecuteTask)
        {
            m_CurTaskExeTime += Time.deltaTime;
        }
    }

}

public enum RobotTaskState
{

}

class Order
{
    public const string Close_Door_Inside = "Close_Door_Inside";
    public const string Close_Door_Outside = "Close_Door_Outside";
    public const string Drink = "Drink";
    public const string Grab_item = "Grab_item";
    public const string Heal_bandages = "Heal_bandages";
    public const string Knock_on_door = "Knock_on_door";
    public const string Lever_Floor_Pull = "Lever_Floor_Pull";
    public const string Lever_Floor_Push = "Lever_Floor_Push";
    public const string Lever_Wall_Pull = "Lever_Wall_Pull";
    public const string Lever_Wall_Push = "Lever_Wall_Push";
    public const string Pick_Fwd = "Pick_Fwd";
    public const string Pick_L = "Pick_L";
    public const string Pick_R = "Pick_R";
    public const string Press_Button = "Press_Button";
    public const string Press_Loop = "Press_Loop";
    public const string Push_End = "Push_End";
    public const string Push_Enter = "Push_Enter";
    public const string Push_Exit = "Push_Exit";
    public const string Push_Idle = "Push_Idle";
    public const string Push_Idle_inPlace = "Push_Idle_inPlace";
    public const string Push_Loop = "Push_Loop";
    public const string Push_Start = "Push_Start";
    public const string IDLE = "IDLE";
}
