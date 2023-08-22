using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFramework;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;
using static GenerateRoomData;
using System;
using static GetEnvGraph;
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
        UnityTool.GetInstance.DelayCoroutineWaitReturnTrue(() => { return m_IsLoadedAssets && MainData.getEnvGraph != null && MainData.getThingGraph != null; }, () =>
        {
            GenerateEntity(() =>
            {
                //场景实体生成完成后，通过mqtt协议上传场景实体数据到服务器，等待服务器决策指令
                Debug.Log("TODO 场景实体生成完成后，通过mqtt协议上传场景实体数据到服务器，等待服务器决策指令");

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
                GenerateRoomItemModel.GetInstance.GenerateRoomItem(k,MainData.getThingGraph);
                generateCompleteCallback?.Invoke();
            }
        });
    }

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

