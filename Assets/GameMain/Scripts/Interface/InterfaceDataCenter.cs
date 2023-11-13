using MFramework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityGameFramework.Runtime;
using static GetEnvGraph;
using static GetThingGraph;
/// <summary>
/// 标题：接口数据管理中心
/// 功能：处理HTTP接口的下发，数据的缓存，处理MQTT协议数据的订阅监听，数据下发
/// 作者：毛俊峰
/// 时间：2023.08.21
/// </summary>
public class InterfaceDataCenter : SingletonByMono<InterfaceDataCenter>
{
    //服务器10.101.80.21   本机10.11.81.241
    private static string URL_SUBROOT = "http://" + MainData.ConfigData?.HttpConfig.IP + ":" + MainData.ConfigData?.HttpConfig.Port + "/";//"http://10.101.80.21:4006/";

    //获取场景图，物体与房间的邻接关系
    private static string URL_GET_THING_GRAPH = URL_SUBROOT + "simulator/getThingGraph";

    //提交场景图，物体与房间的邻接关系
    private static string URL_POST_THING_GRAPH = URL_SUBROOT + "simulator/postThingGraph";

    //获取环境场景图,房间与房间的邻接关系
    private static string URL_GET_ENV_GRAPH = URL_SUBROOT + "simulator/getEnvGraph";

    //改变仿真引擎状态
    private static string URL_CHANGE_SIMULATOR_STATE = URL_SUBROOT + "simulator/changeSimulatorState";

    /*MQTT*/
    #region 仿真
    //更新全局场景图
    private const string TOPIC_GLOBAL = "/simulator/thingGraph/global";
    //更新相机视⻆场景图
    private const string TOPIC_CAMERA = "/simulator/thingGraph/camera";
    //接收服务器控制指令
    private const string TOPIC_SEND = "simulator/send";
    //发控制结果给服务器
    public const string TOPIC_RECV = "simulator/recv";

    //发送房间信息
    public const string TOPIC_ROOMINFODATA = "simulator/roomInfoData";
    //引擎状态
    public const string TOPIC_CHANGESTATE = "simulator/changeState";
    //直播流信息
    public const string TOPIC_LIVEDATA = "simulator/liveStreaming";
    //新增房间实体模型
    public const string TOPIC_ADD_GOODS = "simulator/addGoods";
    //删除房间实体模型
    public const string TOPIC_DEL_GOODS = "simulator/delGoods";

    //测试从Web端 接收服务器控制指令
    public const string TOPIC_WEB_SEND = "simulator/web/send";
    //测试 发控制结果给Web端
    public const string TOPIC_WEB_RECV = "simulator/web/recv";
    #endregion

    #region 数字孪生
    //访客坐标信息
    public const string TOPIC_PEOPLE_PERCEPTION = "feature/people_perception";
    //机器人坐标信息
    public const string TOPIC_ROBOT_POS = "feature/robot_pos";
    #endregion

    #region HTTP
    /// <summary>
    /// 缓存场景图，物体与房间的邻接关系
    /// </summary>
    /// <param name="id">仿真引擎实例的id号码</param>
    public void CacheGetThingGraph(string id)
    {
        if (!MainData.CanReadFile)
        {
            string rawJsonStr = "{\"id\":\"" + id + "\"}";
            Debugger.Log("尝试获取缓存场景图，物体与房间的邻接关系 " + rawJsonStr);
            MFramework.NetworkHttp.GetInstance.SendRequest(RequestType.Post, URL_GET_THING_GRAPH, new Dictionary<string, string>(), (string jsonStr) =>
            {
                MainData.getThingGraph = JsonTool.GetInstance.JsonToObjectByLitJson<GetThingGraph>(jsonStr);
                Debugger.Log("已获取场景图，缓存物体与房间的邻接关系 jsonStr:" + jsonStr);
                //存档
                DataSave.GetInstance.SaveGetThingGraph_data_items(new PostThingGraph
                {
                    id = MainData.SceneID,
                    idScene = MainData.SceneID,
                    items = MainData.getThingGraph.data.items
                });
            }, null, rawJsonStr);
        }
        else
        {
            //读档
            PostThingGraph postThingGraph = DataRead.GetInstance.ReadGetThingGraph_data_items();
            MainData.getThingGraph = new GetThingGraph
            {
                data = new GetThingGraph_data
                {
                    items = postThingGraph.items
                }
            };
        }


    }

