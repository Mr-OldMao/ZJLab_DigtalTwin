using System.Collections.Generic;
using UnityEngine;
using MFramework;
using static GenerateRoomData;
using System;
using static GetEnvGraph;
using static GetThingGraph;
using static GenerateRoomBorderModel;
using System.Collections;
using System.IO;
/// <summary>
/// 标题：程序逻辑入口
/// 功能：程序主逻辑
/// 作者：毛俊峰
/// 时间：2023.08.18
/// </summary>
public class GameLogic : SingletonByMono<GameLogic>
{
    private bool m_IsLoadedAssets = false;
    public GameObject staticModelRootNode = null;
    private Coroutine m_CoroutineUpadeteSceneEntityInfo = null;
    private Coroutine m_CoroutineUpadeteCameraEntityInfo = null;
    private GameObject m_Debugger;
    public bool CanUpadeteSceneEntityInfo { get; set; } = true;
    public float x;
    public float y;

    private int m_CurAgainGenerateSceneCount = 0;
    public void Init()
    {
        Debugger.Log("Init GameLogic");
        HideDebugger();
        SelectObjByMouse.GetInstance.Init();
        MsgEvent.RegisterMsgEvent(MsgEventName.InitComplete, InitCompleteEventCallback);

        string paramStr = string.Empty;
#if UNITY_EDITOR 
        paramStr = "test|1";// "WinPC_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "|" + "1";  //"Simulator:1700126538734|1"
        MainDataTool.GetInstance.InitMainDataParam(paramStr);

#else
#if UNITY_STANDALONE_LINUX
        paramStr = "test|1";//"LinuxPC_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "|" + "1";
        MainDataTool.GetInstance.InitMainDataParam(paramStr);
#elif UNITY_STANDALONE_WIN
        paramStr = "test|1";//"WinPC_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "|" + "1";
        MainDataTool.GetInstance.InitMainDataParam(paramStr);
#endif
#endif
    }

    /// <summary>
    /// 初始化数据完毕后回调
    /// </summary>
    private void InitCompleteEventCallback()
    {
        //优先级 本地读档 > 服务器读档 > 不读当
        //根据配置文件判定是否需要读档本地文件
        if (!string.IsNullOrEmpty(MainData.ConfigData.CoreConfig.LocalReadFileName))
        {
            MainData.CanReadFile = true;
            Debugger.Log("尝试本地读档 CoreConfig.LocalReadFileName：" + MainData.ConfigData.CoreConfig.LocalReadFileName, LogTag.Forever);
            string localReadFilePath = Application.streamingAssetsPath + "/" + MainData.ConfigData.CoreConfig.LocalReadFileName;
            if (File.Exists(localReadFilePath))
            {
                //读取本地数据
                DataRead.GetInstance.ReadAllDataByLocalFile(Application.streamingAssetsPath, MainData.ConfigData.CoreConfig.LocalReadFileName, (b) =>
                {
                    if (b)
                    {
                        Debugger.Log("本地读档成功! SceneID:" + MainData.SceneID, LogTag.Forever);
                        this.EnterMainScene();
                    }
                    else
                    {
                        Debugger.LogError("本地读档失败，请检查文件内容是否合法 CoreConfig.LocalReadFileName：" + MainData.ConfigData.CoreConfig.LocalReadFileName);
                    }
                });
            }
            else
            {
                Debugger.LogError("本地读档失败，配置文件Config.json中LocalReadFileName字段写入文件名不存在../StreamingAssets/目录下，请检查文件是否存在 CoreConfig.LocalReadFileName：" + MainData.ConfigData.CoreConfig.LocalReadFileName);
            }
        }
        else if (MainData.CanReadFile)
        {
            Debugger.Log("尝试从服务器读档", LogTag.Forever);
            //根据SceneID从服务器读取数据
            DataRead.GetInstance.ReadAllDataByServerSceneID((b) =>
            {
                if (b)
                {
                    this.EnterMainScene();
                }
                else
                {
                    Debugger.LogError("从服务器读档失败，无法进入场景，即将不读档，生成随机场景实例");
                    MainData.CanReadFile = false;
                    MsgEvent.SendMsg(MsgEventName.InitComplete);

                    //this.EnterMainScene();
                }
            });
        }
        else
        {
            Debugger.Log("不读档，生成随机场景实例", LogTag.Forever);
            Debugger.Log("MainDataDisplayAA   SceneID：" + MainData.SceneID
                          + ",UseTestData：" + MainData.UseTestData
                          + ",SendEntityInfoHZ：" + MainData.ConfigData.CoreConfig.SendEntityInfoHZ
                          + ",Http_IP：" + MainData.ConfigData.HttpConfig.IP
                          + ",Http_Port：" + MainData.ConfigData.HttpConfig.Port
                          + ",Mqtt_IP：" + MainData.ConfigData.MqttConfig.ClientIP
                          + ",Vs_Frame：" + MainData.ConfigData.VideoStreaming.Frame
                          + ",Vs_Quality：" + MainData.ConfigData.VideoStreaming.Quality,
                          LogTag.Forever);
            this.EnterMainScene();
        }
    }

