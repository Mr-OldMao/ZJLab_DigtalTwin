using MFramework;
using System;
using System.Collections.Generic;
using UnityEngine;
using static GenerateRoomData;
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
    #region HTTP
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

    //改变仿真引擎状态
    private static string URL_GENERATE_TMPID = URL_SUBROOT + "simulator/generateTmpId";

    //Web端房间数据列表
    public const string URL_SCENE_QUERYLIST = "http://10.101.80.74:8080/simulation/scene/queryList";
    #endregion

    /*MQTT*/
    #region 仿真
    //更新全局场景图
    private const string TOPIC_GLOBAL = "/simulator/thingGraph/global";
    //更新相机视⻆场景图
    private const string TOPIC_CAMERA = "/simulator/thingGraph/camera";
    //接收服务器控制指令
    public const string TOPIC_SEND = "simulator/send";
    //发控制结果给服务器
    public const string TOPIC_RECV = "simulator/recv";

    //发送房间信息
    public const string TOPIC_ROOMINFODATA = "simulator/roomInfoData";
    //引擎状态
    public const string TOPIC_CHANGESTATE = "simulator/changeState";
    //直播流信息
    public static string TOPIC_LIVEDATA = "simulator/liveStreaming_" + MainData.SceneID;
    //新增房间实体模型
    public const string TOPIC_ADD_GOODS = "simulator/addGoods";
    //删除房间实体模型
    public const string TOPIC_DEL_GOODS = "simulator/delGoods";

    //测试从Web端 接收服务器控制指令
    public const string TOPIC_WEB_SEND = "simulator/web/send";
    //测试 发控制结果给Web端
    public const string TOPIC_WEB_RECV = "simulator/web/recv";
    //给web端更新全局场景图
    public const string TOPIC_Web_GLOBAL = "/simulator/thingGraph/web/global";

    //web端的房间布局变更
    public const string TOPIC_WEB_CHANGEPOSITION = "simulator/changePosition";
    //web端自定义相机坐标
    public const string TOPIC_WEB_CHANGEVIEWPOSITON = "simulator/changeViewPositon";

    #endregion

    #region 数字孪生
    //访客坐标信息
    public const string TOPIC_PEOPLE_PERCEPTION = "feature/people_perception";
    //机器人坐标信息
    public const string TOPIC_ROBOT_POS = "feature/robot_pos";
    #endregion

    #region HTTP

    /// <summary>
    /// 获取TmpID 即Token
    /// </summary>
    public void GetTmpID(string id, Action<string> callback)
    {
        if (string.IsNullOrEmpty(MainData.tmpID))
        {
            if (MainData.UseTestData)
            {
                string tmpId = UnityEngine.Random.Range(1000000000, 9999999999).ToString();
                MainData.tmpID = tmpId;
                Debugger.Log("tempId:" + tmpId);
                callback?.Invoke(tmpId);
            }
            else
            {
                string rawJsonStr = "{\"id\":\"" + id + "\"}";
                MFramework.NetworkHttp.GetInstance.SendRequest(RequestType.Post, URL_GENERATE_TMPID, new Dictionary<string, string>(), (string jsonStr) =>
                {
                    Debugger.Log("获取Token成功 jsonStr:" + jsonStr);
                    JsonGenerateTempID jsonGenerateTempID = JsonTool.GetInstance.JsonToObjectByLitJson<JsonGenerateTempID>(jsonStr);
                    string tmpId = jsonGenerateTempID.message;
                    MainData.tmpID = tmpId;
                    Debugger.Log("tempId:" + tmpId);
                    callback?.Invoke(tmpId);
                }, null, rawJsonStr, (m, n) =>
                {
                    Debugger.LogError("获取Token失败,系统生成随机token m:" + m + ",n:" + n);
                    string tmpId = UnityEngine.Random.Range(1000000000, 9999999999).ToString();
                    MainData.tmpID = tmpId;
                    Debugger.Log("tempId:" + tmpId);
                    callback?.Invoke(tmpId);
                });
            }
        }
        else
        {
            Debugger.Log("tempId:" + MainData.tmpID);
            callback?.Invoke(MainData.tmpID);
        }
    }

    /// <summary>
    /// 缓存环境场景图,房间与房间的邻接关系
    /// </summary>
    /// <param name="id">仿真引擎实例的id号码</param>
    public void CacheGetEnvGraph(string id, Action callbackSuc = null, Action callbackFail = null)
    {
        string rawJsonStr = "{\"id\":\"" + id + "\"}";
        Debugger.Log("缓存环境场景图,房间与房间的邻接关系 " + rawJsonStr);
        if (MainData.UseTestData)
        {
            //Test测试数据
            string jsonStr = @"
{
    ""code"": 200,
    ""message"": ""good"",
    ""success"": true,
    ""data"": {
        ""items"": [
            {
                ""id"": ""sim:1"",
                ""name"": ""LivingRoom"",
                ""relatedThing"": [
                    {
                        ""target"": {
                            ""id"": ""sim:2"",
                            ""name"": ""BathRoom"",
                            ""relatedThing"": []
                        },
                        ""relationship"": ""Left""
                    },
                    {
                        ""target"": {
                            ""id"": ""sim:3"",
                            ""name"": ""LivingRoom"",
                            ""relatedThing"": []
                        },
                        ""relationship"": ""Right""
                    },
                    {
                        ""target"": {
                            ""id"": ""sim:4"",
                            ""name"": ""KitchenRoom"",
                            ""relatedThing"": []
                        },
                        ""relationship"": ""Top""
                    }
                ]
            },
            {
                ""id"": ""sim:5"",
                ""name"": ""BedRoom"",
                ""relatedThing"": [
                    {
                        ""target"": {
                            ""id"": ""sim:1"",
                            ""name"": ""LivingRoom"",
                            ""relatedThing"": []
                        },
                        ""relationship"": ""Top""
                    }
                ]
            },
            {
                ""id"": ""sim:6"",
                ""name"": ""StorageRoom"",
                ""relatedThing"": [
                    {
                        ""target"": {
                            ""id"": ""sim:3"",
                            ""name"": ""LivingRoom"",
                            ""relatedThing"": []
                        },
                        ""relationship"": ""Top""
                    }
                ]
            },
            {
                ""id"": ""sim:7"",
                ""name"": ""StudyRoom"",
                ""relatedThing"": [
                    {
                        ""target"": {
                            ""id"": ""sim:3"",
                            ""name"": ""LivingRoom"",
                            ""relatedThing"": []
                        },
                        ""relationship"": ""Left""
                    }
                ]
            }
        ]
    }
}
                ";
            Debugger.Log("已使用测试数据，房间与房间临界关系 jsonStr：" + jsonStr);
            MainData.getEnvGraph = JsonTool.GetInstance.JsonToObjectByLitJson<GetEnvGraph>(jsonStr);
            Debugger.Log("缓存环境场景图,房间与房间的邻接关系 jsonStr:" + jsonStr);
            //存档
            DataSave.GetInstance.SaveGetEnvGraph_data(new GetEnvGraph.GetEnvGraph_data
            {
                items = MainData.getEnvGraph.data.items,
                idScene = MainData.SceneID
            });
            callbackSuc?.Invoke();
        }
        else
        {
            if (!MainData.CanReadFile)
            {
                MFramework.NetworkHttp.GetInstance.SendRequest(RequestType.Post, URL_GET_ENV_GRAPH, new Dictionary<string, string>(), (string jsonStr) =>
                {
                    MainData.getEnvGraph = JsonTool.GetInstance.JsonToObjectByLitJson<GetEnvGraph>(jsonStr);
                    Debugger.Log("缓存环境场景图,房间与房间的邻接关系 jsonStr:" + jsonStr);
                    //存档
                    DataSave.GetInstance.SaveGetEnvGraph_data(new GetEnvGraph.GetEnvGraph_data
                    {
                        items = MainData.getEnvGraph.data.items,
                        idScene = MainData.SceneID
                    });
                    callbackSuc?.Invoke();
                }, null, rawJsonStr);
            }
            else
            {
                //读档
                GetEnvGraph_data getEnvGraph_Data = DataRead.GetInstance.ReadGetEnvGraph_data();
                if (getEnvGraph_Data != null)
                {
                    MainData.getEnvGraph = new GetEnvGraph
                    {
                        data = getEnvGraph_Data,
                        message = "读档"
                    };
                    callbackSuc?.Invoke();
                }
                else
                {
                    Debugger.LogError("读档失败 GetEnvGraph_data is null");
                    callbackFail?.Invoke();
                }
            }
        }
    }

    /// <summary>
    /// 缓存场景图，物体与房间的邻接关系
    /// </summary>
    /// <param name="id">仿真引擎实例的id号码</param>
    public void CacheGetThingGraph(string id, Action callbackSuc = null, Action callbakcFail = null)
    {
        //if (MainData.UseTestData)
        //{
        //    string jsonStr = "{\r\n    \"targetList\": [],\r\n    \"target\": {\r\n        \"items\": [\r\n            {\r\n                \"position\": [\r\n                    0.0,\r\n                    0.0,\r\n                    0.0\r\n                ],\r\n                \"rotation\": [\r\n                    0.0,\r\n                    0.0,\r\n                    0.0\r\n                ],\r\n                \"id\": \"sim: 1\",\r\n                \"name\": \"LivingRoom\",\r\n                \"relatedThing\": [\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                10.0,\r\n                                0.0,\r\n                                7.5\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1001\",\r\n                            \"name\": \"Toplamp\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": false\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                7.0,\r\n                                0.0,\r\n                                4.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1002\",\r\n                            \"name\": \"Bin\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": false\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                7.0,\r\n                                0.0,\r\n                                6.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                270.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1004\",\r\n                            \"name\": \"Bin\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": false\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                8.0,\r\n                                0.0,\r\n                                5.099999904632568\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1015\",\r\n                            \"name\": \"Sofa\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": false\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                8.0,\r\n                                0.0,\r\n                                9.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1016\",\r\n                            \"name\": \"TV\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": false\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                11.0,\r\n                                0.0,\r\n                                4.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1034\",\r\n                            \"name\": \"Pile\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": false\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                9.0,\r\n                                0.0,\r\n                                11.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"DoorX_2_7\",\r\n                            \"name\": \"LivingRoomDoor\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": true\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                12.0,\r\n                                0.0,\r\n                                11.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"DoorX_5_7\",\r\n                            \"name\": \"LivingRoomDoor\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": true\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                7.0,\r\n                                0.0,\r\n                                8.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"DoorY_0_4\",\r\n                            \"name\": \"LivingRoomDoor\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": true\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                13.0,\r\n                                0.0,\r\n                                10.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"DoorY_6_6\",\r\n                            \"name\": \"LivingRoomDoor\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": true\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                9.0,\r\n                                0.0,\r\n                                4.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"DoorX_2_0\",\r\n                            \"name\": \"LivingRoomDoor\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": true\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    }\r\n                ],\r\n                \"dynamic\": false,\r\n                \"wallMaterial\": 0,\r\n                \"floorMaterial\": 0\r\n            },\r\n            {\r\n                \"position\": [\r\n                    0.0,\r\n                    0.0,\r\n                    0.0\r\n                ],\r\n                \"rotation\": [\r\n                    0.0,\r\n                    0.0,\r\n                    0.0\r\n                ],\r\n                \"id\": \"sim: 3\",\r\n                \"name\": \"LivingRoom\",\r\n                \"relatedThing\": [\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                3.5,\r\n                                0.0,\r\n                                6.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1003\",\r\n                            \"name\": \"Toplamp\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": false\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                2.0,\r\n                                0.0,\r\n                                8.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"DoorX_-5_4\",\r\n                            \"name\": \"LivingRoomDoor\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": true\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    }\r\n                ],\r\n                \"dynamic\": false,\r\n                \"wallMaterial\": 0,\r\n                \"floorMaterial\": 0\r\n            },\r\n            {\r\n                \"position\": [\r\n                    0.0,\r\n                    0.0,\r\n                    0.0\r\n                ],\r\n                \"rotation\": [\r\n                    0.0,\r\n                    0.0,\r\n                    0.0\r\n                ],\r\n                \"id\": \"sim: 5\",\r\n                \"name\": \"BedRoom\",\r\n                \"relatedThing\": [\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                9.5,\r\n                                0.0,\r\n                                13.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1005\",\r\n                            \"name\": \"Toplamp\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": false\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                7.0,\r\n                                0.0,\r\n                                11.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1006\",\r\n                            \"name\": \"Bin\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": false\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                10.0,\r\n                                0.0,\r\n                                12.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                270.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1017\",\r\n                            \"name\": \"TV\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": false\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                9.0,\r\n                                0.0,\r\n                                11.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"DoorX_2_7\",\r\n                            \"name\": \"BedRoomDoor\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": true\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    }\r\n                ],\r\n                \"dynamic\": false,\r\n                \"wallMaterial\": 0,\r\n                \"floorMaterial\": 0\r\n            },\r\n            {\r\n                \"position\": [\r\n                    0.0,\r\n                    0.0,\r\n                    0.0\r\n                ],\r\n                \"rotation\": [\r\n                    0.0,\r\n                    0.0,\r\n                    0.0\r\n                ],\r\n                \"id\": \"sim: 6\",\r\n                \"name\": \"StorageRoom\",\r\n                \"relatedThing\": [\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                15.0,\r\n                                0.0,\r\n                                13.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1007\",\r\n                            \"name\": \"Toplamp\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": false\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                13.0,\r\n                                0.0,\r\n                                12.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                270.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1008\",\r\n                            \"name\": \"Bin\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": false\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                13.0,\r\n                                0.0,\r\n                                14.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                270.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1030\",\r\n                            \"name\": \"Plant\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": false\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                15.0,\r\n                                0.0,\r\n                                14.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                180.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1031\",\r\n                            \"name\": \"BoxPash\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": true\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                17.0,\r\n                                0.0,\r\n                                14.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                180.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1032\",\r\n                            \"name\": \"BoxPull\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": true\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                12.0,\r\n                                0.0,\r\n                                11.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"DoorX_5_7\",\r\n                            \"name\": \"StorageRoomDoor\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": true\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    }\r\n                ],\r\n                \"dynamic\": false,\r\n                \"wallMaterial\": 0,\r\n                \"floorMaterial\": 0\r\n            },\r\n            {\r\n                \"position\": [\r\n                    0.0,\r\n                    0.0,\r\n                    0.0\r\n                ],\r\n                \"rotation\": [\r\n                    0.0,\r\n                    0.0,\r\n                    0.0\r\n                ],\r\n                \"id\": \"sim: 7\",\r\n                \"name\": \"StudyRoom\",\r\n                \"relatedThing\": [\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                3.5,\r\n                                0.0,\r\n                                10.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1009\",\r\n                            \"name\": \"Toplamp\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": false\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                0.0,\r\n                                0.0,\r\n                                8.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1010\",\r\n                            \"name\": \"Bin\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": false\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                0.0,\r\n                                0.0,\r\n                                10.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1024\",\r\n                            \"name\": \"Sofa\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": false\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                5.0,\r\n                                0.0,\r\n                                10.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1025\",\r\n                            \"name\": \"Desk\",\r\n                            \"relatedThing\": [\r\n                                {\r\n                                    \"target\": {\r\n                                        \"position\": [\r\n                                            5.61299991607666,\r\n                                            0.6840000152587891,\r\n                                            11.107000350952149\r\n                                        ],\r\n                                        \"rotation\": [\r\n                                            0.0,\r\n                                            0.0,\r\n                                            0.0\r\n                                        ],\r\n                                        \"id\": \"sim: 1027\",\r\n                                        \"name\": \"Book\",\r\n                                        \"relatedThing\": [],\r\n                                        \"dynamic\": false\r\n                                    },\r\n                                    \"relationship\": \"On\"\r\n                                },\r\n                                {\r\n                                    \"target\": {\r\n                                        \"position\": [\r\n                                            6.164000034332275,\r\n                                            0.013999998569488526,\r\n                                            10.64900016784668\r\n                                        ],\r\n                                        \"rotation\": [\r\n                                            0.0,\r\n                                            0.0,\r\n                                            0.0\r\n                                        ],\r\n                                        \"id\": \"sim: 1026\",\r\n                                        \"name\": \"Chair\",\r\n                                        \"relatedThing\": [],\r\n                                        \"dynamic\": false\r\n                                    },\r\n                                    \"relationship\": \"Below\"\r\n                                }\r\n                            ],\r\n                            \"dynamic\": false\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                4.0,\r\n                                0.0,\r\n                                8.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1028\",\r\n                            \"name\": \"Cabinet\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": false\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                7.0,\r\n                                0.0,\r\n                                8.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"DoorY_0_4\",\r\n                            \"name\": \"StudyRoomDoor\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": true\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                2.0,\r\n                                0.0,\r\n                                8.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"DoorX_-5_4\",\r\n                            \"name\": \"StudyRoomDoor\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": true\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    }\r\n                ],\r\n                \"dynamic\": false,\r\n                \"wallMaterial\": 0,\r\n                \"floorMaterial\": 0\r\n            },\r\n            {\r\n                \"position\": [\r\n                    0.0,\r\n                    0.0,\r\n                    0.0\r\n                ],\r\n                \"rotation\": [\r\n                    0.0,\r\n                    0.0,\r\n                    0.0\r\n                ],\r\n                \"id\": \"sim: 2\",\r\n                \"name\": \"BathRoom\",\r\n                \"relatedThing\": [\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                16.0,\r\n                                0.0,\r\n                                8.5\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1011\",\r\n                            \"name\": \"Toplamp\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": false\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                13.0,\r\n                                0.0,\r\n                                6.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1012\",\r\n                            \"name\": \"Bin\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": false\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                13.0,\r\n                                0.0,\r\n                                8.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1023\",\r\n                            \"name\": \"Bathtub\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": false\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                13.0,\r\n                                0.0,\r\n                                10.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"DoorY_6_6\",\r\n                            \"name\": \"BathRoomDoor\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": true\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    }\r\n                ],\r\n                \"dynamic\": false,\r\n                \"wallMaterial\": 0,\r\n                \"floorMaterial\": 0\r\n            },\r\n            {\r\n                \"position\": [\r\n                    0.0,\r\n                    0.0,\r\n                    0.0\r\n                ],\r\n                \"rotation\": [\r\n                    0.0,\r\n                    0.0,\r\n                    0.0\r\n                ],\r\n                \"id\": \"sim: 4\",\r\n                \"name\": \"KitchenRoom\",\r\n                \"relatedThing\": [\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                12.0,\r\n                                0.0,\r\n                                2.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1013\",\r\n                            \"name\": \"Toplamp\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": false\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                9.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1014\",\r\n                            \"name\": \"Bin\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": false\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                9.0,\r\n                                0.0,\r\n                                2.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"sim: 1022\",\r\n                            \"name\": \"Bigsink\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": false\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    },\r\n                    {\r\n                        \"target\": {\r\n                            \"position\": [\r\n                                9.0,\r\n                                0.0,\r\n                                4.0\r\n                            ],\r\n                            \"rotation\": [\r\n                                0.0,\r\n                                0.0,\r\n                                0.0\r\n                            ],\r\n                            \"id\": \"DoorX_2_0\",\r\n                            \"name\": \"KitchenRoomDoor\",\r\n                            \"relatedThing\": [],\r\n                            \"dynamic\": true\r\n                        },\r\n                        \"relationship\": \"In\"\r\n                    }\r\n                ],\r\n                \"dynamic\": false,\r\n                \"wallMaterial\": 0,\r\n                \"floorMaterial\": 0\r\n            }\r\n        ],\r\n        \"id\": \"test\",\r\n        \"idScene\": \"test\"\r\n    }\r\n}";
        //    Serialization<PostThingGraph> postThingGraph = JsonUtility.FromJson<Serialization<PostThingGraph>>(jsonStr);
        //    PostThingGraph m_PostThingGraph = postThingGraph.Target();
        //    if (postThingGraph != null)
        //    {
        //        MainData.getThingGraph = new GetThingGraph
        //        {
        //            data = new GetThingGraph_data
        //            {
        //                items = m_PostThingGraph.items
        //            }
        //        };
        //        callbackSuc?.Invoke();
        //    }
        //    else
        //    {
        //        Debugger.LogError("读档失败 postThingGraph is null");
        //        callbakcFail?.Invoke();
        //    }
        //}
        //else 
        if (!MainData.CanReadFile)
        {
            string rawJsonStr = "{\"id\":\"" + id + "\"}";
            Debugger.Log("尝试获取缓存场景图，物体与房间的邻接关系 " + rawJsonStr);
            MFramework.NetworkHttp.GetInstance.SendRequest(RequestType.Post, URL_GET_THING_GRAPH, new Dictionary<string, string>(), (string jsonStr) =>
            {
                MainData.getThingGraph = JsonTool.GetInstance.JsonToObjectByLitJson<GetThingGraph>(jsonStr);
                Debugger.Log("已获取场景图，缓存物体与房间的邻接关系 jsonStr:" + jsonStr);
                //存档
                //DataSave.GetInstance.SaveGetThingGraph_data_items(new PostThingGraph
                //{
                //    id = MainData.SceneID,
                //    idScene = MainData.SceneID,
                //    items = MainData.getThingGraph.data.items
                //});

                List<RoomMatData> listRoomMatData = new List<RoomMatData>();
                foreach (var item in MainData.getThingGraph.data.items)
                {
                    RoomMatData roomMatData = listRoomMatData.Find((p) => { return p.roomID == item.id; });
                    if (roomMatData == null)
                    {
                        roomMatData = new RoomMatData
                        {
                            roomID = item.id,
                            roomType = (RoomType)Enum.Parse(typeof(RoomType), item.name),
                            matIDFloor = item.floorMaterial,
                            matIDWall = item.wallMaterial
                        };
                        listRoomMatData.Add(roomMatData);
                    }
                }
                GenerateRoomData.GetInstance.roomMatDatas = listRoomMatData;

                callbackSuc?.Invoke();
            }, null, rawJsonStr);
        }
        else
        {
            //读档
            PostThingGraph postThingGraph = DataRead.GetInstance.ReadGetThingGraph_data_items();
            if (postThingGraph != null)
            {
                MainData.getThingGraph = new GetThingGraph
                {
                    data = new GetThingGraph_data
                    {
                        items = postThingGraph.items
                    }
                };
                callbackSuc?.Invoke();
            }
            else
            {
                Debugger.LogError("读档失败 postThingGraph is null");
                callbakcFail?.Invoke();
            }
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
    /// 改变仿真引擎状态
    /// </summary>
    /// <param name="sceneId">仿真引擎实例的id号码</param>    
    /// <param name="state">仿真引擎状态标识</param>
    public void ChangeProgramState(string sceneId, string tmpId, ProgramState state, Action calllbackSuc = null, Action callbackFail = null)
    {
        string rawJsonStr = "{  \"id\":\"" + sceneId + "\"  ,\"state\":\"" + state.ToString() + "\" ,\"tmpId\" :\"" + tmpId + "\"  }";
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
            Debugger.Log("读档接口回调 jsonStr:" + jsonStr);
            ReadFileData readFileData = JsonUtility.FromJson<ReadFileData>(jsonStr);
            if (readFileData?.data?.dataPackageInfo?.TargetToList()?.Count > 0 ||
            (readFileData?.data?.dataPackageInfo?.Target() != null && !string.IsNullOrEmpty(readFileData?.data?.dataPackageInfo?.Target().json)))
            {
                calllbackSuc?.Invoke(readFileData);
            }
            else
            {
                Debugger.LogError("读档失败 未找到存档信息 sceneID：" + sceneID);
                callbackFail?.Invoke();
            }
        }, null, "", (m, n) =>
        {
            Debugger.LogError("读档失败 m:" + m + ",n:" + n);
            callbackFail?.Invoke();
        });
    }


    /// <summary>
    /// 获取Web端房间数据列表
    /// </summary>
    public void GetWebRoomDataList(Action<JsonWebRoomDataList> callback)
    {
        MFramework.NetworkHttp.GetInstance.SendRequest(RequestType.Get, URL_SCENE_QUERYLIST, new Dictionary<string, string> { }, (string jsonStr) =>
        {
            Debugger.Log("获取Web端房间数据列表 jsonStr:" + jsonStr);
            JsonWebRoomDataList jsonWebRoomDataList = JsonTool.GetInstance.JsonToObjectByLitJson<JsonWebRoomDataList>(jsonStr);
            callback?.Invoke(jsonWebRoomDataList);
        }, null, null, (m, n) =>
        {
            Debugger.LogError("存档失败 m:" + m + ",n:" + n);
            callback?.Invoke(null);
        });

    }

    #endregion


    #region MQTT
    public void InitMQTT()
    {
        //NetworkMqtt.GetInstance.IsWebgl = false;
        Debug.Log("InitMQTT");
        NetworkMqtt.GetInstance.AddConnectedSucEvent(() =>
        {
            switch (GameLaunch.GetInstance.scene)
            {
                case GameLaunch.Scenes.MainScene1:
                    NetworkMqtt.GetInstance.Subscribe(TOPIC_SEND, TOPIC_CHANGESTATE, TOPIC_ADD_GOODS, TOPIC_DEL_GOODS, TOPIC_WEB_CHANGEPOSITION, TOPIC_WEB_CHANGEVIEWPOSITON, TOPIC_WEB_SEND);
                    if (MainData.ConfigData.CoreConfig.ShowLog == 1)
                    {
                        //TEST
                        NetworkMqtt.GetInstance.Subscribe(
                             TOPIC_WEB_RECV,
                            TOPIC_Web_GLOBAL,
                            //TOPIC_LIVEDATA, TOPIC_GLOBAL, TOPIC_CAMERA,
                            TOPIC_RECV, TOPIC_ROOMINFODATA);
                    }
                    break;
                case GameLaunch.Scenes.MainScene2:
                    NetworkMqtt.GetInstance.Subscribe(TOPIC_PEOPLE_PERCEPTION, TOPIC_ROBOT_POS);
                    break;
                default:
                    break;
            }
        });

        if (string.IsNullOrEmpty(MainData.SceneID))
        {
            MainData.SceneID = "test";
        }

        //初始化并订阅主题tcp://10.5.24.28:1883
        NetworkMqtt.GetInstance.Init(new MqttConfig()
        {
            clientIP = MainData.ConfigData?.MqttConfig.ClientIP, //"10.5.24.28",
            clientPort = NetworkMqtt.GetInstance.IsWebgl ? 8083 : 1883,
            clientID = MainData.SceneID,
            userName = "UserName_" + MainData.SceneID
        });
        //监听消息回调
        Debugger.Log("注册监听消息回调");
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
        Debugger.Log($"recv mqtt callback. topic：{topic}，curTime：{System.DateTime.Now.ToString("yyyyMMdd_HHmmss")} ,msg：{msg}");
        //Debugger.Log($"recv mqtt callback. topic：{topic}");


        //在非Unity主线程中调用UnityEngineApi
        PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            switch (topic)
            {
                case TOPIC_WEB_SEND:
                case TOPIC_SEND:
                    TaskCenter.GetInstance.TryAddOrder(msg);
                    break;
                case TOPIC_CHANGESTATE:
                    ChangeStateData changeStateData = JsonTool.GetInstance.JsonToObjectByLitJson<ChangeStateData>(msg);
                    string sceneID = changeStateData.idScene;
                    string tmpID = changeStateData.tmpId;
                    if (sceneID == MainData.SceneID && tmpID == MainData.tmpID)
                    {
                        ProgramState programState = (ProgramState)Enum.Parse(typeof(ProgramState), changeStateData.state);
                        //ChangeProgramState(id, programState);
                        UIManager.GetInstance.GetUIFormLogicScript<UIFormMain>().OnClickStateBtn(programState, sceneID, tmpID);
                    }
                    break;
                case TOPIC_ADD_GOODS:
                    JsonAddEntity jsonAddEntity = JsonTool.GetInstance.JsonToObjectByLitJson<JsonAddEntity>(msg);
                    if (jsonAddEntity.idScene == MainData.SceneID && jsonAddEntity.tmpId == MainData.tmpID)
                    {
                        MainDataTool.GetInstance.AddEntityToTargetPlace(jsonAddEntity);
                    }
                    else
                    {
                        Debugger.LogError("add fail ,curSceneID：" + MainData.SceneID + ",curTmpId：" + MainData.tmpID + ",targetSceneID：" + jsonAddEntity.idScene + ",targetTmpId：" + jsonAddEntity.tmpId);
                    }
                    break;
                case TOPIC_DEL_GOODS:
                    JsonDelEntity jsonDelEntity = JsonTool.GetInstance.JsonToObjectByLitJson<JsonDelEntity>(msg);
                    if (jsonDelEntity.idScene == MainData.SceneID && jsonDelEntity.tmpId == MainData.tmpID)
                    {
                        MainDataTool.GetInstance.DelEntityToTargetPlace(jsonDelEntity);
                    }
                    else
                    {
                        Debugger.LogError("del fail ,curSceneID：" + MainData.SceneID + ",curTmpId：" + MainData.tmpID + ",targetSceneID：" + jsonDelEntity.idScene + ",targetTmpId：" + jsonDelEntity.tmpId);
                    }
                    break;
                case TOPIC_ROBOT_POS:
                    Feature_Robot_Pos feature_Robot_Pos = JsonTool.GetInstance.JsonToObjectByLitJson<Feature_Robot_Pos>(msg);
                    MainData.feature_robot_pos.Enqueue(feature_Robot_Pos);
                    break;
                case TOPIC_PEOPLE_PERCEPTION:
                    Feature_People_Perception feature_People_Perception = JsonTool.GetInstance.JsonToObjectByLitJson<Feature_People_Perception>(msg);
                    MainData.feature_People_Perceptions.Enqueue(feature_People_Perception);
                    break;
                case TOPIC_WEB_CHANGEPOSITION:
                    JsonChangeRoomLayout jsonChangeRoomLayout = JsonTool.GetInstance.JsonToObjectByLitJson<JsonChangeRoomLayout>(msg);
                    if (UpdateRoomData.GetInstance.CanUpdateRoomData && jsonChangeRoomLayout.sceneID == MainData.SceneID)
                    {
                        UpdateRoomData.GetInstance.UpdateAllRoomData(jsonChangeRoomLayout);
                    }
                    break;
                case TOPIC_WEB_CHANGEVIEWPOSITON:
                    JsonChangeViewPositon jsonChangeViewPositon = JsonTool.GetInstance.JsonToObjectByLitJson<JsonChangeViewPositon>(msg);
                    if (jsonChangeViewPositon.sceneID == MainData.SceneID)
                    {
                        Debugger.Log("change custom camera " + msg);
                        Vector3 pos = new(jsonChangeViewPositon.pos[0], jsonChangeViewPositon.pos[1], jsonChangeViewPositon.pos[2]);
                        Vector3 rot = new(jsonChangeViewPositon.rot[0], jsonChangeViewPositon.rot[1], jsonChangeViewPositon.rot[2]);
                        CameraControl.GetInstance.SetCameraCustomPos(pos, rot);
                        CameraControl.GetInstance.ShowCameraCustom(true);
                    }
                    break;
                case TOPIC_GLOBAL:
                    //Debugger.Log("TOPIC_GLOBAL,json：" + msg);
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


    /// <summary>
    /// Web端新增、删除物品回调
    /// </summary>
    /// <param name="items"></param>
    public void SendMQTTUpdateEntity(JsonWebGlobalEntityData items)
    {
        string jsonStr = JsonTool.GetInstance.ObjectToJsonStringByLitJson(items);
        NetworkMqtt.GetInstance.Publish(TOPIC_Web_GLOBAL, jsonStr);
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
/// MQTT 指令回调数据结构 recv
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
    public string sceneID = MainData.SceneID;
    public string tmpId = MainData.tmpID;
    /// <summary>
    /// 任务id，具有唯⼀性
    /// </summary>
    public string task_id;
    /// <summary>
    /// 当前所在的房间，房间类型
    /// </summary>
    public string targetRoomType;
}

/// <summary>
/// MQTT 接收控制指令数据结构 send
/// </summary>
public class ControlCommit
{
    public string sceneID;
    public string tmpId;
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
    /// 操作⽬标物体的类型
    /// </summary>
    public string objectName;
    /// <summary>
    /// 操作⽬标物体的id
    /// </summary>
    public string objectId;
    ///// <summary>
    ///// 仿真实例id，具有唯⼀性
    ///// </summary>
    //public string simulatorId;
    /// <summary>
    /// 任务id，具有唯⼀性
    /// </summary>
    public string taskId;
}