    /// <summary>
    /// 提交场景图，物体与房间的邻接关系
    /// </summary>
    /// <param name="items"></param>
    public void CommitGetThingGraph(PostThingGraph items, Action callback)
    {
        string rawJsonStr = JsonTool.GetInstance.ObjectToJsonStringByLitJson(items);
        Debugger.Log("提交场景图，物体与房间的邻接关系 " + rawJsonStr);
        MFramework.NetworkHttp.GetInstance.SendRequest(RequestType.Post, URL_POST_THING_GRAPH, new Dictionary<string, string>(), (string jsonStr) =>
        {
            Debugger.Log("提交场景图，物体与房间关系回调 jsonStr:" + jsonStr);
            callback?.Invoke();
        }, null, rawJsonStr, (m, n) =>
        {
            Debugger.LogError("提交场景图失败，m:" + m + ",n:" + n);
        });


    }

    /// <summary>
    /// 缓存环境场景图,房间与房间的邻接关系
    /// </summary>
    /// <param name="id">仿真引擎实例的id号码</param>
    public void CacheGetEnvGraph(string id)
    {
        string rawJsonStr = "{\"id\":\"" + id + "\"}";
        Debugger.Log("缓存环境场景图,房间与房间的邻接关系 " + rawJsonStr);

        if (!MainData.CanReadFile)
        {
            MFramework.NetworkHttp.GetInstance.SendRequest(RequestType.Post, URL_GET_ENV_GRAPH, new Dictionary<string, string>(), (string jsonStr) =>
            {
                if (MainData.UseTestData)
                {
                    //Test测试数据
                    jsonStr = "{\r\n    \"code\": 200,\r\n    \"message\": \"good\",\r\n    \"success\": true,\r\n    \"data\": {\r\n        \"items\": [\r\n            {\r\n                \"id\": \"sim:1\",\r\n                \"name\": \"LivingRoom\",\r\n                \"relatedThing\": [\r\n                    {\r\n                        \"target\": {\r\n                            \"id\": \"sim:2\",\r\n                            \"name\": \"BathRoom\",\r\n                            \"relatedThing\": []\r\n                        },\r\n                        \"relationship\": \"Left\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"id\": \"sim:3\",\r\n                            \"name\": \"LivingRoom\",\r\n                            \"relatedThing\": []\r\n                        },\r\n                        \"relationship\": \"Right\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"id\": \"sim:4\",\r\n                            \"name\": \"KitchenRoom\",\r\n                            \"relatedThing\": []\r\n                        },\r\n                        \"relationship\": \"Top\"\r\n                    }\r\n                ]\r\n            },\r\n            {\r\n                \"id\": \"sim:5\",\r\n                \"name\": \"BedRoom\",\r\n                \"relatedThing\": [\r\n                    {\r\n                        \"target\": {\r\n                            \"id\": \"sim:1\",\r\n                            \"name\": \"LivingRoom\",\r\n                            \"relatedThing\": []\r\n                        },\r\n                        \"relationship\": \"Top\"\r\n                    }\r\n                ]\r\n            },\r\n            {\r\n                \"id\": \"sim:6\",\r\n                \"name\": \"StorageRoom\",\r\n                \"relatedThing\": [\r\n                    {\r\n                        \"target\": {\r\n                            \"id\": \"sim:3\",\r\n                            \"name\": \"LivingRoom\",\r\n                            \"relatedThing\": []\r\n                        },\r\n                        \"relationship\": \"Top\"\r\n                    }\r\n                ]\r\n            },\r\n            {\r\n                \"id\": \"sim:7\",\r\n                \"name\": \"StudyRoom\",\r\n                \"relatedThing\": [\r\n                    {\r\n                        \"target\": {\r\n                            \"id\": \"sim:3\",\r\n                            \"name\": \"LivingRoom\",\r\n                            \"relatedThing\": []\r\n                        },\r\n                        \"relationship\": \"Left\"\r\n                    }\r\n                ]\r\n            }\r\n        ]\r\n    }\r\n}";
                }
                MainData.getEnvGraph = JsonTool.GetInstance.JsonToObjectByLitJson<GetEnvGraph>(jsonStr);
                Debugger.Log("缓存环境场景图,房间与房间的邻接关系 jsonStr:" + jsonStr);
                //存档
                DataSave.GetInstance.SaveGetEnvGraph_data(new GetEnvGraph.GetEnvGraph_data
                {
                    items = MainData.getEnvGraph.data.items,
                    idScene = MainData.SceneID
                });
            }, null, rawJsonStr);
        }
        else
        {
            //读档
            GetEnvGraph_data getEnvGraph_Data = DataRead.GetInstance.ReadGetEnvGraph_data();
            MainData.getEnvGraph = new GetEnvGraph
            {
                data = getEnvGraph_Data,
                message = "读档"
            };
        }
    }