    private void OnDisable()
    {
        MsgEvent.UnregisterMsgEvent(MsgEventName.InitComplete);
    }

    private void EnterMainScene()
    {
        CreateRootNode();

        //注册消息事件
        RegisterMsgEvent();

        //异步加载ab资源
        LoadAssetsByAddressable.GetInstance.LoadAssetsAsyncByLable(new List<string> { "ItemLable", "RoomBorderLable", "RobotEntity", "UIForm", "Mat" }, () =>
        {
            Debugger.Log("ab资源加载完毕回调");
            m_IsLoadedAssets = true;
        });

        //接入网络通信
        SendNetworkHTTP();
        NetworkMQTT();

        //等待ab资源加载完毕，以及http接口获取的场景数据，解析生成场景实体
        UnityTool.GetInstance.DelayCoroutineWaitReturnTrue(() =>
        {
            return m_IsLoadedAssets && MainData.getEnvGraph != null && MainData.getThingGraph != null;
        }, () =>
        {
            //加载UI窗体
            UIManager.GetInstance.Show<UIFormMain>();
            //生成场景中所有房间和物品
            GenerateScene();
        });
    }

    private void CreateRootNode()
    {
        staticModelRootNode = GameObject.Find("StaticModelRootNode");
        if (staticModelRootNode == null)
        {
            staticModelRootNode = new GameObject("StaticModelRootNode");
        }
    }

    /// <summary>
    /// 发送请求获取房间布局信息，物体位置信息
    /// </summary>
    /// <param name="callbackSuc"></param>
    private void SendNetworkHTTP(Action callbackSuc = null)
    {
        bool getThingGraph = false;
        bool getEnvGraph = false;

        InterfaceDataCenter.GetInstance.CacheGetThingGraph(MainData.SceneID, () => getThingGraph = true, () =>
        {
            MainData.CanReadFile = false;
            MsgEvent.SendMsg(MsgEventName.InitComplete);
        });
        InterfaceDataCenter.GetInstance.CacheGetEnvGraph(MainData.SceneID, () => getEnvGraph = true);
        UnityTool.GetInstance.DelayCoroutineWaitReturnTrue(() => { return getThingGraph && getEnvGraph; }, () => callbackSuc?.Invoke());
    }

    private void NetworkMQTT()
    {
        InterfaceDataCenter.GetInstance.InitMQTT();
    }

