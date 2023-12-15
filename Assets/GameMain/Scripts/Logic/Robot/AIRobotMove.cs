using MFramework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using static GenerateRoomBorderModel;
using static TaskCenter;

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

    /// <summary>
    /// 自动切换目标点NavNode协程
    /// </summary>
    private Coroutine m_AutoChangeNavNodeCorountine = null;

    void Start()
    {
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
        m_PathLineRenderer = this.GetComponent<LineRenderer>();
        m_RobotAnimCenter = GetComponent<RobotAnimCenter>();
        targetPoint = GameObject.Find("TargetPoint")?.transform;
        RegisterMsgEvent();
        InitRobotAnimParam();
    }

    public void ShowRobotPath(bool isShow)
    {
        m_PathLineRenderer.enabled = isShow;
    }

    private void InitRobotAnimParam()
    {
        curRobotState = RobotBaseState.Idel;
    }

    public void SetTargetPointObj(Vector3 pos, Vector3 rot)
    {
        targetPoint.gameObject.SetActive(false);
        targetPoint.position = pos;
        targetPoint.rotation = Quaternion.Euler(rot);
        targetPoint.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //if (autoMoveTargetPoint)
            {
                curRobotState = RobotBaseState.Idel;
                Move(targetPoint);
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log(JudgeCanArrivePos(targetPoint.position));
        }


        if (canMove && curRobotState == RobotBaseState.Moving)
        {
            MsgEvent.SendMsg(MsgEventName.RobotMoveStay);
        }
        UpdateMoveSpeed();


        if (Input.GetKeyDown(KeyCode.U))
        {

            StartAutoChangeTargetNavNodeCor();
        }
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
            //继续移动
            if (TaskCenter.GetInstance.GetCurExecuteTask != null)
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
            }
        });

        MsgEvent.RegisterMsgEvent(MsgEventName.GenerateSceneComplete, () =>
        {
            InitRobotAnimParam();
        });
    }

    public void Move(Transform targetPoint, Action callback = null, bool isForceMove = false)
    {
        Move(targetPoint.position, callback);
    }

    /// <summary>
    /// 移动至目标点，到达目标点返回
    /// </summary>
    /// <param name="targetPoint"></param>
    /// <param name="callback"></param>
    public void Move(Vector3 targetPoint, Action callback = null, bool isForceMove = false)
    {
        if (canMove)
        {
            if (curRobotState != RobotBaseState.Moving || isForceMove)
            {
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
        bool res = false, res1 = false, res2 = false, res3 = false;

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

        //判断目标点及周边一定范围内是否有可到达的点位
        res2 = false;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 0.1f, NavMesh.AllAreas))
        {
            res2 = true;
        }
        m_NavMeshAgent.SetDestination(targetPos);
        m_NavMeshAgent.isStopped = true;
        res3 = m_NavMeshAgent.pathStatus == NavMeshPathStatus.PathComplete;
        Debug.Log("res1 " + res1 + " res2  " + res2 + "  res3 " + res3);

        res = res1 && res2 && res3;

        Debug.Log("res " + res);


        return res;
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
    /// 自动切换导航目标点
    /// </summary>
    /// <param name="delay">执行间隔</param>
    /// <param name="listenerTime">自动监听的时间</param>
    public void StartAutoChangeTargetNavNodeCor(float delay = 3f, float listenerTime = TaskCenter.TaskLimitTime)
    {
        Debugger.Log("自动切换导航目标点StartAutoChangeTargetNavNodeCor");
        if (m_AutoChangeNavNodeCorountine != null)
        {
            StopCoroutine(m_AutoChangeNavNodeCorountine);
            m_AutoChangeNavNodeCorountine = null;
        }

        //Vector3 oldPos = transform.position;
        m_AutoChangeNavNodeCorountine = UnityTool.GetInstance.DelayCoroutineTimer((p) =>
         {
             //Vector3 newPos = transform.position;
             JudgeMoveSite(delay, (b) =>
             {
                 Debugger.Log("判断是否在原地移动 " + b);
                 if (b)
                 {
                     //在限时内，如果无法到达目标位置则自动更换目标位置
                     NavNodeData newNavNode = TaskCenter.GetInstance.GetNewNavNode(targetPoint.position);
                     if (newNavNode != null)
                     {
                         // 换其他节点移动
                         Debugger.Log("换其他节点移动");
                         SetTargetPointObj(newNavNode.pos, newNavNode.rot);
                         Move(targetPoint, null, true);
                     }
                 }
             });

             ////在限时内，如果无法到达目标位置则自动更换目标位置
             //Vector3 newNavNode = TaskCenter.GetInstance.GetNewNavNode(targetPoint.position);
             //if (newNavNode != Vector3.zero)
             //{
             //    // 换其他节点移动
             //    targetPoint.position = newNavNode;
             //    Move(targetPoint, null, true);
             //}
             return p < listenerTime;
         }, delay);
    }

    /// <summary>
    /// 检测是否到达目的地
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("coll " + collision.collider.name);
        if (collision.collider.gameObject.name == "TargetPoint")
        {
            transform.position = targetPoint.position;
            transform.rotation = Quaternion.Euler(targetPoint.transform.eulerAngles);
            //transform.LookAt(targetPoint);

            //终止协程
            if (m_AutoChangeNavNodeCorountine != null)
            {
                StopCoroutine(m_AutoChangeNavNodeCorountine);
                m_AutoChangeNavNodeCorountine = null;
            }


            MsgEvent.SendMsg(MsgEventName.RobotMoveEnd);
            MsgEvent.SendMsg(MsgEventName.RobotArriveTargetPos);
        }
    }


    /// <summary>
    /// 判断是否在原地移动
    /// </summary>
    /// <param name="callback">t-原地没动</param>
    private void JudgeMoveSite(float delay, Action<bool> callback)
    {
        Vector3 pos = transform.position;

        UnityTool.GetInstance.DelayCoroutine(delay, () =>
        {
            Vector3 curPos = transform.position;
            callback(Vector3.Distance(pos, curPos) <= 0.05);
        });

    }
}
