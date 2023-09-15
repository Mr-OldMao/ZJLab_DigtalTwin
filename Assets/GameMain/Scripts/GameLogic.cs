using System.Collections.Generic;
using UnityEngine;
using MFramework;
using static GenerateRoomData;
using System;
using static GetEnvGraph;
using static GetThingGraph;
using static GenerateRoomBorderModel;
using Unity.VisualScripting;
using UnityEngine.AI;
using System.Collections;
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

    public void Init()
    {
        Debug.Log("Init GameLogic");
        this.EnterMainScene();
    }

    private void EnterMainScene()
    {
        CreateRootNode();

        //注册消息事件
        RegisterMsgEvent();

        //异步加载ab资源
        LoadAssetsByAddressable.GetInstance.LoadAssetsAsyncByLable(new List<string> { "ItemLable", "RoomBorderLable", "RobotEntity", "UIForm" }, () =>
        {
            Debug.Log("ab资源加载完毕回调");
            m_IsLoadedAssets = true;
        });

        //接入网络通信
        NetworkHTTP();
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

    private void NetworkHTTP()
    {
        InterfaceDataCenter.GetInstance.CacheGetThingGraph("test");
        InterfaceDataCenter.GetInstance.CacheGetEnvGraph("test");
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
            //原点偏移至场景左下角
            Vector2 originOffset = GetOriginOffset();
            staticModelRootNode.transform.position = new Vector3(originOffset.x, 0, originOffset.y);

            //生成机器人实体
            GenerateRobot();

            //监听“门”碰撞事件
            ListenerDoorCollEvent(true);

            //初始化相机
            CameraControl.GetInstance.Init();

            //缓存所有实体物品数据信息
            CacheItemDataInfo();

            //提交场景图，物体与房间的邻接关系
            InterfaceDataCenter.GetInstance.CommitGetThingGraph(MainData.CacheItemsInfo, () =>
            {
                //更新仿真引擎状态，从而获取服务器指令
                InterfaceDataCenter.GetInstance.ChangeProgramState("test", ProgramState.start);
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
        });
    }

    private void ListenerDoorCollEvent(bool canListener)
    {
        foreach (var item in GameObject.FindObjectsOfType<AnimDoor>())
        {
            item.CanListenerDoorColl = canListener;
        }
    }
    #endregion

    #region Generate
    private void GenerateEntity(Action generateCompleteCallback)
    {
        //写入各个房间之间的邻接关系
        List<RoomBaseInfo> roomBaseInfos = new List<RoomBaseInfo>();
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
                        roomSize = new uint[] { (uint)UnityEngine.Random.Range(4, 8), (uint)UnityEngine.Random.Range(4, 8) }
                    });
                }
            }
        }
        GenerateRoomData.GetInstance.GenerateRandomRoomInfoData(roomBaseInfos, (p, k) =>
        {
            if (p == null || k == null)
            {
                Debug.LogError("generage fail , again generate...");
                GenerateScene();
            }
            else
            {
                GenerateRoomBorderModel.GetInstance.GenerateRoomBorder();
                GenerateRoomItemModel.GetInstance.GenerateRoomItem(k, MainData.getThingGraph);
                generateCompleteCallback?.Invoke();
            }
        });
    }


    public void GenerateScene()
    {
        staticModelRootNode.transform.position = Vector3.zero;
        ListenerDoorCollEvent(false);
        //生成场景中所有房间和物品
        GenerateEntity(() =>
        {
            MsgEvent.SendMsg(MsgEventName.GenerateSceneComplete);
        });
    }


    #region Robot
    public void GenerateRobot()
    {
        GameObject robotEntity = GameObject.FindWithTag("Player");
        if (robotEntity != null)
        {
            Destroy(robotEntity);
        }

        if (robotEntity == null)
        {
            GameObject robotRes = LoadAssetsByAddressable.GetInstance.GetEntityRes("RobotEntity", 0);
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
                targetPos = new Vector3(livingRoomFloorPos.pos.x + originOffset.x, 0, livingRoomFloorPos.pos.y + originOffset.y);
            }
        }
        robot?.gameObject.SetActive(false);
        robot.transform.position = targetPos;
        robot?.gameObject.SetActive(true);
    
    }
    #endregion


    #endregion

    #region 缓存房间内实体信息
    //缓存房间内实体信息
    private void CacheItemDataInfo()
    {
        //清理实体缓存信息
        List<GetThingGraph_data_items> items = new List<GetThingGraph_data_items>();
        MainData.CacheItemsInfo = new PostThingGraph
        {
            items = items,
            id = "test",
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
            List<BorderEntityData> doorDataArr = GenerateRoomData.GetInstance.GetDoorInfoByRoomType((RoomType)Enum.Parse(typeof(RoomType), roomName));
            foreach (BorderEntityData doorData in doorDataArr)
            {
                item.relatedThing.Add(new GetThingGraph_data_items_relatedThing
                {
                    target = new GetThingGraph_data_items_relatedThing_target
                    {
                        id = doorData.entity?.name,
                        name = roomName + "Door",
                        relatedThing = null,
                        dynamic = true,
                        position = new float[] { doorData.entity.transform.position.x, doorData.entity.transform.position.y, doorData.entity.transform.position.z },
                        rotation = new float[] { doorData.entity.transform.rotation.eulerAngles.x, doorData.entity.transform.rotation.eulerAngles.y, doorData.entity.transform.rotation.eulerAngles.z },
                    },
                    relationship = "In"
                });
            }
            MainData.CacheItemsInfo.items.Add(item);
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
    private Vector2 GetOriginOffset()
    {
        //找到新原点  所有房间的minX和minY作为新原点
        int offsetX = 0, offsetY = 0;
        List<RoomInfo> roomData = GenerateRoomData.GetInstance.m_ListRoomInfo;
        foreach (RoomInfo ri in roomData)
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
        while (true)
        {
            yield return new WaitForSeconds(1f);

            InterfaceDataCenter.GetInstance.SendMQTTUpdateScenes(MainData.CacheItemsInfo);
        }
    }
    /// <summary>
    /// 更新机器人第一视角相机视野内场景实体信息
    /// </summary>
    IEnumerator UpadeteRobotFirstCameraEntityInfo()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            GetThingGraph_data_items[] getThingGraph_Data_Items = new GetThingGraph_data_items[] { };
            InterfaceDataCenter.GetInstance.SendMQTTUpdateCamera(getThingGraph_Data_Items);
        }
    }
    #endregion



    private void OnDestroy()
    {
        NetworkMqtt.GetInstance.DisConnect();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F7))
        {
            UIManager.GetInstance.Show<UIFormMain>();
            GenerateScene();
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            InterfaceDataCenter.GetInstance.ChangeProgramState("test", ProgramState.start);
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            NetworkMqtt.GetInstance.Publish("TopicTest", "msg from unity " + System.DateTime.Now);
            NetworkMqtt.GetInstance.Publish(InterfaceDataCenter.TOPIC_RECV, "msg from unity " + System.DateTime.Now);
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            InterfaceDataCenter.GetInstance.SendMQTTRoomInfoData(new RoomInfoData
            {
                roomInfos = new List<RoomInfoData.RoomInfoData_roomInfos>
                 {
                     new RoomInfoData.RoomInfoData_roomInfos()
                     {
                            id = "1",
                            roomType = "LivingRoom",
                            minPos = new int[] { 1,2},
                            maxPos = new int[] { 5,7}
                     },
                       new RoomInfoData.RoomInfoData_roomInfos()
                     {
                            id = "2",
                            roomType = "BedRoom",
                            minPos = new int[] { 6,5},
                            maxPos = new int[] { 9,8}
                     },
                 }
            });
        }
    }
}

