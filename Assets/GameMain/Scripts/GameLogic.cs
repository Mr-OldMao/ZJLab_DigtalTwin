using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFramework;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;
using static GenerateRoomData;
using System;
using static GetEnvGraph;
using static GenerateRoomItemModel;
using static GetThingGraph;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
using static UnityEditor.Progress;
/// <summary>
/// 标题：程序逻辑入口
/// 功能：程序主逻辑
/// 作者：毛俊峰
/// 时间：2023.08.18
/// </summary>
public class GameLogic : SingletonByMono<GameLogic>
{
    private bool m_IsLoadedAssets = false;
    public void Init()
    {
        Debug.Log("Init GameLogic");
        this.EnterMainScene();
    }

    private void EnterMainScene()
    {
        //注册消息事件
        RegisterMsgEvent();

        //异步加载ab资源
        ResourcesLoad.GetInstance.AsyncLoadAllResources();

        //接入网络通信
        NetworkHTTP();
        NetworkMQTT();

        //等待ab资源加载完毕，以及http接口获取的场景数据，解析生成场景实体
        UnityTool.GetInstance.DelayCoroutineWaitReturnTrue(() =>
        {

            return m_IsLoadedAssets && MainData.getEnvGraph != null && MainData.getThingGraph != null;
        }, () =>
        {
            //生成场景中所有房间和物品
            GenerateEntity(() =>
            {
                //缓存所有实体物品数据信息
                CacheItemDataInfo();

                //提交场景图，物体与房间的邻接关系
                InterfaceDataCenter.GetInstance.CommitGetThingGraph(MainData.CacheItemsInfo);

                //根据服务器决策指令，控制机器人的行为
            });
        });
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

    private void RegisterMsgEvent()
    {
        MsgEvent.RegisterMsgEvent(MsgEventName.AsyncLoadedComplete, () =>
        {
            Debug.Log("ab资源加载完毕回调");
            m_IsLoadedAssets = true;
        });
    }

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
            //当前房间与其他房间邻接关系
            for (int j = 0; j < roomRelation.relatedThing?.Length; j++)
            {
                RoomType targetRoomType = (RoomType)Enum.Parse(typeof(RoomType), roomRelation.relatedThing[j].target.name);
                roomBaseInfo.targetRoomsDirRelation.Add(new RoomsDirRelation
                {
                    targetRoomType = targetRoomType,
                    locationRelation = (DirEnum)Enum.Parse(typeof(DirEnum), roomRelation.relatedThing[j].relationship),
                    isCommonWall = true
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
                if (roomBaseInfos.Find((p) => { return p.curRoomType == targetRoomType; }) == null)
                {
                    roomBaseInfos.Add(new RoomBaseInfo
                    {
                        curRoomType = targetRoomType,
                        roomSize = new uint[] { (uint)UnityEngine.Random.Range(4, 8), (uint)UnityEngine.Random.Range(4, 8) }
                    });
                }
            }
        }
        GenerateRoomData.GetInstance.GenerateRandomRoomInfoData(roomBaseInfos, (p, k) =>
        {
            if (p == null || k == null)
            {
                Debug.LogError("helf random is fail");
            }
            else
            {
                GenerateRoomBorderModel.GetInstance.GenerateRoomBorder(p);
                GenerateRoomItemModel.GetInstance.GenerateRoomItem(k, MainData.getThingGraph);
                generateCompleteCallback?.Invoke();
            }
        });
    }

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
            MainData.CacheItemsInfo.items.Add(item);
        }
    }
    //缓存数据 递归遍历
    private void CacheItemDataInfo(Transform curNode, ref GetThingGraph_data_items_relatedThing_target curTarget)
    {
        string itemName = curNode.name.Split('_')?[0];
        string itemID = curNode.name.Split('_')?[1];
        curTarget = new GetThingGraph_data_items_relatedThing_target
        {
            position = new float[] { curNode.position.x, curNode.position.y, curNode.position.z },
            rotation = new float[] { curNode.rotation.eulerAngles.x, curNode.rotation.eulerAngles.y, curNode.rotation.eulerAngles.z },
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
    #endregion




    private void OnDestroy()
    {
        NetworkMqtt.GetInstance.DisConnect();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            if (m_IsLoadedAssets && MainData.getEnvGraph != null && MainData.getThingGraph != null)
            {
                GenerateEntity(null);
            }
        }
    }
}

