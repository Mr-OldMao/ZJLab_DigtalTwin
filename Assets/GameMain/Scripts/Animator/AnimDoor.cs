using MFramework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 标题：
/// 功能：
/// 作者：毛俊峰
/// 时间：2023.08.31
/// </summary>
public class AnimDoor : MonoBehaviour
{
    private Animator m_AnimDoor;
    private List<ListenerCollider> m_ListenerCollider1Arr = new List<ListenerCollider>();

    private NavMeshObstacle m_NavMeshObstacle;

    public bool CanOpenDoor
    {
        get;
        set;
    } = false;

    public bool CanCloseDoor
    {
        get;
        set;
    } = false;


    public DoorState CurDoorState
    {
        get;
        private set;
    } = DoorState.Closed;

    public enum DoorState
    {
        Opened,
        Opening,
        Closed,
        Closeding
    }
    private void Awake()
    {
        m_AnimDoor = GetComponentInChildren<Animator>();
        m_NavMeshObstacle = GetComponentInChildren<NavMeshObstacle>();
        GameObject[] doorColl = new GameObject[2] { transform.GetChild(1)?.gameObject, transform.GetChild(2)?.gameObject };
        foreach (var item in doorColl)
        {
            m_ListenerCollider1Arr.Add(item.AddComponent<ListenerCollider>());
        }
    }
    public void Start()
    {
        m_NavMeshObstacle.enabled = false;

        for (int i = 0; i < m_ListenerCollider1Arr.Count; i++)
        {
            m_ListenerCollider1Arr[i].IsTrigger = true;
            switch (m_ListenerCollider1Arr[i].gameObject.name)
            {
                case "CollDown":
                    m_ListenerCollider1Arr[i].callbackTriggerEnter += (p) =>
                    {
                        if (p.gameObject.tag != "Player") return;
                        //Debugger.Log("1111" + TaskCenter.GetInstance.GetCurExecuteTask.name + ",2" + Order.Turn_Door);
                        //Debugger.Log("1111" + TaskCenter.GetInstance.GetCurExecuteTask.name != Order.Turn_Door);
                        //Debugger.Log("2222" + string.Equals(TaskCenter.GetInstance.GetCurExecuteTask.name, Order.Turn_Door));
                        //Debugger.Log("333" + (transform.name != (TaskCenter.GetInstance.GetCurExecuteTask.name + "_" + TaskCenter.GetInstance.GetCurExecuteTask.motionId)));
                        //只要当前任务指令不是“敲门”,且不是指令所想要敲的那扇门“敲门”，都默认经过门口，机器人开门，门打开
                        //if (TaskCenter.GetInstance.GetCurExecuteTask == null || (TaskCenter.GetInstance.GetCurExecuteTask.name != Order.Knock_on_door && TaskCenter.GetInstance.GetCurExecuteTask.name != Order.Turn_Door))
                        if (TaskCenter.GetInstance.GetCurExecuteTask == null || TaskCenter.GetInstance.GetCurExecuteTask.name != RobotOrderAnimData.Knock_on_door
                        // ||(string.Equals(TaskCenter.GetInstance.GetCurExecuteTask.name, Order.Turn_Door) && transform.name != TaskCenter.GetInstance.GetCurExecuteTask .name+ "_"+ TaskCenter.GetInstance.GetCurExecuteTask.motionId)
                        )

                        //|| Vector3.Distance(GameObject.FindObjectOfType<AIRobotMove>().targetPoint.position
                        //, GameObject.FindObjectOfType<AIRobotMove>().transform.position) > 3f)
                        {
                            CanOpenDoor = true;
                            TryPlayAnim("OpenDoorUp", p.gameObject.tag, "CollDown");
                        }
                    };
                    m_ListenerCollider1Arr[i].callbackTriggerStay += (p) =>
                    {
                        if (p.gameObject.tag != "Player") return;
                        TryPlayAnim("OpenDoorUp", p.gameObject.tag, "CollDown");
                    };
                    break;
                case "CollUp":
                    m_ListenerCollider1Arr[i].callbackTriggerEnter += (p) =>
                    {
                        if (p.gameObject.tag != "Player") return;
                        if (TaskCenter.GetInstance.GetCurExecuteTask == null || TaskCenter.GetInstance.GetCurExecuteTask.name != RobotOrderAnimData.Knock_on_door)

                        //|| Vector3.Distance(GameObject.FindObjectOfType<AIRobotMove>().targetPoint.position
                        //, GameObject.FindObjectOfType<AIRobotMove>().transform.position) > 3f)
                        {
                            CanOpenDoor = true;
                            TryPlayAnim("OpenDoorDown", p.gameObject.tag, "CollUp");
                        }
                    };
                    m_ListenerCollider1Arr[i].callbackTriggerStay += (p) =>
                    {
                        if (p.gameObject.tag != "Player") return;
                        TryPlayAnim("OpenDoorDown", p.gameObject.tag, "CollUp");
                    };
                    break;
                case "CollLeft":
                    m_ListenerCollider1Arr[i].callbackTriggerEnter += (p) =>
                    {
                        if (p.gameObject.tag != "Player") return;
                        if (TaskCenter.GetInstance.GetCurExecuteTask == null || TaskCenter.GetInstance.GetCurExecuteTask.name != RobotOrderAnimData.Knock_on_door)

                        //|| Vector3.Distance(GameObject.FindObjectOfType<AIRobotMove>().targetPoint.position
                        //, GameObject.FindObjectOfType<AIRobotMove>().transform.position) > 3f)
                        {
                            CanOpenDoor = true;
                            TryPlayAnim("OpenDoorRight", p.gameObject.tag, "CollLeft");
                        }
                    };
                    m_ListenerCollider1Arr[i].callbackTriggerStay += (p) =>
                    {
                        if (p.gameObject.tag != "Player") return;
                        TryPlayAnim("OpenDoorRight", p.gameObject.tag, "CollLeft");
                    };
                    break;
                case "CollRight":
                    m_ListenerCollider1Arr[i].callbackTriggerEnter += (p) =>
                    {
                        if (p.gameObject.tag != "Player") return;
                        if (TaskCenter.GetInstance.GetCurExecuteTask == null || TaskCenter.GetInstance.GetCurExecuteTask.name != RobotOrderAnimData.Knock_on_door)

                        //|| Vector3.Distance(GameObject.FindObjectOfType<AIRobotMove>().targetPoint.position
                        //, GameObject.FindObjectOfType<AIRobotMove>().transform.position) > 3f)
                        {
                            CanOpenDoor = true;
                            TryPlayAnim("OpenDoorLeft", p.gameObject.tag, "CollRight");
                        }
                    };
                    m_ListenerCollider1Arr[i].callbackTriggerStay += (p) =>
                    {
                        if (p.gameObject.tag != "Player") return;
                        TryPlayAnim("OpenDoorLeft", p.gameObject.tag, "CollRight");
                    };
                    break;
                default:
                    break;
            }

        }
    }

