using MFramework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using static GenerateRoomBorderModel;

/// <summary>
/// 标题：AI
/// 功能：AI移动
/// 作者：毛俊峰
/// 时间：2023.
/// </summary>
[RequireComponent(typeof(LineRenderer), typeof(NavMeshAgent))]
public class AIRobotMove : MonoBehaviour
{
    //是否允许移动
    public bool canMove = true;
    //是否允许移动追踪目标点
    public bool autoMoveTargetPoint = true;

    [Range(0f, 10f)]
    public float moveSpeed = 1f;

    public Transform targetPoint;
    private NavMeshAgent m_NavMeshAgent;
    private LineRenderer m_PathLineRenderer;
    private RobotAnimCenter m_RobotAnimCenter;

    /// <summary>
    /// 机器人基本状态
    /// </summary>
    public RobotBaseState curRobotState;
    ///// <summary>
    ///// 机器人行为状态
    ///// </summary>
    //public RobotActionState curRobotActionState;
    public enum RobotBaseState
    {
        Idel,
        /// <summary>
        /// 正在移动
        /// </summary>
        Moving,
    }

    //public enum RobotActionState
    //{
    //    /// <summary>
    //    /// 闲置
    //    /// </summary>
    //    Idle
    //}


    void Start()
    {
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
        m_PathLineRenderer = this.GetComponent<LineRenderer>();
        m_RobotAnimCenter = GetComponent<RobotAnimCenter>();
        targetPoint = GameObject.Find("TargetPoint")?.transform;
        RegisterMsgEvent();
        InitRobotAnimParam();
    }

    private void InitRobotAnimParam()
    {
        curRobotState = RobotBaseState.Idel;
    }

    public void SetTargetPointObj(Vector3 pos)
    {
        targetPoint.gameObject.SetActive(false);
        targetPoint.position = pos;
        targetPoint.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    if (autoMoveTargetPoint)
        //    {
        //        curRobotState = RobotBaseState.Idel;
        //        Move(targetPoint);
        //    }
        //}

        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log(JudgeCanArrivePos(targetPoint.position));
        }


