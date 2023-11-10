using System.Collections.Generic;
using UnityEngine;
using static GenerateRoomBorderModel;
using static GenerateRoomData;
using MFramework;
using System;
using static GetEnvGraph;
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
    //[SerializeField]
    //public List<NewRoomInfo> newRoomInfos;

    private List<DataPackageInfo> m_DpList = new List<DataPackageInfo>();
    /// <summary>
    /// 存档场景，序列化为json到服务器
    /// </summary>
    /// <returns></returns>
    public bool Save()
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

            string targetJson = JsonUtility.ToJson(dataPackage, false);
            res = !string.IsNullOrEmpty(targetJson);
            Debugger.Log("存档中，序列化完毕，dataPackage " + dataPackage + ",targetjson：" + targetJson);

            //json文件存本地一份
            string fileName = "SaveScene_" + dataPackage.sceneID + "_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json";
            Debugger.Log("尝试本地存档 path：" + Application.streamingAssetsPath + "/" + fileName, LogTag.Forever);
            new FileIOTxt(Application.streamingAssetsPath, fileName).Write(targetJson);
            Debugger.Log("本地存档完毕 path：" + Application.streamingAssetsPath + "/" + fileName, LogTag.Forever);

            InterfaceDataCenter.GetInstance.SaveFileData(targetJson);
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

    ///// <summary>
    ///// 由于部分字段无法序列化，把部分字段做调整
    ///// </summary>
    //private List<NewRoomInfo> ChangeRoomInfoData(List<RoomInfo> roomInfos)
    //{
    //    List<NewRoomInfo> res = new List<NewRoomInfo>();
    //    for (int i = 0; i < roomInfos?.Count; i++)
    //    {
    //        List<NewBorderEntityData> newBorderEntityDatas = new List<NewBorderEntityData>();
    //        List<NewBorderEntityData> newListEmptyPosInfo = new List<NewBorderEntityData>();
    //        for (int j = 0; j < roomInfos[i].listDoorPosInfo?.Count; j++)
    //        {
    //            NewBorderEntityData newBorderEntityData = new NewBorderEntityData
    //            {
    //                entity = roomInfos[i].listDoorPosInfo[j].entity,
    //                pos = new float[2] { roomInfos[i].listDoorPosInfo[j].pos.x, roomInfos[i].listDoorPosInfo[j].pos.y },
    //                entityModelType = roomInfos[i].listDoorPosInfo[j].entityModelType.ToString(),
    //                entityAxis = roomInfos[i].listDoorPosInfo[j].entityAxis,
    //                borderDir = roomInfos[i].listDoorPosInfo[j].borderDir.ToString(),
    //                listRoomType = new List<string>(),
    //                listRoomTypeID = roomInfos[i].listDoorPosInfo[j].listRoomTypeID
    //            };
    //            for (int k = 0; k < roomInfos[i].listDoorPosInfo[j].listRoomType?.Count; k++)
    //            {
    //                newBorderEntityData.listRoomType.Add(roomInfos[i].listDoorPosInfo[j].listRoomType[k].ToString());
    //            }
    //            newBorderEntityDatas.Add(newBorderEntityData);
    //        }
    //        for (int j = 0; j < roomInfos[i].listEmptyPosInfo?.Count; j++)
    //        {
    //            NewBorderEntityData newListEmpty = new NewBorderEntityData
    //            {
    //                entity = roomInfos[i].listEmptyPosInfo[j].entity,
    //                pos = new float[2] { roomInfos[i].listEmptyPosInfo[j].pos.x, roomInfos[i].listEmptyPosInfo[j].pos.y },
    //                entityModelType = roomInfos[i].listEmptyPosInfo[j].entityModelType.ToString(),
    //                entityAxis = roomInfos[i].listEmptyPosInfo[j].entityAxis,
    //                borderDir = roomInfos[i].listEmptyPosInfo[j].borderDir.ToString(),
    //                listRoomType = new List<string>(),
    //                listRoomTypeID = roomInfos[i].listEmptyPosInfo[j].listRoomTypeID
    //            };
    //            for (int k = 0; k < roomInfos[i].listEmptyPosInfo[j].listRoomType?.Count; k++)
    //            {
    //                newListEmpty.listRoomType.Add(roomInfos[i].listEmptyPosInfo[j].listRoomType[k].ToString());
    //            }
    //            newListEmptyPosInfo.Add(newListEmpty);
    //        }
    //        res.Add(new NewRoomInfo
    //        {
    //            roomType = roomInfos[i].roomType.ToString(),
    //            roomID = roomInfos[i].roomID,
    //            roomSize = roomInfos[i].roomSize,
    //            roomPosMin = new float[2] { roomInfos[i].roomPosMin.x, roomInfos[i].roomPosMin.y },
    //            roomPosMax = new float[2] { roomInfos[i].roomPosMax.x, roomInfos[i].roomPosMax.y },
    //            listDoorPosInfo = newBorderEntityDatas,
    //            listEmptyPosInfo = newListEmptyPosInfo
    //        });
    //    }
    //    return res;
    //}


    //[Serializable]
    //public class NewRoomInfo
    //{
    //    public string roomType;//New
    //    public string roomID;
    //    /// <summary>
    //    /// 房间长宽，单位米(正整数)  PS：暂时没用上，后面考虑删除此字段
    //    /// </summary>
    //    public uint[] roomSize;
    //    /// <summary>
    //    /// 房间左下世界坐标位置(整数)
    //    /// </summary>
    //    public float[] roomPosMin;//New
    //    /// <summary>
    //    /// 房间右上世界坐标位置(整数)
    //    /// </summary>
    //    public float[] roomPosMax;//New
    //    /// <summary>
    //    /// 当前房间门的世界坐标位置(整数)
    //    /// </summary>
    //    public List<NewBorderEntityData> listDoorPosInfo;
    //    /// <summary>
    //    /// 当前房间空墙的世界坐标位置(整数)
    //    /// </summary>
    //    public List<NewBorderEntityData> listEmptyPosInfo;
    //}
    ///// <summary>
    ///// 具体边界实体数据信息
    ///// </summary>
    //[Serializable]
    //public class NewBorderEntityData
    //{
    //    public GameObject entity = null;
    //    public float[] pos;//New
    //    public string entityModelType; //New
    //    /// <summary>
    //    /// 边界实体轴向，0-横向 1-纵向
    //    /// </summary>
    //    public int entityAxis;

    //    /// <summary>
    //    /// 边界实体方位，相对于当前房间中心点的方位
    //    /// </summary>
    //    public string borderDir;

    //    /// <summary>
    //    /// 当前实体所属房间类型，一个边界实体最多可属于两个房间，listRoomType.count<=2
    //    /// </summary>
    //    public List<string> listRoomType; //New

    //    /// <summary>
    //    /// 当前实体所属房间类型ID,可代替listRoomType使用，一个边界实体最多可属于两个房间，listRoomType.count<=2
    //    /// </summary>
    //    public List<string> listRoomTypeID;
    //}

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
