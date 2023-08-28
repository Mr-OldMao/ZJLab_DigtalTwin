using MFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using static GetThingGraph;
using static System.Net.WebRequestMethods;
using static UnityEditor.Progress;
using static UnityEditor.VersionControl.Asset;
/// <summary>
/// 标题：接口数据管理中心
/// 功能：处理HTTP接口的下发，数据的缓存，处理MQTT协议数据的订阅监听，数据下发
/// 作者：毛俊峰
/// 时间：2023.08.21
/// </summary>
public class InterfaceDataCenter : SingletonByMono<InterfaceDataCenter>
{
    private const string URL_SUBROOT = "http://10.11.81.241:4006/";

    //获取场景图，物体与房间的邻接关系
    private const string URL_GET_THING_GRAPH = URL_SUBROOT + "simulator/getThingGraph";

    //提交场景图，物体与房间的邻接关系
    private const string URL_POST_THING_GRAPH = URL_SUBROOT + "simulator/postThingGraph";

    //获取环境场景图,房间与房间的邻接关系
    private const string URL_GET_ENV_GRAPH = URL_SUBROOT + "simulator/getEnvGraph";

    //改变仿真引擎状态
    private const string URL_CHANGE_SIMULATOR_STATE = URL_SUBROOT + "simulator/changeSimulatorState";

    /*MQTT*/
    //更新全局场景图
    private const string TOPIC_GLOBAL = "/simulator/thingGraph/global";
    //更新相机视⻆场景图
    private const string TOPIC_CAMERA = "/simulator/thingGraph/camera";
    //接收服务器控制指令
    private const string TOPIC_SEND = "simulator/send";
    //发控制结果给服务器
    public const string TOPIC_RECV = "simulator/recv";

    #region HTTP
    /// <summary>
    /// 缓存场景图，物体与房间的邻接关系
    /// </summary>
    /// <param name="id">仿真引擎实例的id号码</param>
    public void CacheGetThingGraph(string id)
    {
        string rawJsonStr = "{\"id\":\"" + id + "\"}";
        MFramework.NetworkHttp.GetInstance.SendRequest(RequestType.Post, URL_GET_THING_GRAPH, new Dictionary<string, string>(), (string jsonStr) =>
        {
            MainData.getThingGraph = JsonTool.GetInstance.JsonToObjectByLitJson<GetThingGraph>(jsonStr);
            Debug.Log("缓存场景图，物体与房间的邻接关系 jsonStr:" + jsonStr);
        }, null, rawJsonStr);
    }

    /// <summary>
    /// 提交场景图，物体与房间的邻接关系
    /// </summary>
    /// <param name="items"></param>
    public void CommitGetThingGraph(PostThingGraph items, Action callback)
    {
        string jsonStr = JsonTool.GetInstance.ObjectToJsonStringByLitJson(items);
        Debug.Log("提交场景图，物体与房间的邻接关系 jsonStr:" + jsonStr);
        MFramework.NetworkHttp.GetInstance.SendRequest(RequestType.Post, URL_POST_THING_GRAPH, new Dictionary<string, string>(), (string jsonStr) =>
        {
            Debug.Log("提交场景图，物体与房间关系回调 jsonStr:" + jsonStr);
            callback?.Invoke();
        }, null, jsonStr);
    }

    /// <summary>
    /// 缓存环境场景图,房间与房间的邻接关系
    /// </summary>
    /// <param name="id">仿真引擎实例的id号码</param>
    public void CacheGetEnvGraph(string id)
    {
        string rawJsonStr = "{\"id\":\"" + id + "\"}";
        MFramework.NetworkHttp.GetInstance.SendRequest(RequestType.Post, URL_GET_ENV_GRAPH, new Dictionary<string, string>(), (string jsonStr) =>
        {
            MainData.getEnvGraph = JsonTool.GetInstance.JsonToObjectByLitJson<GetEnvGraph>(jsonStr);
            Debug.Log("缓存环境场景图,房间与房间的邻接关系 jsonStr:" + jsonStr);
        }, null, rawJsonStr);
    }

    /// <summary>
    /// 改变仿真引擎状态
    /// </summary>
    /// <param name="id">仿真引擎实例的id号码</param>    
    /// <param name="state">仿真引擎状态标识</param>
    public void ChangeProgramState(string id, ProgramState state)
    {
        string rawJsonStr = "{  \"id\":\"" + id + "\"  ,\"state\":\"" + state.ToString() + "\" }";
        MFramework.NetworkHttp.GetInstance.SendRequest(RequestType.Post, URL_CHANGE_SIMULATOR_STATE, new Dictionary<string, string>(), (string jsonStr) =>
        {
            Debug.Log("改变仿真引擎状态 jsonStr:" + jsonStr);
        }, null, rawJsonStr);
    }
    #endregion