    #region Event
    private void RegisterMsgEvent()
    {
        MsgEvent.RegisterMsgEvent(MsgEventName.GenerateSceneComplete, () =>
        {
            UIManager.GetInstance.GetUIFormLogicScript<UIFormHintNotBtn>().Show("场景生成完毕！");


            MainData.IsFirstGenerate = false;
            //原点偏移至场景左下角
            Vector2 originOffset = GetOriginOffset();
            staticModelRootNode.transform.position = new Vector3(originOffset.x, 0, originOffset.y);
            //GenerateRoomItemModel.GetInstance.ItemEntityGroupNode.transform.position = new Vector3(originOffset.x, 0, originOffset.y);
            if (MainData.CanReadFile)
            {
                GenerateRoomItemModel.GetInstance.ItemEntityGroupNode.transform.position = Vector3.zero;
                MainData.ReadingFile = false;
                MainData.CanReadFile = false;
                Debugger.Log("读档生成场景实例完成", LogTag.Forever);
            }
            else
            {
                GenerateRoomItemModel.GetInstance.ItemEntityGroupNode.parent = staticModelRootNode.transform;
                GenerateRoomItemModel.GetInstance.ItemEntityGroupNode.transform.localPosition = Vector3.zero;
            }

            //生成机器人实体
            GenerateRobot();

            ////监听“门”碰撞事件
            //ListenerDoorCollEvent(true);

            //初始化相机
            CameraControl.GetInstance.Init();

            //启动TCP服务端 传输视频流
            LiveStreaming.GetInstance.Init(
                CameraControl.GetInstance.GetCameraEntity(CameraControl.CameraType.First).transform,
                CameraControl.GetInstance.GetCameraEntity(CameraControl.CameraType.Three).transform);

            //缓存所有实体物品数据信息
            InitCreateCacheItemDataInfo();

            //提交场景图，物体与房间的邻接关系
            InterfaceDataCenter.GetInstance.CommitGetThingGraph(MainData.CacheSceneItemsInfo, () =>
            {
                ////更新仿真引擎状态，从而获取服务器指令
                //InterfaceDataCenter.GetInstance.ChangeProgramState(MainData.ID, ProgramState.start);
            });

            //提交场景图布局，房间与房间位置关系
            SendRoomInfoData(originOffset);

            //提交场景实体信息
            if (m_CoroutineUpadeteSceneEntityInfo != null)
            {
                StopCoroutine(m_CoroutineUpadeteSceneEntityInfo);
                m_CoroutineUpadeteSceneEntityInfo = null;
            }
            m_CoroutineUpadeteSceneEntityInfo = StartCoroutine(UpadeteSceneEntityInfo());

            //提交摄像机前的实体信息
            if (m_CoroutineUpadeteCameraEntityInfo != null)
            {
                StopCoroutine(m_CoroutineUpadeteCameraEntityInfo);
                m_CoroutineUpadeteCameraEntityInfo = null;
            }
            m_CoroutineUpadeteCameraEntityInfo = StartCoroutine(UpadeteRobotFirstCameraEntityInfo());

            TaskCenter.GetInstance.Init();
        });
    }

    public void ListenerAllDoorOpenEvent(bool canListener)
    {
        foreach (var item in GameObject.FindObjectsOfType<AnimDoor>())
        {
            item.CanOpenDoor = canListener;
        }
    }
    public void ListenerAllDoorCloseEvent(bool canListener)
    {
        foreach (var item in GameObject.FindObjectsOfType<AnimDoor>())
        {
            item.CanCloseDoor = canListener;
        }
    }
    #endregion

    #region Generate

    public void GenerateScene()
    {
        if (!MainData.IsFirstGenerate)
        {
            SendNetworkHTTP(() =>
            {
                //生成场景中所有房间和物品
                GenerateEntity(() =>
                {
                    MsgEvent.SendMsg(MsgEventName.GenerateSceneComplete);
                });
            });
        }
        else
        {
            //生成场景中所有房间和物品
            GenerateEntity(() =>
            {
                MsgEvent.SendMsg(MsgEventName.GenerateSceneComplete);
            });
        }
    }

