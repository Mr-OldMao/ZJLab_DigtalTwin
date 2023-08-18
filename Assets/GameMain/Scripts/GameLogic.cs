using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFramework;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;
using static GenerateRoomData;
using System;
/// <summary>
/// 标题：程序逻辑入口
/// 功能：程序主逻辑
/// 作者：毛俊峰
/// 时间：2023.08.18
/// </summary>
public class GameLogic : SingletonByMono<GameLogic>
{
    private bool m_IsLoadedAssets = false;
    private bool m_IsCompleteHttp = false;
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
        UnityTool.GetInstance.DelayCoroutineWaitReturnTrue(() => { return m_IsLoadedAssets && m_IsCompleteHttp; }, () =>
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
        //string url3 = "http://10.11.81.241:4006/simulator/getThingGraph";
        //MFramework.NetworkHttp.GetInstance.SendRequest(RequestType.Post, url3, new Dictionary<string, string>(), (string jsonStr) =>
        //{
        //    //JsonEntityThingGraph jsonEntityThingGraph = JsonTool.GetInstance.JsonToObjectByLitJson<JsonEntityThingGraph>(jsonStr);
        //    //ChacheNodeInfo(jsonEntityThingGraph);
        //    Debug.Log("recv callback jsonStr:" + jsonStr);
        //}, null, "{\"id\":\"test\"}");

        //string url4 = "http://10.11.81.241:4006/simulator/getEnvGraph";
        //MFramework.NetworkHttp.GetInstance.SendRequest(RequestType.Post, url4, new Dictionary<string, string>(), (string jsonStr) =>
        //{
        //    //JsonEntityThingGraph jsonEntityThingGraph = JsonTool.GetInstance.JsonToObjectByLitJson<JsonEntityThingGraph>(jsonStr);
        //    //ChacheNodeInfo(jsonEntityThingGraph);
        //    Debug.Log("recv callback jsonStr:" + jsonStr);
        //}, null, "{\"id\":\"test\"}");

        string url = "http://10.11.81.241:4006/brain/show/thingGraph";
        MFramework.NetworkHttp.GetInstance.SendRequest(RequestType.Post, url, new Dictionary<string, string> { }, (string jsonStr) =>
        {
            Debug.Log("recv callback jsonStr:" + jsonStr);

            m_IsCompleteHttp = true;
        });
    }
    private void NetworkMQTT()
    {
        //初始化并订阅主题
        NetworkMqtt.GetInstance.Init(new MqttConfig()).Subscribe(MqttTopicName.TopicTest);
        //监听消息回调
        NetworkMqtt.GetInstance.AddListener((object sender, MqttMsgPublishEventArgs e) =>
        {
            Debug.Log($"通过代理收到消息：{Encoding.UTF8.GetString(e.Message)}");
        });
        NetworkMqtt.GetInstance.AddListener((object sender, MqttMsgSubscribedEventArgs e) =>
        {
            Debug.Log($"客户端订阅消息成功回调 ，sender：{sender}");
        });
        //订阅多个主题
        NetworkMqtt.GetInstance.Subscribe("TopicTest1", "TopicTest2");
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
        List<RoomBaseInfo> roomBaseInfos = new()
        {
            new RoomBaseInfo
            {
                 curRoomType = RoomType.LivingRoom,
                 roomSize = new uint[]{ 4,6 },
                 targetRoomsDirRelation = new List<RoomsDirRelation>()
                 {
                     new RoomsDirRelation{ targetRoomType = RoomType.BedRoom , locationRelation = DirEnum.Right , isCommonWall = true},
                     new RoomsDirRelation{ targetRoomType = RoomType.KitChenRoom , locationRelation = DirEnum.Down  , isCommonWall = true},
                 }
            },
            new RoomBaseInfo
            {
                 curRoomType = RoomType.BedRoom,
                 roomSize = new uint[]{ 3,5 },
                 //targetRoomsDirRelation = new List<RoomsDirRelation>()
                 //{
                 //    new RoomsDirRelation{ targetRoomType = RoomType.LivingRoom , locationRelation = DirEnum.Left },
                 //}
            },
            new RoomBaseInfo
            {
                 curRoomType = RoomType.KitChenRoom,
                 roomSize = new uint[]{ 2,4 },
                 //targetRoomsDirRelation = new List<RoomsDirRelation>()
                 //{
                 //    new RoomsDirRelation{ targetRoomType = RoomType.LivingRoom , locationRelation = DirEnum.Up },
                 //}
            },
            new RoomBaseInfo
            {
                 curRoomType = RoomType.BathRoom,
                 roomSize = new uint[]{3,4 },
                 targetRoomsDirRelation = new List<RoomsDirRelation>()
                 {
                     new RoomsDirRelation{ targetRoomType = RoomType.LivingRoom , locationRelation = DirEnum.Right , isCommonWall = true},
                 }
            },
              new RoomBaseInfo
            {
                 curRoomType = RoomType.StudyRoom,
                 roomSize = new uint[]{2,3 },
                 targetRoomsDirRelation = new List<RoomsDirRelation>()
                 {
                     new RoomsDirRelation{ targetRoomType = RoomType.BathRoom , locationRelation = DirEnum.Right , isCommonWall = true},
                 }
            },
              new RoomBaseInfo
            {
                 curRoomType = RoomType.StorageRoom,
                 roomSize = new uint[]{3,2 },
                 targetRoomsDirRelation = new List<RoomsDirRelation>()
                 {
                     new RoomsDirRelation{ targetRoomType = RoomType.StudyRoom , locationRelation = DirEnum.Right , isCommonWall = true},
                 }
            },
        };
        GenerateRoomData.GetInstance.GenerateRandomRoomInfoData(roomBaseInfos, (p, k) =>
        {
            if (p == null || k == null)
            {
                Debug.LogError("helf random is fail");
            }
            else
            {
                GenerateRoomBorderModel.GetInstance.GenerateRoomBorder(p);
                GenerateRoomItemModel.GetInstance.GenerateRoomItem(k);
                generateCompleteCallback?.Invoke();
            }
        });
    }

    private void OnDestroy()
    {
        NetworkMqtt.GetInstance.DisConnect();
    }
}