    #region MQTT
    public void InitMQTT()
    {
        //初始化并订阅主题tcp://10.5.24.28:1883
        NetworkMqtt.GetInstance.Init(new MqttConfig()
        {
            clientIP = "10.5.24.28",
            clientPort = 1883
        }).Subscribe(TOPIC_GLOBAL, TOPIC_CAMERA, TOPIC_SEND, TOPIC_RECV);
        //监听消息回调
        NetworkMqtt.GetInstance.AddListener((object sender, MqttMsgPublishEventArgs e) =>
        {
            MqttClient mqttClient = sender as MqttClient;
            string jsonStr = Encoding.UTF8.GetString(e.Message);
            Debug.Log($"recv mqtt callback. topic：{e.Topic}， msg：{jsonStr}，ClientId：{mqttClient.ClientId}");
            switch (e.Topic)
            {
                case TOPIC_SEND:
                    ControlCommit controlCommit = JsonTool.GetInstance.JsonToObjectByLitJson<ControlCommit>(jsonStr);
                    if (controlCommit != null)
                    {
                        MainData.controlCommit.Add(controlCommit);
                        Debug.Log("TODO 解析指令，根据服务器决策指令，控制机器人的行为 " + controlCommit);
                    }
                    else
                    {
                        Debug.LogError("controlCommit is null");
                    }
                    break;
                default:
                    Debug.LogError($"Other Topoc :{e.Topic}，msg:{jsonStr} ");
                    break;
            }
        });
        NetworkMqtt.GetInstance.AddListener((object sender, MqttMsgSubscribedEventArgs e) =>
        {
            Debug.Log($"客户端订阅消息成功回调 ，sender：{sender}");
        });
    }

    /// <summary>
    /// 更新全局场景图
    /// </summary>
    /// <param name="items"></param>
    public void SendMQTTUpdateScenes(GetThingGraph_data_items[] items)
    {
        string jsonStr = JsonTool.GetInstance.ObjectToJsonStringByLitJson(items);
        NetworkMqtt.GetInstance.Publish(TOPIC_GLOBAL, jsonStr);
    }

    /// <summary>
    /// 更新相机视⻆场景图
    /// </summary>
    /// <param name="items"></param>
    public void SendMQTTUpdateCamera(GetThingGraph_data_items[] items)
    {
        string jsonStr = JsonTool.GetInstance.ObjectToJsonStringByLitJson(items);
        NetworkMqtt.GetInstance.Publish(TOPIC_CAMERA, jsonStr);
    }
    #endregion


    /// <summary>
    /// 发送控制结果
    /// </summary>
    /// <param name="controlResult"></param>
    public void SendMQTTControlResult(ControlResult controlResult)
    {
        string jsonStr = JsonTool.GetInstance.ObjectToJsonStringByLitJson(controlResult);
        NetworkMqtt.GetInstance.Publish(TOPIC_RECV, jsonStr);
    }
}

/// <summary>
/// 程序状态标识
/// </summary>
public enum ProgramState
{
    start,
    pause,
    resume,
    stop
}

/// <summary>
/// MQTT 控制结果数据结构
/// </summary>
public class ControlResult
{
    /// <summary>
    /// 动作或者控制id
    /// </summary>
    public string motionId;
    /// <summary>
    /// 控制指令名称
    /// </summary>
    public string name;
    /// <summary>
    /// 返回状态的说明信息
    /// </summary>
    public string stateMsg;
    /// <summary>
    /// 返回状态0: ok - 1: ⽆法执⾏ -2: ⽆法导航 -3：⽆法执⾏ ⽆法导航
    /// </summary>
    public int stateCode;
    /// <summary>
    /// 仿真实例id，具有唯⼀性
    /// </summary>
    public string simulatorId;
    /// <summary>
    /// 任务id，具有唯⼀性
    /// </summary>
    public string task_id;
}

/// <summary>
/// MQTT 控制指令数据结构
/// </summary>
public class ControlCommit
{
    /// <summary>
    /// 动作或者控制id
    /// </summary>
    public string motionId;
    /// <summary>
    /// 控制指令名称
    /// </summary>
    public string name;
    /// <summary>
    /// ⽤于参照的参数名称表
    /// </summary>
    public string[] vars;
    /// <summary>
    /// 导航到⽬标的坐标x y z
    /// </summary>
    public float[] position;
    /// <summary>
    /// 导航到⽬标的欧拉⻆ r y p
    /// </summary>
    public float[] rotation;
    /// <summary>
    /// 重点感知物体\类⽬的名称列表
    /// </summary>
    public string[] perception;
    /// <summary>
    /// 操作⽬标物体的id
    /// </summary>
    public string obj;
    /// <summary>
    /// 仿真实例id，具有唯⼀性
    /// </summary>
    public string simulatorId;
    /// <summary>
    /// 任务id，具有唯⼀性
    /// </summary>
    public string task_id;
}