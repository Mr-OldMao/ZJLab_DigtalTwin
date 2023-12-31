using System.Collections.Generic;
using UnityEngine;
using MFramework;
using System;
using UnityEngine.AI;
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

    private bool m_CanExecuteTask = true;
    /// <summary>
    /// 是否允许机器人自动处理任务
    /// </summary>
    public bool CanExecuteTask
    {
        get
        {
            return m_CanExecuteTask;
        }
        set
        {
            ////监听“门”碰撞事件
            //GameLogic.GetInstance.ListenerDoorCollEvent(value);
            m_CanExecuteTask = value;
        }
    }
    /// <summary>
    /// 机器人是否正在执行任务
    /// </summary>
    public bool IsExecuteTask { get; private set; } = false;

    /// <summary>
    /// 获取当前机器人正在执行的任务，为空则未在执行位置
    /// </summary>
    public ControlCommit GetCurExecuteTask { get; private set; }

    private AIRobotMove m_AIRobotMove = null;
    private RobotAnimCenter m_RobotAnimCenter = null;
    /// <summary>
    /// 执行任务限制用时
    /// </summary>
    public const float TaskLimitTime = 30f;
    /// <summary>
    /// 执行当前任务用时
    /// </summary>
    private float m_CurTaskExeTime;
    /// <summary>
    /// 限时任务协程
    /// </summary>
    private Coroutine m_CorLimitTask;

    /// <summary>
    /// 缓存“拿”某物体时，当前的父对象，便于“放”时回到之前父对象下
    /// </summary>
    private Dictionary<GameObject, Transform> m_DicCacheGrabParent = new Dictionary<GameObject, Transform>();

    /// <summary>
    /// 当前任务的目标坐标位置
    /// </summary>
    private List<NavNodeData> m_TargetPos = new List<NavNodeData>();


    public class NavNodeData
    {
        public Vector3 pos;
        public Vector3 rot;
    }

    public void Init()
    {
        if (m_AIRobotMove == null)
        {
            //aIRobotMove = GameObject.FindGameObjectWithTag("Player")?.GetComponent<AIRobotMove>();
            m_AIRobotMove = FindObjectOfType<AIRobotMove>();
            RegisterEvent();
        }

        m_RobotAnimCenter = GameObject.FindObjectOfType<RobotAnimCenter>();
    }
    private void RegisterEvent()
    {
        MsgEvent.RegisterMsgEvent(MsgEventName.RobotArriveTargetPos, () =>
        {
            ArriveTargetPosCallback();
        });
    }



    /// <summary>
    /// 解析指令并执行任务
    /// </summary>
    public void ParseOrderExcute(ControlCommit controlCommit)
    {
        //强制改变机器人状态
        m_RobotAnimCenter.PlayAnimByBool("IsMoving", false);
        m_AIRobotMove.curRobotState = AIRobotMove.RobotBaseState.Idel;

        ////监听“门”碰撞事件
        //GameLogic.GetInstance.ListenerAllDoorOpenEvent(true);


        IsExecuteTask = true;
        GetCurExecuteTask = controlCommit;

        if (controlCommit.name == RobotOrderAnimData.Grab_item || controlCommit.name == RobotOrderAnimData.Grab_item_pull || controlCommit.name == RobotOrderAnimData.Pick_item)
        {
            string taskFailDes = string.Empty;
            bool isRight = JudgeGrabInterfaceOrder(controlCommit, ref taskFailDes);
            if (!isRight)
            {
                Debugger.LogError(taskFailDes);
                TaskExecuteFail(Vector3.zero, 1, taskFailDes);
                return;
            }
        }

        Vector3 targetPos = Vector3.zero;
        Vector3 targetRot = Vector3.zero;

        //通过id查要走到的位置
        //GameObject objModel = null;


        bool isFindNavNode = false;
        bool isInterfaceDoor = false;
        if (controlCommit.name == RobotOrderAnimData.Open_Door_Inside || controlCommit.name == RobotOrderAnimData.Close_Door_Inside || controlCommit.name == RobotOrderAnimData.Knock_on_door)
        {
            isInterfaceDoor = true;
        }
        string key = string.Empty;
        if (isInterfaceDoor)
        {
            key = isInterfaceDoor ? controlCommit.objectId : controlCommit.objectName + "_" + controlCommit.objectId;
        }

        List<Transform> navNodes = GameObject.Find(key)?.transform.Finds<Transform>("NavNode");

        if (!isInterfaceDoor)
        {
            if (navNodes?.Count > 0)
            {
                foreach (var item in navNodes)
                {
                    NavNodeData navNodeData = new NavNodeData { pos = item.position, rot = item.rotation.eulerAngles };
                    m_TargetPos.Add(navNodeData);
                    Debugger.Log("add " + navNodeData.pos);
                }
            }
        }
        else
        {
            //找最近的NavNode
            float dis1 = Vector3.Distance(navNodes[0].position, m_AIRobotMove.transform.position);
            float dis2 = Vector3.Distance(navNodes[1].position, m_AIRobotMove.transform.position);


            targetPos = dis1 < dis2 ? navNodes[0].position : navNodes[1].position;
            targetRot = dis1 < dis2 ? navNodes[0].rotation.eulerAngles : navNodes[1].rotation.eulerAngles;
            isFindNavNode = true;
        }




        
        if (m_TargetPos?.Count == 1)
        {
            //objModel = m_TargetPos[0].gameObject;
            targetPos = m_TargetPos[0].pos;
            targetRot = m_TargetPos[0].rot;
            isFindNavNode = true;
        }
        else if (m_TargetPos?.Count > 1)//寻找最合适的NavaNode
        {
            NavNodeData curNavNodeData = m_TargetPos[0];
            float minDis = Vector3.Distance(m_AIRobotMove.transform.position, curNavNodeData.pos);

            for (int i = 1; i < m_TargetPos.Count; i++)
            {
                float curDic = Vector3.Distance(m_AIRobotMove.transform.position, m_TargetPos[i].pos);
                if (curDic < minDis)
                {
                    minDis = curDic;
                    curNavNodeData = m_TargetPos[i];
                }
            }

            if (m_AIRobotMove.JudgeCanArrivePos(curNavNodeData.pos))
            {
                //objModel = curNav.gameObject;
                targetPos = curNavNodeData.pos;
                targetRot = curNavNodeData.rot;
                isFindNavNode = true;
            }
            else
            {
                foreach (var navNode in m_TargetPos)
                {
                    if (m_AIRobotMove.JudgeCanArrivePos(navNode.pos))
                    {
                        //objModel = navNode.gameObject;
                        targetPos = navNode.pos;
                        targetRot = navNode.rot;
                        isFindNavNode = true;
                        break;
                    }
                }
            }

            if (isFindNavNode)
            {
                Debugger.LogError("所用NavNode均不可使用， item :" + controlCommit.objectName + "_" + controlCommit.objectId);
            }
        }


        if (!isFindNavNode)
        {
            Debugger.LogError("未找到可用的NavNode，使用pos数据 , item :" + controlCommit.objectName + "_" + controlCommit.objectId);
            targetPos = new Vector3(controlCommit.position[0], controlCommit.position[1], controlCommit.position[2]);
            targetRot = Vector3.zero;
        }
        //else
        //{
        //    targetPos = objModel.transform.position;
        //    targetRot = objModel.transform.rotation.eulerAngles;
        //}


        NavNodeData navNodeData1 = new NavNodeData()
        {
            pos = new Vector3(controlCommit.position[0], controlCommit.position[1], controlCommit.position[2]),
            rot = Vector3.zero
        };
        m_TargetPos.Add(navNodeData1);

        //判定是否能到达目标位置
        bool canArrive = m_AIRobotMove.JudgeCanArrivePos(targetPos);

        if (canArrive)
        {
            m_AIRobotMove.SetTargetPointObj(targetPos, targetRot);
            //导航到目标位置
            m_AIRobotMove.Move(targetPos);

            ////在限时内，如果无法到达目标位置则自动更好目标位置
            //UnityTool.GetInstance.DelayCoroutineTimer((p) => 
            //{
            //    bool canArrive = m_AIRobotMove.JudgeCanArrivePos(targetPos);
            //    Debug.Log("test " + canArrive);
            //    //删除该节点
            //    if (!canArrive)
            //    {
            //        for (int i = 0; i < m_TargetPos.Count; i++)
            //        {
            //            if (m_TargetPos[i].position == targetPos)
            //            {
            //                m_TargetPos.Remove(m_TargetPos[i]);
            //                Debugger.Log("删除该节点成功");
            //                if (m_TargetPos.Count > 0)
            //                {
            //                    //换其他节点移动
            //                    m_AIRobotMove.Move(m_TargetPos[0], null, true);
            //                }
            //                break;
            //            }
            //        }
            //    }
            //    return p <= 10f || canArrive; 
            //}, 1f);

            //自动切换导航目标点
            m_AIRobotMove.StartAutoChangeTargetNavNodeCor();
            //限时，未在指定时间内到达指定位置视为无法到达
            m_CorLimitTask = UnityTool.GetInstance.DelayCoroutine(TaskLimitTime, () => TaskExecuteFail(targetPos));
        }
        else
        {
            TaskExecuteFail(targetPos);
        }

        m_AIRobotMove.ShowRobotPath(true);
    }


    /// <summary>
    /// 获取新的目标节点
    /// </summary>
    public NavNodeData GetNewNavNode(Vector3 oldNavNode)
    {
        NavNodeData newNavNode = null;
        //删除该节点
        for (int i = 0; i < m_TargetPos.Count; i++)
        {
            if (Vector3.Distance(m_TargetPos[i].pos, oldNavNode) < 0.1f)
            {
                m_TargetPos.Remove(m_TargetPos[i]);
                Debugger.Log("删除该节点成功" + ",count " + m_TargetPos.Count);
                break;
            }
        }
        if (m_TargetPos.Count > 0)
        {
            newNavNode = m_TargetPos[0];
            // m_TargetPos.Remove(m_TargetPos[0]);
            Debugger.Log("获取新的目标节点成功" + m_TargetPos[0].pos + ",count " + m_TargetPos.Count);
        }
        return newNavNode;

    }

    /// <summary>
    /// 终止当前的任务
    /// </summary>
    public void StopTask()
    {
        if (IsExecuteTask)
        {
            TaskExecuteFail(Vector2.zero, 1, "当前任务被终止");
        }
    }

    /// <summary>
    /// 机器人到达目标位置回调
    /// </summary>
    private void ArriveTargetPosCallback()
    {
        if (GetCurExecuteTask == null)
        {
            return;
        }
        //到达指定位置，与物体交互，播放动画
        RobotInteractionByOrder(() =>
        {

            //任务执行成功回调后返回指令执行结果
            ControlResult controlResult = new ControlResult
            {
                motionId = GetCurExecuteTask?.motionId,
                name = GetCurExecuteTask?.name,
                task_id = GetCurExecuteTask?.taskId,
                //simulatorId = GetCurExecuteTask?.simulatorId,
                stateCode = 0,
                stateMsg = "suc",
                objectName = GetCurExecuteTask?.objectName
                //targetRoomType = GetTargetRoomType().ToString()
            };
            InterfaceDataCenter.GetInstance.SendMQTTControlResult(controlResult);
            IsExecuteTask = false;
            GetCurExecuteTask = null;
            m_TargetPos.Clear();
            m_CurTaskExeTime = 0;
            if (m_CorLimitTask != null)
            {
                StopCoroutine(m_CorLimitTask);
                m_CorLimitTask = null;
            }
        });
        m_AIRobotMove.ShowRobotPath(false);
    }


    /// <summary>
    /// 机器人与物体交互，播放动画
    /// </summary>
    private void RobotInteractionByOrder(Action animCompleteCallback)
    {
        //解析指令名称
        string orderStr = GetCurExecuteTask.name;

        Debug.Log("到达指定位置 机器人与物体交互，播放动画  orderStr :" + orderStr);

        if (m_RobotAnimCenter != null)
        {
            float animSecond = 0;
            m_RobotAnimCenter.PlayAnimByBool("CanInteraction", true);

            //“拿取”“放下”交互对象实体
            GameObject grabObj = null;
            switch (orderStr)
            {
                //行走
                case RobotOrderAnimData.Walk:
                    break;
                //拿取
                case RobotOrderAnimData.Grab_item:
                    //物品父节点放置在机器人手中
                    grabObj = GrabEntity(1f);
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_Grab_item");
                    break;
                //放下
                case RobotOrderAnimData.Grab_item_pull:
                    //物品父节点放置在机器人手中
                    string objName2 = GetCurExecuteTask.objectName + "_" + GetCurExecuteTask.objectId;
                    if (MainData.CacheItemsEntity.ContainsKey(objName2))
                    {
                        grabObj = MainData.CacheItemsEntity[objName2];
                        animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_PutDown");
                    }
                    else
                    {
                        Debugger.Log("not find entity name:" + objName2);
                        animSecond = 0f;
                    }
                    break;
                //打开门
                case RobotOrderAnimData.Open_Door_Inside:
                    //case Order.Open_Door_Outside:
                    GameLogic.GetInstance.ListenerAllDoorOpenEvent(true);
                    //GameLogic.GetInstance.ListenerAllDoorCloseEvent(false);

                    Debugger.Log("openDoor", LogTag.Forever);
                    animSecond = m_RobotAnimCenter.PlayAnimByTrigger("Robot_Close_Door_Outside");
                    break;
                //关闭门
                case RobotOrderAnimData.Close_Door_Inside:
                    //case Order.Close_Door_Outside:
                    GameLogic.GetInstance.ListenerAllDoorCloseEvent(true);
                    //GameLogic.GetInstance.ListenerAllDoorOpenEvent(false);

                    Debugger.Log("closeDoor", LogTag.Forever);
                    animSecond = m_RobotAnimCenter.PlayAnimByTrigger("Robot_Close_Door_Inside");
                    //UnityTool.GetInstance.DelayCoroutine(1f, () =>
                    //{
                    //    m_RobotAnimCenter.PlayAnimByBool("CanInteraction", true);
                    //    m_RobotAnimCenter.PlayAnimByTrigger("Robot_Close_Door_Inside");
                    //    m_RobotAnimCenter.PlayAnimByBool("CanInteraction", false);
                    //}
                    //);
                    break;
                //擦桌子
                case RobotOrderAnimData.Robot_CleanTable:
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_CleanTable");
                    break;
                //操作阀门
                case RobotOrderAnimData.Wheel:
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_Wheel");
                    string key = GetCurExecuteTask.objectName + "_" + GetCurExecuteTask.objectId;
                    if (MainData.CacheItemsEntity.ContainsKey(key))
                    {
                        Animator wheelAnim = MainData.CacheItemsEntity[key].GetComponentInChildren<Animator>();
                        if (wheelAnim != null)
                        {
                            wheelAnim.Play("Wheel");
                        }
                        else
                        {
                            Debugger.LogError("wheelAnim is null");
                        }
                    }
                    else
                    {
                        Debugger.LogError("Wheel is null");
                    }
                    break;
                //充电
                case RobotOrderAnimData.Pile:
                    animSecond = 0.1f;
                    break;
                //蹲下拾取
                case RobotOrderAnimData.Pick_item:
                    //物品父节点放置在机器人手中
                    grabObj = GrabEntity(0.5f);
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_Pick");
                    break;
                //推
                //case Order.Push_End:
                //case Order.Push_Enter:
                //case Order.Push_Exit:
                //case Order.Push_Idle:
                //case Order.Push_Loop:
                //case Order.Push_Idle_inPlace:
                case RobotOrderAnimData.Push_Start:
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_Box_Push");
                    //箱子动画
                    BoxAnim(true);
                    break;
                //拉
                case RobotOrderAnimData.Pull_Start:
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_Box_Pull");
                    //箱子动画
                    BoxAnim(false);
                    break;
                //按下按钮
                case RobotOrderAnimData.Press_Button:
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_PressBtn");
                    break;
                //空闲姿态
                case RobotOrderAnimData.Idle:
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Idle");
                    break;
                //敲门
                case RobotOrderAnimData.Knock_on_door:
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_Knock_on_door");
                    break;
                //跳跃
                case RobotOrderAnimData.Jump:
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_Jump_in_Place");
                    break;
                //射箭
                case RobotOrderAnimData.CDA_Release:
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_CDA_Release");
                    break;
                //回旋踢
                case RobotOrderAnimData.Combat_Spinning_Kick:
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_Combat_Spinning_Kick");
                    break;
                //双手抱胸
                case RobotOrderAnimData.Hand_Chest:
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_Hand_Chest");
                    break;
                //双手叉腰
                case RobotOrderAnimData.Hand_Waist:
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_Hand_Waist");
                    break;
                //查找
                case RobotOrderAnimData.Find:
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_Find");
                    break;


                ////转动门把手
                //case Order.Turn_Door:
                //    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_Turn_Door");
                //    break;
                //倒水todo
                //case Order.:
                //    animSecond = robotAnimCenter.PlayAnimByName("");
                //    break;

                default:
                    Debug.LogError("当前指令动画未配置 orderAnim: " + orderStr + "，motionId:" + GetCurExecuteTask.motionId);
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_Other");
                    break;
            }
            m_RobotAnimCenter.PlayAnimByBool("CanInteraction", false);
            //UnityTool.GetInstance.DelayCoroutine(0.5f, () => m_RobotAnimCenter.PlayAnimByBool("CanInteraction", false));
            if (animSecond > 0)
            {
                UnityTool.GetInstance.DelayCoroutine(animSecond, () =>
                {
                    Debug.Log("play anim complete , orderStr :" + orderStr);
                    //取消物品父节点放置在机器人手中
                    if (orderStr == RobotOrderAnimData.Grab_item_pull)
                    {
                        if (grabObj != null && m_DicCacheGrabParent.ContainsKey(grabObj))
                        {
                            grabObj.transform.parent = m_DicCacheGrabParent[grabObj].transform;
                            foreach (var item in grabObj.GetComponentsInChildren<MeshCollider>())
                            {
                                item.enabled = true;
                            }
                            foreach (var item in grabObj.GetComponentsInChildren<NavMeshObstacle>())
                            {
                                item.enabled = true;
                            }
                            if (grabObj.GetComponentsInChildren<Rigidbody>() != null)
                            {
                                foreach (var item in grabObj.GetComponentsInChildren<Rigidbody>())
                                {
                                    item.useGravity = true;
                                    item.isKinematic = false;
                                }
                            }
                            m_DicCacheGrabParent.Remove(grabObj);
                        }
                    }
                });

                UnityTool.GetInstance.DelayCoroutine(animSecond + 1f, () =>
                {
                    animCompleteCallback?.Invoke();
                });
            }
            else
            {
                Debug.LogError("get animSecond fail , orderStr : " + orderStr);
            }
        }
        else
        {
            Debug.LogError("animSecond = robotAnimCenter is null");
        }
    }

    private void BoxAnim(bool isPush)
    {
        if (GetCurExecuteTask.objectName == "Box")
        {
            GameObject boxObj = GameObject.Find(GetCurExecuteTask.objectName + "_" + GetCurExecuteTask.objectId);
            GameObject player = GameObject.FindWithTag("Player");
            if (boxObj != null)
            {
                //判断离哪个节点近
                List<Transform> navNodes = boxObj.transform.Finds<Transform>("NavNode");
                float dicNavNode1 = Vector3.Distance(player.transform.position, navNodes[0].transform.position);
                float dicNavNode2 = Vector3.Distance(player.transform.position, navNodes[1].transform.position);
                Transform targetNavNode = dicNavNode1 < dicNavNode2 ? navNodes[0] : navNodes[1];
                Vector3 forceDir = targetNavNode.Find("forceDir").transform.localPosition;
                if (!isPush)
                {
                    forceDir = -forceDir;
                }
                //施加力
                UnityTool.GetInstance.DelayCoroutine(1.1f, () => boxObj.GetComponent<Rigidbody>().AddForce(forceDir * 1));
                UnityTool.GetInstance.DelayCoroutine(3.2f, () => boxObj.GetComponent<Rigidbody>().AddForce(forceDir * 3));
            }
        }
    }

    /// <summary>
    /// 拿取实体
    /// </summary>
    /// <param name="delayTime">延时设置实体父对象为机器人手臂节点</param>
    /// <returns></returns>
    private GameObject GrabEntity(float delayTime)
    {
        //物品父节点放置在机器人手中
        string objName1 = GetCurExecuteTask.objectName + "_" + GetCurExecuteTask.objectId;
        GameObject grabObj = null;
        if (MainData.CacheItemsEntity.ContainsKey(objName1))
        {
            grabObj = MainData.CacheItemsEntity[objName1];
            if (grabObj.GetComponentsInChildren<Rigidbody>() != null)
            {
                foreach (var item in grabObj.GetComponentsInChildren<Rigidbody>())
                {
                    item.useGravity = false;
                    item.isKinematic = true;
                }
            }
            GameObject model = grabObj.transform.Find<Transform>("Model")?.gameObject;
            if (model != null)
            {
                model.transform.localPosition = Vector3.zero;
                for (int i = 0; i < model.transform.childCount; i++)
                {
                    model.transform.GetChild(i).transform.localPosition = Vector3.zero;
                }
            }
            //当前物品的父对象实体
            UnityTool.GetInstance.DelayCoroutine(delayTime, () =>
            {
                Transform grabOldParentNode = grabObj.transform.parent;
                grabObj.transform.parent = m_RobotAnimCenter.RobotHandleNode;
                grabObj.transform.localPosition = Vector3.zero;
                grabObj.transform.localRotation = Quaternion.Euler(Vector3.zero);
                if (!m_DicCacheGrabParent.ContainsKey(grabObj))
                {
                    m_DicCacheGrabParent.Add(grabObj, grabOldParentNode);
                }
                //foreach (var item in grabObj.transform.Finds<Transform>("Model"))
                //{
                //    item.transform.localPosition = Vector3.zero;
                //}
                model.transform.localPosition = Vector3.zero;
                foreach (var item in grabObj.GetComponentsInChildren<MeshCollider>())
                {
                    item.enabled = false;
                }
                foreach (var item in grabObj.GetComponentsInChildren<NavMeshObstacle>())
                {
                    item.enabled = false;
                }

            });
        }
        else
        {
            Debug.LogError("obj is null ,objName: " + objName1);
        }
        return grabObj;
    }

    /// <summary>
    /// 任务执行失败回调
    /// </summary>
    /// <param name="stateCode"></param>
    private void TaskExecuteFail(Vector3 targetPos, int stateCode = 2, string stateMsg = "")
    {
        Debug.Log("任务执行失败回调 targetPos:" + targetPos);

        if (GetCurExecuteTask == null)
        {
            Debug.LogError("GetCurExecuteTask is null");
            return;
        }

        //改变机器人状态
        m_RobotAnimCenter.PlayAnimByBool("IsMoving", false);
        m_AIRobotMove.curRobotState = AIRobotMove.RobotBaseState.Idel;

        if (string.IsNullOrEmpty(stateMsg))
        {
            stateMsg = "dont arrive target pos , targetPos:" + targetPos;
        }
        //返回指令执行结果
        ControlResult controlResult = new ControlResult
        {
            motionId = GetCurExecuteTask.motionId,
            name = GetCurExecuteTask.name,
            task_id = GetCurExecuteTask.taskId,
            stateCode = stateCode,
            stateMsg = stateMsg,
            //targetRoomType = GetTargetRoomType().ToString()
            objectName = GetCurExecuteTask?.objectName
        };
        InterfaceDataCenter.GetInstance.SendMQTTControlResult(controlResult);
        IsExecuteTask = false;
        GetCurExecuteTask = null;
        m_TargetPos.Clear();

        if (m_CorLimitTask != null)
        {
            StopCoroutine(m_CorLimitTask);
            m_CorLimitTask = null;
        }

        m_AIRobotMove.ShowRobotPath(false);
    }

    /// <summary>
    /// 当前所在的房间，房间类型
    /// </summary>
    /// <returns></returns>
    private RoomType GetTargetRoomType()
    {
        return RoomType.TeaRoom;
    }

    private void Update()
    {
        ListenerTask();
    }

    /// <summary>
    /// 新增一条指令到队列
    /// </summary>
    /// <param name="controlCommitJsonStr"></param>
    public void TryAddOrder(string controlCommitJsonStr)
    {
        ControlCommit controlCommit = JsonTool.GetInstance.JsonToObjectByLitJson<ControlCommit>(controlCommitJsonStr);
        bool isRight = true;
        string taskFailDes = "";
        //判断当前指令是否合法
        if (controlCommit != null)
        {
            if (!string.IsNullOrEmpty(controlCommit.taskId))
            {
                if (controlCommit.sceneID == MainData.SceneID && controlCommit.tmpId == MainData.tmpID)
                {
                    isRight = true;
                }
                else
                {
                    isRight = false;
                    if (controlCommit.sceneID != MainData.SceneID)
                    {
                        taskFailDes = "新增决策指令失败 SceneID不匹配，忽视当前决策指令，curSceneID：" + MainData.SceneID + "，curTmpID：" + MainData.tmpID + "，cbSceneID：" + controlCommit.sceneID + "，cbTmpID：" + controlCommit.tmpId + ",json：" + controlCommitJsonStr;
                    }
                    else if (controlCommit.tmpId != MainData.tmpID)
                    {
                        taskFailDes = "新增决策指令失败 tmpId不匹配，忽视当前决策指令，curSceneID：" + MainData.SceneID + "，curTmpID：" + MainData.tmpID + "，cbSceneID：" + controlCommit.sceneID + "，cbTmpID：" + controlCommit.tmpId + ",json：" + controlCommitJsonStr;
                    }
                    Debugger.LogWarning(taskFailDes);
                    return;
                }
                //点击启动后的 对于同一个sceneID 的实例， 其taskId始终一致， 除非停止再开启
                //if (MainData.ControlCommitCompletedList.Find((p) => { return p.task_id == controlCommit.taskId; }) != null)
                //{
                //    isRight = false;
                //    taskFailDes = "新增决策指令失败,已完成过当前决策指令，id，task_id：" + controlCommit.taskId + ",json：" + controlCommitJsonStr;
                //}


            }
            else
            {
                taskFailDes = "新增决策指令失败,task_id is null , json：" + controlCommitJsonStr;
                isRight = false;
            }
        }
        else
        {
            taskFailDes = "新增决策指令失败,controlCommit is null , json：" + controlCommitJsonStr;
            isRight = false;
        }


        if (isRight)
        {
            MainData.ControlCommit.Enqueue(controlCommit);
            Debugger.Log("新增决策指令成功,当前指令缓存数量：" + MainData.ControlCommit.Count + " , json：" + controlCommitJsonStr, LogTag.Forever);
        }
        else
        {
            Debugger.LogError(taskFailDes, LogTag.Forever);
            TaskExecuteFail(Vector3.zero, 1, taskFailDes);
        }
    }

    /// <summary>
    /// 判断抓取、放下交互命令是否合法
    /// </summary>
    /// <param name="controlCommit"></param>
    /// <param name="taskFailDes"></param>
    /// <returns></returns>
    private bool JudgeGrabInterfaceOrder(ControlCommit controlCommit, ref string taskFailDes)
    {
        bool res = false;
        GameObject obj = GameObject.Find(controlCommit.objectName + "_" + controlCommit.objectId);
        if (obj != null)
        {
            //res = !obj.isStatic;
            res = !ItemStaticData.GetItemStatic(controlCommit.objectName);
            if (!res)
            {
                taskFailDes = "任务执行失败，无法进行抓取、放下交互，目标交互为非动态实体 , obj :" + controlCommit.objectName + "_" + controlCommit.objectId;
            }
        }
        else
        {
            res = false;
            taskFailDes = "任务执行失败，未找到所要交互的物体, obj :" + controlCommit.objectName + "_" + controlCommit.objectId;
        }
        Debugger.LogError("判断抓取、放下交互命令是否合法 res:" + res + ",obj: " + controlCommit.objectName + "_" + controlCommit.objectId);
        return res;
    }


    /// <summary>
    /// 监听任务
    /// </summary>
    public void ListenerTask()
    {
        if (CanExecuteTask && !IsExecuteTask)
        {
            if (MainData.ControlCommit?.Count > 0)
            {
                ControlCommit controlCommit = MainData.ControlCommit.Dequeue();
                ParseOrderExcute(controlCommit);

            }
        }
        if (IsExecuteTask)
        {
            m_CurTaskExeTime += Time.deltaTime;
        }
    }

    public void TestSendOrder(string orderName, string objName, string objID)
    {
        GameObject objModel = GameObject.Find(objName + "_" + objID)?.transform.Find("Model").gameObject;
        if (objModel == null)
        {
            Debugger.LogError("not find item , item :" + objName + "_" + objID);
            return;
        }
        string msg =
            @"
        {
            ""motionId"": ""motion://Knock_on_door"",
            ""name"": """ + orderName + "\"," + @"
            ""objectName"": """ + objName + "\"," + @"

            ""objectId"": "
+ "\"" + objID + "\"," +
@"""position"": ["

  + objModel.transform.position.x + "," + objModel.transform.position.y + "," + objModel.transform.position.z +

@"],
            ""rotation"": [
                0.0,
                0.0,
                0.0
            ],
            ""taskId"": ""task:grab1698110424418""
        }
                        ";
        Debugger.Log(msg);
        ControlCommit controlCommit = JsonTool.GetInstance.JsonToObjectByLitJson<ControlCommit>(msg);
        if (controlCommit != null)
        {
            MainData.ControlCommit.Enqueue(controlCommit);
            Debugger.Log("enqueue suc ,msg：" + msg);
        }
        else
        {
            Debugger.LogError("controlCommit is null");
        }
    }

}