    /// <summary>
    /// 改变仿真引擎状态
    /// </summary>
    /// <param name="id">仿真引擎实例的id号码</param>    
    /// <param name="state">仿真引擎状态标识</param>
    public void ChangeProgramState(string id, ProgramState state, Action calllbackSuc = null, Action callbackFail = null)
    {
        string rawJsonStr = "{  \"id\":\"" + id + "\"  ,\"state\":\"" + state.ToString() + "\" }";
        Debugger.Log("改变仿真引擎状态 rawJsonStr:" + rawJsonStr);
        MFramework.NetworkHttp.GetInstance.SendRequest(RequestType.Post, URL_CHANGE_SIMULATOR_STATE, new Dictionary<string, string>(), (string jsonStr) =>
        {
            Debugger.Log("改变仿真引擎状态 jsonStr:" + jsonStr);
            calllbackSuc?.Invoke();
        }, null, rawJsonStr, (m, n) =>
        {
            Debugger.LogError("改变仿真引擎状态接口调用失败 m:" + m + ",n:" + n);
            callbackFail?.Invoke();
        });
    }

    /// <summary>
    /// 存档
    /// </summary>
    public void SaveFileData(string jsonData, Action calllbackSuc = null, Action callbackFail = null)
    {
        string path = "http://10.101.80.74:8080/simulation/history/add";

        //Test
        //path = "http://6ziygv.natappfree.cc/simulation/history/add";

        Debugger.Log("尝试发起线上存档 path:" + path);
        string rawJsonStr = jsonData;
        MFramework.NetworkHttp.GetInstance.SendRequest(RequestType.Post, path, new Dictionary<string, string> { }, (string jsonStr) =>
        {
            Debugger.Log("存档成功 jsonStr:" + jsonStr);
            calllbackSuc?.Invoke();
        }, null, rawJsonStr, (m, n) =>
        {
            Debugger.LogError("存档失败 m:" + m + ",n:" + n);
            callbackFail?.Invoke();
        });
    }

    /// <summary>
    /// 读档
    /// </summary>
    public void ReadFileData(string sceneID, Action<ReadFileData> calllbackSuc = null, Action callbackFail = null)
    {
        string path = "http://10.101.80.74:8080/simulation/history/getInfo";

        //Test
        //path = "http://6ziygv.natappfree.cc/simulation/history/getInfo";

        Debugger.Log("尝试读档 " + path);
        //string rawJsonStr = path;
        MFramework.NetworkHttp.GetInstance.SendRequest(RequestType.Get, path, new Dictionary<string, string>()
        {
            {
                "id",sceneID
            }
        }, (string jsonStr) =>
        {
            Debugger.Log("读档接口回调 jsonStr:");
            ReadFileData readFileData = JsonUtility.FromJson<ReadFileData>(jsonStr);
            calllbackSuc?.Invoke(readFileData);
        }, null, "", (m, n) =>
        {
            Debugger.LogError("读档失败 m:" + m + ",n:" + n);
            callbackFail?.Invoke();
        });
    }

    #endregion


    #region MQTT
    public void InitMQTT()
    {
        //NetworkMqtt.GetInstance.IsWebgl = false;

        NetworkMqtt.GetInstance.AddConnectedSucEvent(() =>
        {
            switch (GameLaunch.GetInstance.scene)
            {
                case GameLaunch.Scenes.MainScene1:
                    NetworkMqtt.GetInstance.Subscribe(TOPIC_SEND, TOPIC_CHANGESTATE, TOPIC_ADD_GOODS, TOPIC_DEL_GOODS);
                    //TEST
                    NetworkMqtt.GetInstance.Subscribe(
                        TOPIC_WEB_SEND, TOPIC_WEB_RECV,
                        TOPIC_LIVEDATA, TOPIC_GLOBAL, TOPIC_CAMERA,
                        TOPIC_RECV, TOPIC_ROOMINFODATA);
                    break;
                case GameLaunch.Scenes.MainScene2:
                    NetworkMqtt.GetInstance.Subscribe(TOPIC_PEOPLE_PERCEPTION, TOPIC_ROBOT_POS);
                    break;
                default:
                    break;
            }
        });

        //初始化并订阅主题tcp://10.5.24.28:1883
        NetworkMqtt.GetInstance.Init(new MqttConfig()
        {
            clientIP = MainData.ConfigData?.MqttConfig.ClientIP, //"10.5.24.28",
            clientPort = NetworkMqtt.GetInstance.IsWebgl ? 8083 : 1883
        });
        //监听消息回调
        NetworkMqtt.GetInstance.AddListenerSubscribe((string topic, string msg) =>
        {
            ParseMQTTMsg(topic, msg);
        });
    }