    public void GenerateEntity(Action generateCompleteCallback)
    {
        staticModelRootNode.transform.position = Vector3.zero;
        //写入各个房间之间的邻接关系
        List<RoomBaseInfo> roomBaseInfos = new List<RoomBaseInfo>();
        if (!MainData.CanReadFile)
        {
            for (int i = 0; i < MainData.getEnvGraph?.data?.items.Length; i++)
            {
                GetEnvGraph_data_items roomRelation = MainData.getEnvGraph?.data?.items[i];
                RoomBaseInfo roomBaseInfo = new RoomBaseInfo();
                roomBaseInfos.Add(roomBaseInfo);

                roomBaseInfo.curRoomType = (RoomType)Enum.Parse(typeof(RoomType), roomRelation.name);
                roomBaseInfo.roomSize = new uint[] { (uint)UnityEngine.Random.Range(4, 8), (uint)UnityEngine.Random.Range(4, 8) };
                roomBaseInfo.targetRoomsDirRelation = new List<RoomsDirRelation>();
                roomBaseInfo.curRoomID = roomRelation.id;
                //当前房间与其他房间邻接关系
                for (int j = 0; j < roomRelation.relatedThing?.Length; j++)
                {
                    RoomType targetRoomType = (RoomType)Enum.Parse(typeof(RoomType), roomRelation.relatedThing[j].target.name);
                    roomBaseInfo.targetRoomsDirRelation.Add(new RoomsDirRelation
                    {
                        targetRoomType = targetRoomType,
                        locationRelation = (DirEnum)Enum.Parse(typeof(DirEnum), roomRelation.relatedThing[j].relationship),
                        isCommonWall = true,
                        targetRoomID = roomRelation.relatedThing[j].target.id
                    });


                }

            }
            //补充各个房间之间的邻接关系
            for (int i = 0; i < MainData.getEnvGraph?.data?.items.Length; i++)
            {
                GetEnvGraph_data_items roomRelation = MainData.getEnvGraph?.data?.items[i];
                for (int j = 0; j < roomRelation.relatedThing?.Length; j++)
                {
                    RoomType targetRoomType = (RoomType)Enum.Parse(typeof(RoomType), roomRelation.relatedThing[j].target.name);
                    string curRoomID = roomRelation.relatedThing[j].target.id;
                    if (roomBaseInfos.Find((p) => { return p.curRoomType == targetRoomType && p.curRoomID == curRoomID; }) == null)
                    {
                        roomBaseInfos.Add(new RoomBaseInfo
                        {
                            curRoomType = targetRoomType,
                            curRoomID = curRoomID,
                            roomSize = new uint[] { (uint)UnityEngine.Random.Range(6, 8), (uint)UnityEngine.Random.Range(4, 8) }
                        });
                    }
                }
            }
        }
        else
        {
            roomBaseInfos = DataRead.GetInstance.ReadRoomBaseInfos();
        }




        GenerateRoomData.GetInstance.GenerateRandomRoomInfoData(roomBaseInfos, (p, k) =>
        {
            if (p == null || k == null)
            {
                Debugger.Log("generage fail , again generate...");
                if (++m_CurAgainGenerateSceneCount < 500)
                {
                    GenerateEntity(generateCompleteCallback);
                }
                else
                {
                    m_CurAgainGenerateSceneCount = 0;
                    Debugger.LogError("again generate fail!");
                    UIManager.GetInstance.GetUIFormLogicScript<UIFormHintNotBtn>().Show(new UIFormHintNotBtn.ShowParams { txtHintContent = "重新生成场景失败，请重试", colorHintContent = Color.red });
                }
            }
            else
            {
                m_CurAgainGenerateSceneCount = 0;
                GenerateRoomBorderModel.GetInstance.GenerateRoomBorder();
                GenerateRoomItemModel.GetInstance.GenerateRoomItem(k, MainData.getThingGraph.data.items);
                generateCompleteCallback?.Invoke();
            }
        });
    }


    #region Robot
    public void GenerateRobot()
    {
        GameObject robotEntity = GameObject.FindObjectOfType<AIRobotMove>()?.gameObject;
        if (robotEntity == null)
        {
            GameObject robotRes = LoadAssetsByAddressable.GetInstance.GetEntityRes("RobotEntity");
            robotEntity = Instantiate(robotRes);
        }
        SetRobotPos(robotEntity);
    }

