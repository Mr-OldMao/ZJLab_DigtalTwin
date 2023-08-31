using MFramework;
using System;
using UnityEngine;
using UnityEngine.AI;

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
    private Animator m_RobotAnimator;

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
        curRobotState = RobotBaseState.Idel;
        m_PathLineRenderer = this.GetComponent<LineRenderer>();
        m_RobotAnimator = transform.Find<Animator>("Mesh1");
        targetPoint = GameObject.Find("TargetPoint")?.transform;
        RegisterMsgEvent();
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (autoMoveTargetPoint)
            {
                curRobotState = RobotBaseState.Idel;
                Move(targetPoint);
            }
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
            m_RobotAnimator?.SetBool("IsMoving", true);
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
            m_RobotAnimator?.SetBool("IsMoving", false);
        });

        MsgEvent.RegisterMsgEvent(MsgEventName.DoorAnimBegin, () =>
        {
            MsgEvent.SendMsg(MsgEventName.RobotMoveEnd);
            m_RobotAnimator?.SetTrigger("OpenDoor");
        });

        MsgEvent.RegisterMsgEvent(MsgEventName.DoorAnimEnd, () =>
        {
            Move(targetPoint);
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
                MsgEvent.SendMsg(MsgEventName.RobotMoveBegin);
                m_NavMeshAgent.isStopped = false;
                m_NavMeshAgent.SetDestination(targetPoint);
                MFramework.UnityTool.GetInstance.DelayCoroutineWaitReturnTrue(() =>
                {
                    return Vector3.Distance(transform.position, targetPoint) < 0.1f;
                }, () =>
                {
                    Debug.Log("Move Complete");
                    m_NavMeshAgent.isStopped = true;
                    curRobotState = RobotBaseState.Idel;
                    callback?.Invoke();
                });
            }
            else
            {
                Debug.LogError("Robot is Moving");
            }
        }
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
            MsgEvent.SendMsg(MsgEventName.RobotMoveEnd);
        }
    }
}
