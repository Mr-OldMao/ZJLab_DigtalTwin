using MFramework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
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
    public DoorState CurDoorState
    {
        get;
        private set;
    } = DoorState.Closed;

    public enum DoorState
    {
        Opened,
        Opening,
        Closed
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
                        Debug.Log("CollDown" + p.gameObject.tag);
                        if (CurDoorState == DoorState.Closed && p.gameObject.tag == "Player")
                        {
                            PlayDoorAnim("OpenDoorUp", true);
                        }
                    };
                    break;
                case "CollUp":
                    m_ListenerCollider1Arr[i].callbackTriggerEnter += (p) =>
                    {
                        Debug.Log("CollUp" + p.gameObject.tag);
                        if (CurDoorState == DoorState.Closed && p.gameObject.tag == "Player")
                        {
                            PlayDoorAnim("OpenDoorDown", true);
                        }
                    };
                    break;
                case "CollLeft":
                    m_ListenerCollider1Arr[i].callbackTriggerEnter += (p) =>
                    {
                        Debug.Log("CollLeft" + p.gameObject.tag);
                        if (CurDoorState == DoorState.Closed && p.gameObject.tag == "Player")
                        {
                            PlayDoorAnim("OpenDoorRight", true);
                        }
                    };
                    break;
                case "CollRight":
                    m_ListenerCollider1Arr[i].callbackTriggerEnter += (p) =>
                    {
                        Debug.Log("CollRight" + p.gameObject.tag);
                        if (CurDoorState == DoorState.Closed && p.gameObject.tag == "Player")
                        {
                            PlayDoorAnim("OpenDoorLeft", true);
                        }
                    };
                    break;
                default:
                    break;
            }

        }
    }

    private void PlayDoorAnim(string name, bool isOpen)
    {
        if (m_AnimDoor.GetBool(name) == isOpen)
        {
            return;
        }
        CurDoorState = DoorState.Opening;
        m_NavMeshObstacle.enabled = true;
        MsgEvent.SendMsg(MsgEventName.DoorAnimBegin);
        UnityTool.GetInstance.DelayCoroutine(0.2f, () =>
        {
            m_AnimDoor.SetBool(name, isOpen);
            
        });
        UnityTool.GetInstance.DelayCoroutine(1.2f, () =>
        {
            Debug.Log("doorAnim player complete");
            CurDoorState = isOpen ? DoorState.Opened : DoorState.Closed;
            m_NavMeshObstacle.enabled = isOpen;
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