    private void SetRobotPos(GameObject robot)
    {
        Vector3 targetPos = Vector3.zero;
        List<BorderEntityData> livingRoomFloorPosArr = GenerateRoomData.GetInstance.listRoomBuilderInfo.FindAll((p) =>
        {
            return p.entityModelType == EntityModelType.Floor && p.listRoomType.Contains(RoomType.LivingRoom);
        });
        if (livingRoomFloorPosArr != null)
        {
            int randomValue = UnityEngine.Random.Range(0, livingRoomFloorPosArr.Count);
            BorderEntityData livingRoomFloorPos = livingRoomFloorPosArr[randomValue];
            if (livingRoomFloorPos != null)
            {
                Vector2 originOffset = GetOriginOffset();
                targetPos = new Vector3(livingRoomFloorPos.pos.x + originOffset.x + 0.5f, 0, livingRoomFloorPos.pos.y + originOffset.y + 0.5f);
            }
        }
        robot?.gameObject.SetActive(false);
        robot.transform.position = targetPos;
        //robot.transform.Find("MeshContainer").transform.localPosition = Vector3.zero;
        robot?.gameObject.SetActive(true);
    }
    #endregion


    #endregion

    #region 缓存房间内实体信息
    /// <summary>
    /// 初始化创建房间内实体信息对象
    /// </summary>
    public void InitCreateCacheItemDataInfo()
    {
        //清理实体缓存信息
        List<GetThingGraph_data_items> items = new List<GetThingGraph_data_items>();
        MainData.CacheSceneItemsInfo = new PostThingGraph
        {
            items = items,
            id = MainData.SceneID,
        };
        Transform ItemEntityGroupNode = GenerateRoomItemModel.GetInstance.ItemEntityGroupNode;
        //遍历所有房间
        for (int i = 0; i < ItemEntityGroupNode.childCount; i++)
        {
            if (!ItemEntityGroupNode.GetChild(i).gameObject.activeSelf)
            {
                continue;
            }
            string roomName = ItemEntityGroupNode.GetChild(i).name.Split('_')?[0];
            string roomID = ItemEntityGroupNode.GetChild(i).name.Split('_')?[1];
            GetThingGraph_data_items item = new GetThingGraph_data_items()
            {
                id = roomID,
                name = roomName,
                position = new float[] { 0, 0, 0 },
                rotation = new float[] { 0, 0, 0 },
                relatedThing = new List<GetThingGraph_data_items_relatedThing>(),
                dynamic = false,
            };
            //遍历第一层物体
            for (int j = 0; j < ItemEntityGroupNode.GetChild(i).childCount; j++)
            {
                if (!ItemEntityGroupNode.GetChild(i).GetChild(j).gameObject.activeSelf)
                {
                    continue;
                }
                Transform curNode = ItemEntityGroupNode.GetChild(i).GetChild(j);
                GetThingGraph_data_items_relatedThing_target target = null;
                CacheItemDataInfo(curNode, ref target);
                item.relatedThing.Add(new GetThingGraph_data_items_relatedThing
                {
                    target = target,
                    relationship = "In"
                });
            }
            //手动添加该房间的实体门信息
            List<BorderEntityData> doorDataArr = GenerateRoomData.GetInstance.GetDoorInfoByRoomType((RoomType)Enum.Parse(typeof(RoomType), roomName), roomID);
            foreach (BorderEntityData doorData in doorDataArr)
            {
                Debugger.Log(doorData);
                string doorID = doorData.entity?.name;
                string doorName = roomName + "Door";
                Transform modelTrans = doorData.entity.transform.Find("Model")?.transform;
                //doorData.entity.name = doorName + "_" + doorID;


                item.relatedThing.Add(new GetThingGraph_data_items_relatedThing
                {
                    target = new GetThingGraph_data_items_relatedThing_target
                    {
                        id = doorID,
                        name = doorName,
                        relatedThing = null,
                        dynamic = true,
                        position = new float[] { modelTrans.position.x, modelTrans.position.y, modelTrans.position.z },
                        rotation = new float[] { modelTrans.rotation.eulerAngles.x, modelTrans.rotation.eulerAngles.y, modelTrans.rotation.eulerAngles.z },
                    },
                    relationship = "In"
                });

            }
            MainData.CacheSceneItemsInfo.items.Add(item);
        }
    }
    //缓存数据 递归遍历
    private void CacheItemDataInfo(Transform curNode, ref GetThingGraph_data_items_relatedThing_target curTarget)
    {
        string itemName = curNode.name.Split('_')?[0];
        string itemID = curNode.name.Split('_')?[1];
        Transform curModelEntityNoded = curNode.Find("Model");
        curTarget = new GetThingGraph_data_items_relatedThing_target
        {
            position = new float[] { curModelEntityNoded.position.x, curModelEntityNoded.position.y, curModelEntityNoded.position.z },
            rotation = new float[] { curModelEntityNoded.rotation.eulerAngles.x, curModelEntityNoded.rotation.eulerAngles.y, curModelEntityNoded.rotation.eulerAngles.z },
            id = itemID,
            name = itemName,
            relatedThing = new List<GetThingGraph_data_items_relatedThing>(),
            dynamic = !curNode.gameObject.isStatic
        };
        //判断是否有下一次节点
        Transform putAreaTrans = curNode.Find("PutArea");
        if (putAreaTrans != null)
        {
            for (int k = 0; k < putAreaTrans.childCount; k++)
            {
                //节点关系对象 In On Below Above
                Transform nodeRelation = putAreaTrans.GetChild(k);
                for (int m = 0; m < nodeRelation.childCount; m++)
                {
                    Transform curNode2 = nodeRelation.GetChild(m);
                    GetThingGraph_data_items_relatedThing_target nextTarget = null;

                    CacheItemDataInfo(curNode2, ref nextTarget);
                    curTarget.relatedThing.Add(new GetThingGraph_data_items_relatedThing
                    {
                        relationship = nodeRelation.name,
                        target = nextTarget
                    });
                }
            }
        }
    }