    /// <summary>
    /// 解析MQTT消息
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="msg"></param>
    private void ParseMQTTMsg(string topic, string msg)
    {
        Debugger.Log($"recv mqtt callback. topic：{topic}， msg：{msg}");
        //Debugger.Log($"recv mqtt callback. topic：{topic}");

        //在非Unity主线程中调用UnityEngineApi
        PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            switch (topic)
            {
                case TOPIC_WEB_SEND:
                case TOPIC_SEND:
                    TaskCenter.GetInstance.AddOrder(msg);
                    break;
                case TOPIC_CHANGESTATE:
                    ChangeStateData changeStateData = JsonTool.GetInstance.JsonToObjectByLitJson<ChangeStateData>(msg);
                    string id = changeStateData.id;
                    ProgramState programState = (ProgramState)Enum.Parse(typeof(ProgramState), changeStateData.state);
                    //ChangeProgramState(id, programState);
                    UIManager.GetInstance.GetUIFormLogicScript<UIFormMain>().OnClickStateBtn(programState, id);
                    break;
                case TOPIC_ADD_GOODS:
                    JsonAddEntity jsonAddEntity = JsonTool.GetInstance.JsonToObjectByLitJson<JsonAddEntity>(msg);
                    MainDataTool.GetInstance.AddEntityToTargetPlace(jsonAddEntity);
                    break;
                case TOPIC_DEL_GOODS:
                    JsonDelEntity jsonDelEntity = JsonTool.GetInstance.JsonToObjectByLitJson<JsonDelEntity>(msg);
                    MainDataTool.GetInstance.DelEntityToTargetPlace(jsonDelEntity);
                    break;
                case TOPIC_ROBOT_POS:
                    Feature_Robot_Pos feature_Robot_Pos = JsonTool.GetInstance.JsonToObjectByLitJson<Feature_Robot_Pos>(msg);
                    MainData.feature_robot_pos.Enqueue(feature_Robot_Pos);
                    break;
                case TOPIC_PEOPLE_PERCEPTION:
                    Feature_People_Perception feature_People_Perception = JsonTool.GetInstance.JsonToObjectByLitJson<Feature_People_Perception>(msg);
                    MainData.feature_People_Perceptions.Enqueue(feature_People_Perception);
                    break;
                default:
                    //Debugger.Log($"Other Topoc :{topic}");
                    break;
            }
        });
    }

    /// <summary>
    /// 更新全局场景图
    /// </summary>
    /// <param name="items"></param>
    public void SendMQTTUpdateScenes(PostThingGraph items)
    {
        string jsonStr = JsonTool.GetInstance.ObjectToJsonStringByLitJson(items);
        NetworkMqtt.GetInstance.Publish(TOPIC_GLOBAL, jsonStr);
        //Debugger.Log("更新全局场景图 jsonStr:" + jsonStr);
    }

    /// <summary>
    /// 更新相机视⻆场景图
    /// </summary>
    /// <param name="items"></param>
    public void SendMQTTUpdateCamera(PostThingGraph items)
    {
        string jsonStr = JsonTool.GetInstance.ObjectToJsonStringByLitJson(items);
        NetworkMqtt.GetInstance.Publish(TOPIC_CAMERA, jsonStr);
    }

    public void SendMQTTLiveData(byte[] msgBytes)
    {
        NetworkMqtt.GetInstance.Publish(TOPIC_LIVEDATA, msgBytes);
    }
    #endregion


    /// <summary>
    /// 发送指令完成情况
    /// </summary>
    /// <param name="controlResult"></param>
    public void SendMQTTControlResult(ControlResult controlResult)
    {
        MainData.ControlCommitCompletedList.Add(controlResult);
        string jsonStr = JsonTool.GetInstance.ObjectToJsonStringByLitJson(controlResult);

        NetworkMqtt.GetInstance.Publish(TOPIC_WEB_RECV, jsonStr);
        NetworkMqtt.GetInstance.Publish(TOPIC_RECV, jsonStr);
    }

    /// <summary>
    /// 发送房间坐标位置信息
    /// </summary>
    /// <param name="controlResult"></param>
    public void SendMQTTRoomInfoData(RoomInfoData roomInfoData)
    {
        string jsonStr = JsonTool.GetInstance.ObjectToJsonStringByLitJson(roomInfoData);
        NetworkMqtt.GetInstance.Publish(TOPIC_ROOMINFODATA, jsonStr);
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
    /// 仿真实例id，具有唯⼀性,即场景ID
    /// </summary>
    public string simulatorId = MainData.SceneID;
    /// <summary>
    /// 任务id，具有唯⼀性
    /// </summary>
    public string task_id;
    /// <summary>
    /// 当前所在的房间，房间类型
    /// </summary>
    public string targetRommType;
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
    public string objectName;
    /// <summary>
    /// 操作⽬标物体的id
    /// </summary>
    public string objectId;
    /// <summary>
    /// 仿真实例id，具有唯⼀性
    /// </summary>
    public string simulatorId;
    /// <summary>
    /// 任务id，具有唯⼀性
    /// </summary>
    public string task_id;
}