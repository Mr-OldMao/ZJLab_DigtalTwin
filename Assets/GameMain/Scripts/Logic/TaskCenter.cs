using System.Collections.Generic;
using UnityEngine;
using MFramework;
using System;
using UnityEngine.AI;
using UnityEngine.UIElements;
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
        Vector3 targetPos = Vector3.zero;
        Vector3 targetRot = Vector3.zero;

        //通过id查要走到的位置
        //GameObject objModel = null;

        List<Transform> navNodes = GameObject.Find(controlCommit.objectName + "_" + controlCommit.objectId)?.transform.Finds<Transform>("NavNode");

        foreach (var item in navNodes)
        {
            NavNodeData navNodeData = new NavNodeData { pos = item.position, rot = item.rotation.eulerAngles };
            m_TargetPos.Add(navNodeData);
            Debugger.Log("add " + navNodeData.pos);
        }

        //GameObject rootNode = GameObject.Find(controlCommit.objectName + "_" + controlCommit.objectId);
        //for (int i = 0; i < rootNode?.transform.childCount; i++)
        //{
        //    if (rootNode?.transform.GetChild(i).name == "NavNode")
        //    {
        //        m_TargetPos.Add(rootNode?.transform.GetChild(i).gameObject);
        //        Debugger.Log(rootNode?.transform.GetChild(i).gameObject.transform.position);
        //    }
        //}

        bool isFindNavNode = false;
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
                motionId = GetCurExecuteTask.motionId,
                name = GetCurExecuteTask.name,
                task_id = GetCurExecuteTask.taskId,
                simulatorId = GetCurExecuteTask.simulatorId,
                stateCode = 0,
                stateMsg = "suc",
                targetRommType = GetTargetRoomType().ToString()
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
                case Order.WALK:
                    break;
                //拿取
                case Order.Grab_item:
                    //物品父节点放置在机器人手中
                    grabObj = GrabEntity(1f);
                    //string objName1 = GetCurExecuteTask.objectName + "_" + GetCurExecuteTask.objectId;
                    //grabObj = MainData.CacheItemsEntity[objName1];
                    //if (grabObj != null)
                    //{
                    //    //当前物品的父对象实体
                    //    Transform grabOldParentNode = grabObj.transform.parent;
                    //    grabObj.transform.parent = m_RobotAnimCenter.RobotHandleNode;
                    //    grabObj.transform.localPosition = Vector3.zero;
                    //    grabObj.transform.localRotation = Quaternion.Euler(Vector3.zero);
                    //    if (!m_DicCacheGrabParent.ContainsKey(grabObj))
                    //    {
                    //        m_DicCacheGrabParent.Add(grabObj, grabOldParentNode);
                    //    }
                    //    foreach (var item in grabObj.transform.Finds<Transform>("Model"))
                    //    {
                    //        item.transform.localPosition = Vector3.zero;
                    //    }
                    //    foreach (var item in grabObj.GetComponentsInChildren<MeshCollider>())
                    //    {
                    //        item.enabled = false;
                    //    }
                    //    foreach (var item in grabObj.GetComponentsInChildren<NavMeshObstacle>())
                    //    {
                    //        item.enabled = false;
                    //    }
                    //}
                    //else
                    //{
                    //    Debug.LogError("obj is null ,objName: " + objName1);
                    //}
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_Grab_item");
                    break;
                //放下
                case Order.Grab_item_pull:
                    //物品父节点放置在机器人手中
                    string objName2 = GetCurExecuteTask.objectName + "_" + GetCurExecuteTask.objectId;
                    grabObj = MainData.CacheItemsEntity[objName2];
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_PutDown");
                    break;
                //打开门
                case Order.Open_Door_Inside:
                case Order.Open_Door_Outside:
                    GameLogic.GetInstance.ListenerAllDoorOpenEvent(true);

                    Debugger.Log("openDoor", LogTag.Forever);
                    animSecond = m_RobotAnimCenter.PlayAnimByTrigger("Robot_Close_Door_Outside");
                    break;
                //关闭门
                case Order.Close_Door_Inside:
                case Order.Close_Door_Outside:
                    GameLogic.GetInstance.ListenerAllDoorCloseEvent(true);
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
                case Order.Robot_CleanTable:
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_CleanTable");
                    break;
                //操作阀门
                case Order.Wheel:
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
                case Order.Pile:
                    animSecond = 0.1f;
                    break;
                //蹲下拾取
                case Order.Pick_item:
                    //物品父节点放置在机器人手中
                    grabObj = GrabEntity(0.5f);
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_Pick");
                    break;
                //推
                case Order.Push_End:
                case Order.Push_Enter:
                case Order.Push_Exit:
                case Order.Push_Idle:
                case Order.Push_Loop:
                case Order.Push_Idle_inPlace:
                case Order.Push_Start:
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_Box_Push");
                    //箱子动画 todo

                    break;
                //拉
                case Order.Pull_Start:
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_Box_Pull");
                    //箱子动画 todo

                    break;
                //按下按钮
                case Order.Press_Button:
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_PressBtn");
                    break;
                //空闲姿态
                case Order.IDLE:
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Idle");
                    break;
                //敲门
                case Order.Knock_on_door:
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_Knock_on_door");
                    break;
                //跳跃
                case Order.Jump:
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_Jump_in_Place");
                    break;
                //射箭
                case Order.CDA_Release:
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_CDA_Release");
                    break;
                //回旋踢
                case Order.Combat_Spinning_Kick:
                    animSecond = m_RobotAnimCenter.PlayAnimByName("Robot_Combat_Spinning_Kick");
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
                    if (orderStr == Order.Grab_item_pull)
                    {
                        if (m_DicCacheGrabParent.ContainsKey(grabObj))
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
                            m_DicCacheGrabParent.Remove(grabObj);
                        }
                    }
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

    /// <summary>
    /// 拿取实体
    /// </summary>
    /// <param name="delayTime">延时设置实体父对象为机器人手臂节点</param>
    /// <returns></returns>
    private GameObject GrabEntity(float delayTime)
    {
        //物品父节点放置在机器人手中
        string objName1 = GetCurExecuteTask.objectName + "_" + GetCurExecuteTask.objectId;
        GameObject grabObj = MainData.CacheItemsEntity[objName1];
        if (grabObj != null)
        {
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
                foreach (var item in grabObj.transform.Finds<Transform>("Model"))
                {
                    item.transform.localPosition = Vector3.zero;
                }
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
            targetRommType = GetTargetRoomType().ToString()
        };
        InterfaceDataCenter.GetInstance.SendMQTTControlResult(controlResult);
        IsExecuteTask = false;
        GetCurExecuteTask = null;
        m_TargetPos.Clear();

        m_AIRobotMove.ShowRobotPath(false);
    }

    /// <summary>
    /// 当前所在的房间，房间类型
    /// </summary>
    /// <returns></returns>
    private RoomType GetTargetRoomType()
    {
        return default;
    }

    private void Update()
    {
        ListenerTask();
    }

    /// <summary>
    /// 新增一条指令到队列
    /// </summary>
    /// <param name="controlCommitJsonStr"></param>
    public void AddOrder(string controlCommitJsonStr)
    {
        ControlCommit controlCommit = JsonTool.GetInstance.JsonToObjectByLitJson<ControlCommit>(controlCommitJsonStr);
        bool isRight = true;
        string taskFailDes = "";
        //判断当前指令是否合法
        if (controlCommit != null)
        {
            if (!string.IsNullOrEmpty(controlCommit.taskId))
            {
                if (controlCommit.sceneID == MainData.SceneID || controlCommit.simulatorId == MainData.SceneID)
                {
                    isRight = true;
                }
                else
                {
                    isRight = false;
                    taskFailDes = "SceneID不匹配，忽视当前决策指令，curSceneID：" + MainData.SceneID + "，simulatorId：" + controlCommit.simulatorId + "，sceneID：" + controlCommit.sceneID + ",json：" + controlCommitJsonStr;
                    Debugger.LogWarning(taskFailDes);
                    return;
                }
                if (MainData.ControlCommitCompletedList.Find((p) => { return p.task_id == controlCommit.taskId; }) != null)
                {
                    isRight = false;
                    taskFailDes = "新增决策指令失败,已完成过当前决策指令，id，task_id：" + controlCommit.taskId + ",json：" + controlCommitJsonStr;
                }
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
            Debugger.Log("新增决策指令成功 , json：" + controlCommitJsonStr, LogTag.Forever);
        }
        else
        {
            Debugger.LogError(taskFailDes, LogTag.Forever);
            TaskExecuteFail(Vector3.zero, 1, taskFailDes);
        }
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

public enum RobotTaskState
{

}

class Order
{
    public const string Open_Door_Inside = "Open_Door_Inside";
    public const string Open_Door_Outside = "Open_Door_Outside";
    public const string Close_Door_Inside = "Close_Door_Inside";
    public const string Close_Door_Outside = "Close_Door_Outside";
    public const string Drink = "Drink";
    public const string Grab_item = "Grab_item";
    public const string Grab_item_pull = "Grab_item_pull";
    public const string Heal_bandages = "Heal_bandages";
    public const string Knock_on_door = "Knock_on_door";
    public const string Lever_Floor_Pull = "Lever_Floor_Pull";
    public const string Lever_Floor_Push = "Lever_Floor_Push";
    public const string Lever_Wall_Pull = "Lever_Wall_Pull";
    public const string Lever_Wall_Push = "Lever_Wall_Push";
    public const string Pick_item = "Pick_item";
    public const string Press_Button = "Press_Button";
    public const string Press_Loop = "Press_Loop";
    public const string Push_End = "Push_End";
    public const string Push_Enter = "Push_Enter";
    public const string Push_Exit = "Push_Exit";
    public const string Push_Idle = "Push_Idle";
    public const string Push_Idle_inPlace = "Push_Idle_inPlace";
    public const string Push_Loop = "Push_Loop";
    public const string Push_Start = "Push_Start";

    public const string Pull_Start = "Pull_Start";
    public const string IDLE = "IDLE";
    public const string WALK = "WALK";

    public const string Wheel = "Wheel";
    public const string Robot_CleanTable = "Robot_CleanTable";
    public const string Pile = "Pile";
    //public const string Turn_Door = "Turn_Door";

    public const string Jump = "Jump";
    public const string CDA_Release = "CDA_Release";
    public const string Combat_Spinning_Kick = "Combat_Spinning_Kick";
}