    /// <summary>
    /// 原点偏移量，老原点位置在客厅的左下角，新原点需要在全局房间的左下角位置
    /// </summary>
    /// <returns></returns>
    public Vector2 GetOriginOffset(List<RoomInfo> roomInfos = null)
    {
        //找到新原点  所有房间的minX和minY作为新原点
        int offsetX = 0, offsetY = 0;
        if (roomInfos == null)
        {
            roomInfos = GenerateRoomData.GetInstance.m_ListRoomInfo;
        }
        foreach (RoomInfo ri in roomInfos)
        {
            if (ri.roomPosMin.x < offsetX)
            {
                offsetX = (int)ri.roomPosMin.x;
            }
            if (ri.roomPosMin.y < offsetY)
            {
                offsetY = (int)ri.roomPosMin.y;
            }
        }
        return new Vector3(-offsetX, -offsetY);
    }
    #endregion

    #region SendMsgGenerateInfo

    /// <summary>
    /// 提交场景图布局，房间与房间位置关系
    /// </summary>
    private void SendRoomInfoData(Vector2 originOffset)
    {
        RoomInfoData roomInfoData = new RoomInfoData
        {
            roomInfos = new List<RoomInfoData.RoomInfoData_roomInfos>()
        };
        for (int i = 0; i < GenerateRoomData.GetInstance.m_ListRoomInfo.Count; i++)
        {
            roomInfoData.roomInfos.Add(new RoomInfoData.RoomInfoData_roomInfos
            {
                roomType = GenerateRoomData.GetInstance.m_ListRoomInfo[i].roomType.ToString(),
                id = GenerateRoomData.GetInstance.m_ListRoomInfo[i].roomID,
                minPos = new int[] { (int)GenerateRoomData.GetInstance.m_ListRoomInfo[i].roomPosMin.x + (int)originOffset.x, (int)GenerateRoomData.GetInstance.m_ListRoomInfo[i].roomPosMin.y + (int)originOffset.y },
                maxPos = new int[] { (int)GenerateRoomData.GetInstance.m_ListRoomInfo[i].roomPosMax.x + (int)originOffset.x, (int)GenerateRoomData.GetInstance.m_ListRoomInfo[i].roomPosMax.y + (int)originOffset.y }
            });
        }
        InterfaceDataCenter.GetInstance.SendMQTTRoomInfoData(roomInfoData);
    }

