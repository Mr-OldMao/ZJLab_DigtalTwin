using System.Runtime.InteropServices;
using UnityEngine;
using MFramework;
using System;
/// <summary>
/// 标题：用于Webgl平台使用mqtt通信
/// 功能：为外部提供mqtt的方面包含 mqtt连接、订阅消息、发送消息、取消订阅、断开连接、监听接收消息、取消监听接收消息
/// 作者：毛俊峰
/// 时间：2023.09.14
/// </summary>
public class MqttWebglCenter : SingletonByMono<MqttWebglCenter>
{
    [DllImport("__Internal")]
    private static extern void Jslib_Connect(string host, string port, string clientId, string username, string password, string destination);
    [DllImport("__Internal")]
    private static extern void Jslib_Subscribe(string topic);
    [DllImport("__Internal")]
    private static extern void Jslib_Publish(string topic, string payload);
    [DllImport("__Internal")]
    private static extern void Jslib_Unsubscribe(string topic);
    [DllImport("__Internal")]
    private static extern void Jslib_Disconnect();

    [DllImport("__Internal")]
    private static extern void Jslib_RefreshWebgl();


    private MqttRecvMsgCallback m_RecvMsgCallback;

    public void Connect(string clientIP, int clientPort, string clientId, string username, string password, string destination = "Unity_Test_Destination")
    {
        Jslib_Connect(clientIP, clientPort.ToString(), clientId, username, password, destination);
    }

    /// <summary>
    /// 订阅消息，为Unity提供
    /// </summary>
    /// <param name="topics"></param>
    public void Subscribe(params string[] topics)
    {
        foreach (string topic in topics)
        {
            Jslib_Subscribe(topic);
        }
    }

    /// <summary>
    /// 取消订消息，为Unity提供
    /// </summary>
    public void Unsubscribe(params string[] topics)
    {
        foreach (string topic in topics)
        {
            Jslib_Unsubscribe(topic);
        }
    }

    /// <summary>
    /// 监听订阅过的消息，为Unity提供
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="msg"></param>
    public void AddListenerSubscribe(MqttRecvMsgCallback mqttRecvMsgCallback)
    {
        m_RecvMsgCallback += mqttRecvMsgCallback;
    }

    /// <summary>
    /// 监听订阅过的消息，为Unity提供
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="msg"></param>
    public void RemoveListenerSubscribe(MqttRecvMsgCallback mqttRecvMsgCallback)
    {
        m_RecvMsgCallback -= mqttRecvMsgCallback;
    }

    /// <summary>
    /// 刷新Webgl html页面，为Unity提供
    /// </summary>
    public void RefreshWeb()
    {
        Debugger.Log("刷新Webgl html页面，为Unity提供");
        Jslib_RefreshWebgl();
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="msg"></param>
    public void Publish(string topic, string msg)
    {
        Jslib_Publish(topic, msg);
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public void DisConnect()
    {
        Jslib_Disconnect();
    }

    /// <summary>
    /// 接收订阅的消息，为外部h5提供api
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="msg"></param>
     void RecvMsg(string jsonStr)
    {
        string topic = jsonStr.Split('|')[0];
        string msg = jsonStr.Split('|')[1];
        //Debug.Log("[Unity] RecvMsg，topic：" + topic + "，msg：" + msg);
        m_RecvMsgCallback?.Invoke(topic, msg);
    }

    /// <summary>
    /// mqtt连接成功后回调
    /// </summary>
    public void ConnSuc()
    {
        //Debug.Log("[Unity] ConnSuc");
        NetworkMqtt.GetInstance. ConnSucCallbackHandle?.Invoke();
    }
}