        if (canMove && curRobotState == RobotBaseState.Moving)
        {
            MsgEvent.SendMsg(MsgEventName.RobotMoveStay);
        }
        UpdateMoveSpeed();
    }

    private void RegisterMsgEvent()
    {
        MsgEvent.RegisterMsgEvent(MsgEventName.RobotMoveBegin, () =>
        {
            //Debug.Log("Robot RobotMoveBegin");
            curRobotState = RobotBaseState.Moving;
            m_RobotAnimCenter?.PlayAnimByBool("IsMoving", true);
        });
        MsgEvent.RegisterMsgEvent(MsgEventName.RobotMoveStay, () =>
        {
            //Debug.Log("Robot RobotMoveStay");
            //更新导航路线
            DrawNavMeshAgentLine();

        });
        MsgEvent.RegisterMsgEvent(MsgEventName.RobotMoveEnd, () =>
        {
            //Debug.Log("Robot RobotMoveEnd");
            m_NavMeshAgent.isStopped = true;
            curRobotState = RobotBaseState.Idel;
            //curRobotActionState = RobotActionState.Idle;
            m_RobotAnimCenter?.PlayAnimByBool("IsMoving", false);
        });

        MsgEvent.RegisterMsgEvent(MsgEventName.DoorAnimBegin, () =>
        {
            //MsgEvent.SendMsg(MsgEventName.RobotMoveEnd);
            m_NavMeshAgent.isStopped = true;
            curRobotState = RobotBaseState.Idel;
            m_RobotAnimCenter?.PlayAnimByBool("IsMoving", false);
            m_RobotAnimCenter?.PlayAnimByTrigger("OpenDoor");
        });

        MsgEvent.RegisterMsgEvent(MsgEventName.DoorAnimEnd, () =>
        {
            float dis = Vector3.Distance(transform.position, targetPoint.transform.position);
            Debugger.Log("curPos:" + transform.position + "  targetPos:" + targetPoint.transform.position + ",dis " + dis);
            if (dis < 0.5f)
            {
                Debugger.Log("目标点太近无法移动");
                if (TaskCenter.GetInstance.GetCurExecuteTask == null
            || TaskCenter.GetInstance.GetCurExecuteTask.name == Order.Close_Door_Inside
            || TaskCenter.GetInstance.GetCurExecuteTask.name == Order.Close_Door_Outside
            || TaskCenter.GetInstance.GetCurExecuteTask.name == Order.Open_Door_Inside
            || TaskCenter.GetInstance.GetCurExecuteTask.name == Order.Open_Door_Outside)
                {
                    return;
                }
            }

            Move(targetPoint);
        });

        MsgEvent.RegisterMsgEvent(MsgEventName.GenerateSceneComplete, () =>
        {
            InitRobotAnimParam();
        });
    }

    public void Move(Transform targetPoint, Action callback = null)
    {
        Move(targetPoint.position, callback);
    }

    /// <summary>
    /// 移动至目标点，到达目标点返回
    /// </summary>
    /// <param name="targetPoint"></param>
    /// <param name="callback"></param>
    public void Move(Vector3 targetPoint, Action callback = null)
    {
        if (canMove)
        {
            if (curRobotState != RobotBaseState.Moving)
            {
                Debug.Log("movemovemovemovemove");
                MsgEvent.SendMsg(MsgEventName.RobotMoveBegin);
                m_NavMeshAgent.isStopped = false;
                m_NavMeshAgent.SetDestination(targetPoint);
                //MFramework.UnityTool.GetInstance.DelayCoroutineWaitReturnTrue(() =>
                //{
                //    return Vector3.Distance(transform.position, targetPoint) < 0.1f;
                //}, () =>
                //{
                //    Debug.Log("Move Complete");
                //    m_NavMeshAgent.isStopped = true;
                //    curRobotState = RobotBaseState.Idel;
                //    callback?.Invoke();
                //});
            }
            else
            {
                Debug.LogError("Robot is Moving");
            }
        }
    }

    /// <summary>
    /// 判定是否能到达目标位置
    /// </summary>
    /// <returns></returns>
    public bool JudgeCanArrivePos(Vector3 targetPos)
    {
        bool res1 = false, res2 = false;

        //判定目标点是否在屋内
        Vector2 offsetValue = GameLogic.GetInstance.GetOriginOffset();
        //x
        List<BorderEntityData> xBorderEntityData = GenerateRoomData.GetInstance.listRoomBuilderInfo.FindAll((p) =>
        {
            return (p.entityModelType == EntityModelType.Wall || p.entityModelType == EntityModelType.Door) && p.entityAxis == 0 && p.pos.x == (int)((int)targetPos.x - offsetValue.x);
        });
        if (xBorderEntityData?.Count > 0)
        {
            float maxY = xBorderEntityData[0].pos.y;
            float minY = xBorderEntityData[0].pos.y;
            foreach (var item in xBorderEntityData)
            {
                if (item.pos.y > maxY)
                {
                    maxY = item.pos.y;
                }
                if (item.pos.y < minY)
                {
                    minY = item.pos.y;
                }
            }
            maxY += offsetValue.y;
            minY += offsetValue.y;
            res1 = targetPos.z >= minY && targetPos.z <= maxY;
        }

        //判断目标点是否有障碍物
        res2 = true;
        //NavMeshHit hit;
        //if (NavMesh.SamplePosition(targetPos, out hit, 1.0f, NavMesh.AllAreas))
        //{
        //    res2 = true;
        //}

        return res1 && res2;
    }

    private void UpdateMoveSpeed()
    {
        if (m_NavMeshAgent.speed != moveSpeed)
        {
            m_NavMeshAgent.speed = moveSpeed;
        }
    }

    /// <summary>
    /// 画导航路线
    /// </summary>
    private void DrawNavMeshAgentLine()
    {
        Vector3[] path = m_NavMeshAgent.path.corners;//导航路径点
        m_PathLineRenderer.positionCount = path.Length;//linerenderer组件

        for (int i = 0; i < path.Length; i++)
        {
            m_PathLineRenderer.SetPosition(i, path[i]);
        }
    }


    /// <summary>
    /// 检测是否到达目的地
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("coll " + collision.collider.name);
        if (collision.collider.gameObject.name == "TargetPoint")
        {
            transform.LookAt(targetPoint);

            MsgEvent.SendMsg(MsgEventName.RobotMoveEnd);
            MsgEvent.SendMsg(MsgEventName.RobotArriveTargetPos);
        }
    }
}