    /// <summary>
    /// 更新全局场景图实体信息
    /// </summary>
    IEnumerator UpadeteSceneEntityInfo()
    {
        while (CanUpadeteSceneEntityInfo)
        {

            UpdateEnityInfoTool.GetInstance.UpdateSceneEntityInfo();

            //存档
            DataSave.GetInstance.SaveGetThingGraph_data_items(MainData.CacheSceneItemsInfo);

            InterfaceDataCenter.GetInstance.SendMQTTUpdateScenes(MainData.CacheSceneItemsInfo);
            yield return new WaitForSeconds(MainData.ConfigData.CoreConfig.SendEntityInfoHZ);
        }
    }
    /// <summary>
    /// 更新机器人第一视角相机视野内场景实体信息
    /// </summary>
    IEnumerator UpadeteRobotFirstCameraEntityInfo()
    {
        while (true)
        {
            yield return new WaitForSeconds(MainData.ConfigData.CoreConfig.SendEntityInfoHZ);

            UpdateEnityInfoTool.GetInstance.UpdateCameraFOVEntityInfo();
            InterfaceDataCenter.GetInstance.SendMQTTUpdateCamera(MainData.CacheCameraItemsInfo);
        }
    }
    #endregion

    #region Other
    private void HideDebugger()
    {
        m_Debugger = GameObject.Find("Debugger");
        m_Debugger?.SetActive(false);
    }

    #endregion

    private void OnDestroy()
    {
        NetworkMqtt.GetInstance.DisConnect();
    }

