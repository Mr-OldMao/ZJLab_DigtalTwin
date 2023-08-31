using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 标题：碰撞检测
/// 功能：
/// 作者：毛俊峰
/// 时间：2023.08.31
/// </summary>
[RequireComponent(typeof(Collider))]
public class ListenerCollider : MonoBehaviour
{
    public delegate void CallbackTriggerEnter(Collider collider);
    public delegate void CallbackTriggerExit(Collider collider);
    public delegate void CallbackTriggerStay(Collider collider);
    public delegate void CallbackCollisionEnter(Collision collision);
    public delegate void CallbackCollisionExit(Collision collision);
    public delegate void CallbackCollisionStay(Collision collision);

    public CallbackTriggerEnter callbackTriggerEnter;
    public CallbackTriggerExit callbackTriggerExit;
    public CallbackTriggerStay callbackTriggerStay;
    public CallbackCollisionEnter callbackCollisionEnter;
    public CallbackCollisionExit callbackCollisionExit;
    public CallbackCollisionStay callbackCollisionStay;

    private Collider m_Collider;
    public bool IsTrigger
    {
        get { return m_Collider.isTrigger; }
        set { m_Collider.isTrigger = value; }
    }
    private void Awake()
    {
        m_Collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        callbackTriggerEnter?.Invoke(other);
    }
    private void OnTriggerStay(Collider other)
    {
        callbackTriggerStay?.Invoke(other);
    }
    private void OnTriggerExit(Collider other)
    {
        callbackTriggerExit?.Invoke(other);
    }
    private void OnCollisionEnter(Collision collision)
    {
        callbackCollisionEnter?.Invoke(collision);
    }
    private void OnCollisionStay(Collision collision)
    {
        callbackCollisionStay?.Invoke(collision);
    }
    private void OnCollisionExit(Collision collision)
    {
        callbackCollisionExit?.Invoke(collision);
    }
}