    private void TryPlayAnim(string animName, string tag, string selfName)
    {
        if (CanOpenDoor)
        {
            if (CurDoorState == DoorState.Closed)
            {
                Debugger.Log("selfName:" + selfName);
                PlayDoorAnim(animName, true);
            }
        }
        if (CanCloseDoor)
        {
            if (CurDoorState == DoorState.Opened)
            {
                Debugger.Log("selfName:" + selfName);
                PlayDoorAnim(animName, false);
            }
        }
    }

    private void PlayDoorAnim(string animName, bool isOpenDoor)
    {
        if (m_AnimDoor.GetBool(animName) == isOpenDoor)
        {
            return;
        }
        if (CurDoorState == DoorState.Opening || CurDoorState == DoorState.Closeding)
        {
            return;
        }
        CurDoorState = isOpenDoor ? DoorState.Opening : DoorState.Closeding;
        m_NavMeshObstacle.enabled = true;
        MsgEvent.SendMsg(MsgEventName.DoorAnimBegin);
        UnityTool.GetInstance.DelayCoroutine(0.2f, () =>
        {
            m_AnimDoor.SetBool(animName, isOpenDoor);

        });
        UnityTool.GetInstance.DelayCoroutine(1.5f, () =>
        {
            Debug.Log("doorAnim player complete");
            CurDoorState = isOpenDoor ? DoorState.Opened : DoorState.Closed;
            m_NavMeshObstacle.enabled = isOpenDoor;
            CanOpenDoor = false;
            CanCloseDoor = false;
            MsgEvent.SendMsg(MsgEventName.DoorAnimEnd);
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            m_AnimDoor.SetBool("OpenDoorUp", false);
            m_AnimDoor.SetBool("OpenDoorDown", false);
            m_AnimDoor.SetBool("OpenDoorLeft", false);
            m_AnimDoor.SetBool("OpenDoorRight", false);
            m_NavMeshObstacle.enabled = false;
            CurDoorState = DoorState.Closed;
        }
    }
}