    public void Update()
    {
        if (Input.GetKey(KeyCode.F2))
        {
            string testJson =
                 "{\"test\":\"" + System.DateTime.Now.ToString("HHmmss") + "\"}";

            NetworkMqtt.GetInstance.Publish(InterfaceDataCenter.TOPIC_SEND,
                testJson);
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            string testJson =
                //"{\"entityInfo\":[{\"id\":\"sim:20\",\"type\":\"Book\",\"modelId\":\"Book_1\",\"pos\":{\"x\":4.5,\"y\":13.4},\"roomInfo\":{\"roomType\":\"LivingRoom\",\"roomID\":\"sim:3\"},\"parentEntityInfo\":{\"id\":\"sim:7\",\"type\":\"BIN\",\"PutArea\":\"In\"}}]}"
                //"{\r\n    \"entityInfo\": [\r\n        {\r\n            \"id\": \"sim:2\",\r\n            \"type\": \"FOOD\",\r\n            \"modelId\": \"Food_1\",\r\n            \"pos\": {\r\n                \"x\": 7,\r\n                \"y\": 7\r\n            },\r\n            \"dynamic\": -1,\r\n            \"putArea\": \"In\",\r\n            \"roomInfo\": {\r\n                \"roomType\": \"LivingRoom\",\r\n                \"roomID\": \"sim:10\"\r\n            },\r\n            \"parentEntityInfo\": {\r\n                \"id\": \"sim:8\",\r\n                \"type\": \"POT\"\r\n            }\r\n        }\r\n    ]\r\n}"
                //"{\"entityInfo\":[{\"id\":\"sim:8\",\"type\":\"Book\",\"modelId\":\"Book_1\",\"pos\":{\"x\":8.033,\"y\":5.967},\"dynamic\":0,\"roomInfo\":{\"roomType\":\"LivingRoom\",\"roomID\":\"sim:1\"},\"PutArea\":\"On\",\"parentEntityInfo\":{\"id\":\"sim:6\",\"type\":\"BED\"}}]}"
                "{\"entityInfo\":[{\"id\":\"sim:10\",\"type\":\"Desk\",\"modelId\":\"Desk_1\",\"pos\":{\"x\":1,\"y\":7},\"dynamic\":0,\"roomInfo\":{\"roomType\":\"LivingRoom\",\"roomID\":\"sim:3\"},\"putArea\":\"In\",\"parentEntityInfo\":{}}]}"
                ;

            NetworkMqtt.GetInstance.Publish(InterfaceDataCenter.TOPIC_ADD_GOODS,
                testJson);
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            string testJson =
                //"{\r\n    \"entityInfo\": [\r\n        {\r\n            \"id\": \"sim:9\",\r\n            \"type\": \"FOOD\",\r\n            \"delChind\":0\r\n        }\r\n    ]\r\n}"
                "{\r\n    \"entityInfo\": [\r\n        {\r\n            \"id\": \"sim:8\",\r\n            \"type\": \"POT\",\r\n            \"delChind\":0\r\n        }\r\n    ]\r\n}"
                ;
            NetworkMqtt.GetInstance.Publish(InterfaceDataCenter.TOPIC_DEL_GOODS,
        testJson);
        }
        if (Input.GetKeyDown(KeyCode.F7))
        {
            Debugger.Log("测试发送web端的房间布局变更mqtt消息");
            string testJson =
                @"
{
    ""sceneID"": """ + MainData.SceneID + @""",
    ""roomType"": """",
    ""roomID"": ""sim:6"",
    ""offsetPos"":{
            ""x"": " + x + @",
            ""y"": " + y + @"
    },
    ""ChangeTime"": ""20231123_165011""
}
";
            NetworkMqtt.GetInstance.Publish(InterfaceDataCenter.TOPIC_WEB_CHANGEPOSITION,
        testJson);
        }

        if (Input.GetKeyDown(KeyCode.F8))
        {
            string testJson =

                "{\r\n    \"motionId\" : \"motion://Grab_itemv\",\r\n    \"name\"     : \"Grab_itemv\",\r\n    \"stateMsg\" : \"suc\",\r\n    \"stateCode\" : 0,\r\n    \"simulatorId\" : \"\",\r\n    \"task_id\"     : null\r\n}"
                ;
            NetworkMqtt.GetInstance.Publish(InterfaceDataCenter.TOPIC_RECV,
        testJson);
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            //TaskCenter.GetInstance.TestSendOrder(Order.Grab_item, "Book", "sim:1027");
            //TaskCenter.GetInstance.TestSendOrder(Order.Open_Door_Inside, "DoorX", testDoorID);
            //TaskCenter.GetInstance.TestSendOrder(Order.Grab_item_pull, "Book", "sim:1027");
            //TaskCenter.GetInstance.TestSendOrder(Order.Close_Door_Inside, "DoorX", testDoorID);
            //TaskCenter.GetInstance.TestSendOrder(Order.Robot_CleanTable, "Desk", "sim:1025");
            //TaskCenter.GetInstance.TestSendOrder(Order.Press_Button, "TV", "sim:1016");
            //TaskCenter.GetInstance.TestSendOrder(Order.Knock_on_door, "DoorX", testDoorID);

            //TaskCenter.GetInstance.TestSendOrder(Order.Pull_Start, "BoxPull", "sim:1032");
            //TaskCenter.GetInstance.TestSendOrder(Order.Push_Enter, "BoxPash", "sim:1031");
            //TaskCenter.GetInstance.TestSendOrder(Order.Wheel, "Wheel", "sim:1033");
            //TaskCenter.GetInstance.TestSendOrder(Order.Pile, "Pile", "sim:1034");
            //TaskCenter.GetInstance.TestSendOrder(Order.Turn_Door, "DoorX", "3_7");

            TaskCenter.GetInstance.TestSendOrder(Order.Press_Button, "Sofa", "sim:1015");

        }
        if (Input.GetKeyDown(KeyCode.F10))
        {
            RobotAnimCenter robotAnimCenter = GameObject.FindObjectOfType<RobotAnimCenter>();
            robotAnimCenter.PlayAnimByBool("CanInteraction", true);
            robotAnimCenter.PlayAnimByName("Robot_Pick");
            robotAnimCenter.PlayAnimByBool("CanInteraction", false);
        }

        if (Input.GetKeyDown(KeyCode.F12))
        {
            if (m_Debugger != null)
            {
                m_Debugger.SetActive(!m_Debugger.gameObject.activeSelf);
            }
        }
    }
    public string testDoorID;
}

