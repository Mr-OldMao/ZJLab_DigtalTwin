using MFramework;
using System;
using UnityEngine;

/// <summary>
/// 描述：测试事件消息系统，消息的注册、注销、发送
/// 作者：毛俊峰
/// 时间：2022.08.22
/// 版本：1.0
/// </summary>
public class TestMsgEvent : MonoBehaviour
{
    private void Awake()
    {
        RegisterMsgEvent();
    }
    void Start()
    {
        SendMsg();
    }
    private void OnDestroy()
    {
        UnregisterMsgEvent();
    }
    private void Update()
    {
        //发送消息
        if (Input.GetKeyDown(KeyCode.Q))
        { 
            Debug.Log("Press Q");
            MsgEvent.SendMsg(MsgEventName.Test);
            //MsgEvent.SendMsg(MsgEventName.Test, "测试发送有参无返回值消息");
            //MsgEvent.SendMsg(MsgEventName.Test, (obj) => { Debug.Log("测试发送无参有返回值 res：" + obj); });
            //发送有参有返回值消息
            //MsgEvent.SendMsg(MsgEventName.Test, "1001", (obj) => { Debug.Log("测试发送有参有返回值 res：" + obj); });
        }
        //注销指定类型指定消息，仅针对非lamdba事件注册的消息使用
        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("Press W");
            MsgEvent.UnregisterMsgEvent(MsgEventName.Test, action);
            //MsgEvent.UnregisterMsgEvent(MsgEventName.Test, actionParam);
            //MsgEvent.UnregisterMsgEvent(MsgEventName.Test, func);
            //MsgEvent.UnregisterMsgEvent(MsgEventName.Test, funcParam);
        }
        //注销指定类型所有消息
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Press E");
            MsgEvent.UnregisterMsgEvent(MsgEventName.Test, MsgEventType.NoParamNoReturn);
            //MsgEvent.UnregisterMsgEvent(MsgEventName.Test, MsgEventType.HasParamNoReturn);
            //MsgEvent.UnregisterMsgEvent(MsgEventName.Test, MsgEventType.NoParamHasReturn);
            //MsgEvent.UnregisterMsgEvent(MsgEventName.Test, MsgEventType.HasParamHasReturn);
        }
        //注销所有类型所有消息
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Press R");
            MsgEvent.UnregisterMsgEvent(MsgEventName.Test);
        }
    }

    Action action;
    Action<object> actionParam;
    Func<object> func;
    Func<object, object> funcParam;
    /// <summary>
    /// 注册四类消息
    /// </summary>
    private void RegisterMsgEvent()
    {
        //无参无返回值
        MsgEvent.RegisterMsgEvent(MsgEventName.Test, () => { Debug.Log("无参、无返回、lamdba"); });
        action += () => { Debug.Log("无参无返回 action"); };
        MsgEvent.RegisterMsgEvent(MsgEventName.Test, action);

        //有参无返回值
        MsgEvent.RegisterMsgEvent(MsgEventName.Test, (object obj) => { Debug.Log("有参、无返回、lamdba obj：" + obj); });
        actionParam = (obj) => { Debug.Log("无参无返回 actionParam obj：" + obj); };
        MsgEvent.RegisterMsgEvent(MsgEventName.Test, actionParam);

        //无参有返回值
        MsgEvent.RegisterMsgEvent(MsgEventName.Test, () =>
        {
            Debug.Log("无参、有返回、lamdba");
            int value = 123;
            return value;
        });
        func = () =>
        {
            Debug.Log("无参、有返回、func");
            bool isVip = false;
            return isVip;
        };
        MsgEvent.RegisterMsgEvent(MsgEventName.Test, func);

        //有参有返回值
        MsgEvent.RegisterMsgEvent(MsgEventName.Test, (object friendID) =>
        {
            Debug.Log("有参、有返回、lamdba");
            string res = (string)friendID + "1111";
            return res;
        });
        funcParam = (object friendID) =>
        {
            Debug.Log("有参、有返回、funcParam");
            bool isVIP = false;
            if ((string)friendID == "1001")
            {
                isVIP = true;
            }
            return isVIP;
        };
        MsgEvent.RegisterMsgEvent(MsgEventName.Test, funcParam);
        Debug.Log("注册了四种消息事件，每种一个Lamdba事件、一个非Lamdba事件，共八个消息事件");
    }

    /// <summary>
    /// 发送消息(四类)
    /// </summary>
    private void SendMsg()
    {
        //发送无参无返回值消息
        MsgEvent.SendMsg(MsgEventName.Test);
        //发送有参无返回值消息
        MsgEvent.SendMsg(MsgEventName.Test, "测试发送有参无返回值消息");
        //发送无参有返回值消息
        MsgEvent.SendMsg(MsgEventName.Test, (obj) => { Debug.Log("测试发送无参有返回值 res：" + obj); });
        //发送有参有返回值消息
        MsgEvent.SendMsg(MsgEventName.Test, "1001", (obj) => { Debug.Log("测试发送有参有返回值 res：" + obj); });
    }

    /// <summary>
    /// 注销消息(九种)
    /// </summary>
    private void UnregisterMsgEvent()
    {
        //注销指定类型指定消息，仅针对非lamdba事件注册的消息使用
        MsgEvent.UnregisterMsgEvent(MsgEventName.Test, action);
        MsgEvent.UnregisterMsgEvent(MsgEventName.Test, actionParam);
        MsgEvent.UnregisterMsgEvent(MsgEventName.Test, func);
        MsgEvent.UnregisterMsgEvent(MsgEventName.Test, funcParam);

        //注销指定类型所有消息
        MsgEvent.UnregisterMsgEvent(MsgEventName.Test, MsgEventType.NoParamNoReturn);
        MsgEvent.UnregisterMsgEvent(MsgEventName.Test, MsgEventType.HasParamNoReturn);
        MsgEvent.UnregisterMsgEvent(MsgEventName.Test, MsgEventType.NoParamHasReturn);
        MsgEvent.UnregisterMsgEvent(MsgEventName.Test, MsgEventType.HasParamHasReturn);

        //注销所有类型所有消息
        MsgEvent.UnregisterMsgEvent(MsgEventName.Test);
    }
}
