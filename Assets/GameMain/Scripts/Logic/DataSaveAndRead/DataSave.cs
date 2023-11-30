using System.Collections.Generic;
using UnityEngine;
using static GenerateRoomBorderModel;
using static GenerateRoomData;
using MFramework;
using System;
using static GetEnvGraph;
using static DataRead;
/// <summary>
/// 标题：存档
/// 功能：存储当前场景中必要的数据
/// 作者：毛俊峰
/// 时间：2023.11.03
/// </summary>
[Serializable]
public class DataSave : SingletonByMono<DataSave>
{
    [SerializeField]
    private List<RoomInfo> m_RoomInfos;
    [SerializeField]
    private List<RoomBaseInfo> m_RoomBaseInfos;
    [SerializeField]
    private List<BorderEntityData> m_BorderEntityDatas;
    [SerializeField]
    public PostThingGraph m_PostThingGraph;
    [SerializeField]
    private GetEnvGraph_data m_GetEnvGraphData; 
    [SerializeField]
    private List<RoomMatData> m_ListRoomMatData;




    private List<DataPackageInfo> m_DpList = new List<DataPackageInfo>();
    /// <summary>
    /// 存档场景，序列化为json到服务器
    /// </summary>
    /// <returns></returns>
    public bool Save(Action callbackSuc = null)
    {
        bool res = false;
        try
        {
            m_DpList.Clear();
            string saveTime = saveTime = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            DataPackage dataPackage = new DataPackage() { sceneID = MainData.SceneID, saveTime = saveTime };
            dataPackage.dataPackageInfo = new Serialization<DataPackageInfo>(m_DpList);

            Debugger.Log("存档开始 saveTime:" + saveTime);

            SerializeData("m_RoomInfosJson", m_RoomInfos);
            SerializeData("m_RoomBaseInfosJson", m_RoomBaseInfos);
            SerializeData("m_BorderEntityDatasJson", m_BorderEntityDatas);
            SerializeData("m_PostThingGraphJson", m_PostThingGraph);
            SerializeData("m_GetEnvGraphDataJson", m_GetEnvGraphData);
            //SerializeData("m_ListRoomMatData", m_ListRoomMatData);
            

            string targetJson = JsonUtility.ToJson(dataPackage, false);
            res = !string.IsNullOrEmpty(targetJson);
            Debugger.Log("存档中，序列化完毕，dataPackage " + dataPackage + ",targetjson：" + targetJson);

            //json文件存本地一份
            string fileName = "SaveScene_" + dataPackage.sceneID + "_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json";
            fileName= fileName.Replace(":", "");
            Debugger.Log("尝试本地存档 path：" + Application.streamingAssetsPath + "/" + fileName, LogTag.Forever);
            new FileIOTxt(Application.streamingAssetsPath, fileName).Write(targetJson);
            Debugger.Log("本地存档完毕 path：" + Application.streamingAssetsPath + "/" + fileName, LogTag.Forever);

            InterfaceDataCenter.GetInstance.SaveFileData(targetJson, callbackSuc);
        }
        catch (System.Exception e)
        {
            Debugger.LogError("save fail , e:" + e);
        }
        Debugger.Log("save , res : " + res);
        return res;
    }

    private void SerializeData<T>(string key, List<T> entityClass)
    {
        string json = JsonUtility.ToJson(new Serialization<T>(entityClass));
        m_DpList.Add(new DataPackageInfo { key = key, json = json });
        Debugger.Log(key + ", json :" + json);
    }
    private void SerializeData<T>(string key, T entityClass)
    {
        string json = JsonUtility.ToJson(new Serialization<T>(entityClass));
        m_DpList.Add(new DataPackageInfo { key = key, json = json });
        Debugger.Log(key + ", json :" + json);
    }

    //已经建造的房间信息
    public void SaveRoomInfos(List<RoomInfo> roomInfos)
    {
        m_RoomInfos = roomInfos;
    }

    //各个房间之间的邻接关系
    public void SaveRoomBaseInfos(List<RoomBaseInfo> roomBaseInfos)
    {
        m_RoomBaseInfos = roomBaseInfos;
    }

    //房间所有边界(墙、门、地板)数据
    public void SaveListRoomBuilderInfo(List<BorderEntityData> borderEntityDatas)
    {
        m_BorderEntityDatas = borderEntityDatas;
    }

    /// <summary>
    /// 保存所有实体放置的位置等信息
    /// </summary>
    public void SaveGetThingGraph_data_items(PostThingGraph postThingGraph)
    {
        m_PostThingGraph = postThingGraph;
    }

    /// <summary>
    /// 保存环境场景图,房间与房间的邻接关系
    /// </summary>
    /// <param name="getEnvGraph_Data"></param>
    public void SaveGetEnvGraph_data(GetEnvGraph_data getEnvGraph_Data)
    {
        m_GetEnvGraphData = getEnvGraph_Data;
    }

    /// <summary>
    /// 保存所有房间的地板、墙壁材质数据
    /// </summary>
    public void SaveGetThingGraph_data_items(List<RoomMatData> listRoomMatData)
    {
        m_ListRoomMatData = listRoomMatData;
    }
}

//序列化List类
[Serializable]
public class Serialization<T>
{
    [SerializeField]
    List<T> targetList;
    [SerializeField]
    T target;

    public List<T> TargetToList() { return targetList; }
    public T Target() { return target; }

    public Serialization(List<T> targetList)
    {
        this.targetList = targetList;
    }
    public Serialization(T target)
    {
        this.target = target;
    }
}

/// <summary>
/// 读档存档 数据包
/// </summary>
[Serializable]
public class DataPackage
{
    public string sceneID;
    public string saveTime;
    public Serialization<DataPackageInfo> dataPackageInfo;
}

[Serializable]
public struct DataPackageInfo
{
    public string key;
    public string json;
}
