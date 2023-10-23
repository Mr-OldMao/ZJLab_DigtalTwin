using MFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Feature_Robot_Pos;

/// <summary>
/// 标题：数字孪生程序主逻辑
/// 功能：
/// 作者：毛俊峰
/// 时间：2023.10.23
/// </summary>
public class GameLogic2 : SingletonByMono<GameLogic2>
{
    public GameObject robotPrefab;
    public GameObject peoplePrefab;
    public Dictionary<string, EntityInfo> m_DicEntityArray = new Dictionary<string, EntityInfo>();

    public class EntityInfo
    {
        public string id;
        public int type;
        public GameObject entity;
    }

    public void Init()
    {
        //异步加载ab资源
        LoadAssetsByAddressable.GetInstance.LoadAssetsAsyncByLable(new List<string> { "Scene2" }, () =>
        {
            Debug.Log("ab资源加载完毕回调");
            new ReadConfigFile(() =>
            {
                //接入网络通信 
                NetworkMQTT();
            });
        });


    }
    private void NetworkMQTT()
    {
        InterfaceDataCenter.GetInstance.InitMQTT();
    }

    private void Update()
    {
        if (MainData.feature_robot_pos?.Count > 0)
        {
            Feature_Robot_Pos feature_Robot_Pos = MainData.feature_robot_pos.Dequeue();
            DisposeRobotPos(feature_Robot_Pos);
        }
        if (MainData.feature_People_Perceptions?.Count > 0)
        {
            Feature_People_Perception feature_People_Perception = MainData.feature_People_Perceptions.Dequeue();
            DisposePeoplePerception(feature_People_Perception);
        }
    }

    /// <summary>
    /// 处理机器人坐标
    /// </summary>
    private void DisposeRobotPos(Feature_Robot_Pos feature_Robot_Pos)
    {
        GameObject entity = GetEntity(feature_Robot_Pos.robotId);
        if (entity != null)
        {
            Feature_Robot_Pos_data_feature transInfo = feature_Robot_Pos.data.feature;
            entity.transform.position = new Vector3(transInfo.position[0], transInfo.position[1], transInfo.position[2]);
            entity.transform.rotation = Quaternion.Euler(new Vector3(transInfo.orientation[0], transInfo.orientation[1], transInfo.orientation[2]));
        }
    }

    private GameObject GetEntity(string key)
    {
        GameObject res = null;
        if (m_DicEntityArray.ContainsKey(key))
        {
            res = m_DicEntityArray[key].entity;
        }
        else
        {
            GameObject prefab = LoadAssetsByAddressable.GetInstance.dicCacheAssets["RobotPrefab"]?.items[0];
            res = Instantiate(prefab);
            res.name = key;
            m_DicEntityArray.Add(key, new EntityInfo { entity = res });
        }
        return res;
    }

    /// <summary>
    /// 处理访客坐标信息
    /// </summary>
    private void DisposePeoplePerception(Feature_People_Perception feature_People_Perception)
    {

    }
}
