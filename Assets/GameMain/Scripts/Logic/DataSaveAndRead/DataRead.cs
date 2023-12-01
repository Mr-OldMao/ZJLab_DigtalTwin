using System.Collections.Generic;
using UnityEngine;
using MFramework;
using static GenerateRoomData;
using static GenerateRoomBorderModel;
using static GetEnvGraph;
using System;

/// <summary>
/// 标题：读档
/// 功能：
/// 作者：毛俊峰
/// 时间：2023.11.03
/// </summary>
public class DataRead : SingletonByMono<DataRead>
{
    private List<RoomInfo> m_RoomInfos;
    private List<RoomBaseInfo> m_RoomBaseInfos;
    private List<BorderEntityData> m_BorderEntityDatas;
    private PostThingGraph m_PostThingGraph;
    private GetEnvGraph_data m_GetEnvGraphData;
    private List<RoomMatData> m_ListRoomMatData;



    /// <summary>
    /// 预读取所有数据 根据服务器场景ID
    /// </summary>
    /// <param name="callback"> p-是否读取成功</param>
    public void ReadAllDataByServerSceneID(Action<bool> callback)
    {
        //根据sceneID从服务器获取核心数据
        InterfaceDataCenter.GetInstance.ReadFileData(MainData.SceneID, (ReadFileData readFileData) =>
        {
            try
            {
                if (readFileData == null)
                {
                    Debugger.LogError("尝试服务器读档失败，readFileData is null");
                    callback.Invoke(false);
                    return;
                }
                if (readFileData?.data == null)
                {
                    Debugger.LogError("尝试服务器读档失败，readFileData.data is null");
                    callback.Invoke(false);
                    return;
                }
                if (readFileData?.data.dataPackageInfo == null)
                {
                    Debugger.LogError("尝试服务器读档失败，readFileData.data.dataPackageInfo is null");
                    callback.Invoke(false);
                    return;
                }
                DataPackage dataPackage = readFileData?.data;
                try
                {
                    ParseDataPackage(dataPackage);
                    Debugger.Log("解析服务器读档成功");
                    callback.Invoke(true);
                    //if (dataPackage.dataPackageInfo.TargetToList().Count == 6)
                    //{
                    //    Debugger.Log("解析服务器读档成功");
                    //    callback.Invoke(true);
                    //}
                    //else
                    //{
                    //    Debugger.LogError("服务器读档失败 数据数量有误：" + dataPackage.dataPackageInfo.TargetToList().Count);
                    //    callback.Invoke(false);
                    //}
                }
                catch (Exception e)
                {
                    Debugger.LogError("解析服务器读档失败，e：" + e);
                    callback.Invoke(false);
                }
            }
            catch (Exception e)
            {
                Debugger.LogError("尝试服务器读档失败，e:" + e);
                callback.Invoke(false);
            }
        }, () =>
        {
            callback?.Invoke(false);
        });
    }

    /// <summary>
    /// 本地预读取所有数据 根据本地文件
    /// </summary>
    /// <param name="callback"> p-是否读取成功</param>
    public void ReadAllDataByLocalFile(string rootPath, string fileName, Action<bool> callback)
    {
        //string jsonData = new MFramework.FileIOTxt(rootPath, fileName).Read();
        //bool parseSuc = ParseJsonData(jsonData);
        //callback(parseSuc);

        string path = rootPath + "/" + fileName;
#if !UNITY_WEBGL
        path = "file://" + path;
#endif
        UnityTool.GetInstance.DownLoadAssetsByURL<string>(path, (jsonData) =>
        {
            bool parseSuc = TryParseLocalJsonData(jsonData);
            callback(parseSuc);
        }, () =>
        {
            callback(false);
        });
    }

    /// <summary>
    /// 尝试解析本地json
    /// </summary>
    /// <param name="jsonData"></param>
    /// <returns></returns>
    private bool TryParseLocalJsonData(string jsonData)
    {
        bool res = false;
        Debugger.Log("读档 解析json数据 jsonData：" + jsonData);
        try
        {
            DataPackage dataPackage = JsonUtility.FromJson<DataPackage>(jsonData);
            ParseDataPackage(dataPackage);
            Debugger.Log($"读档解析数据完成! sceneID：{dataPackage.sceneID}，saveTime：{dataPackage.saveTime}，{m_RoomInfos},{m_RoomBaseInfos},{m_BorderEntityDatas},{m_PostThingGraph},{m_GetEnvGraphData}");
            res = true;
        }
        catch (Exception e)
        {
            Debugger.LogError("读档解析失败，e：" + e);
        }
        return res;
    }

    private void ParseDataPackage(DataPackage dataPackage)
    {
        if (!string.IsNullOrEmpty(dataPackage.sceneID))
        {
            MainData.SceneID = dataPackage.sceneID;
        }
        foreach (var item in dataPackage.dataPackageInfo.TargetToList())
        {
            switch (item.key)
            {
                case "m_RoomInfosJson":
                    Serialization<RoomInfo> roomInfos = JsonUtility.FromJson<Serialization<RoomInfo>>(item.json);
                    m_RoomInfos = roomInfos.TargetToList();
                    break;
                case "m_RoomBaseInfosJson":
                    Serialization<RoomBaseInfo> roomBaseInfosJson = JsonUtility.FromJson<Serialization<RoomBaseInfo>>(item.json);
                    m_RoomBaseInfos = roomBaseInfosJson.TargetToList();
                    break;
                case "m_BorderEntityDatasJson":
                    Serialization<BorderEntityData> borderEntityData = JsonUtility.FromJson<Serialization<BorderEntityData>>(item.json);
                    m_BorderEntityDatas = borderEntityData.TargetToList();
                    break;
                case "m_PostThingGraphJson":
                    Serialization<PostThingGraph> postThingGraph = JsonUtility.FromJson<Serialization<PostThingGraph>>(item.json);
                    m_PostThingGraph = postThingGraph.Target();
                    break;
                case "m_GetEnvGraphDataJson":
                    Serialization<GetEnvGraph_data> getEnvGraph_data = JsonUtility.FromJson<Serialization<GetEnvGraph_data>>(item.json);
                    m_GetEnvGraphData = getEnvGraph_data.Target();
                    break;
                case "m_ListRoomMatData":
                    Serialization<RoomMatData> listRoomMatData = JsonUtility.FromJson<Serialization<RoomMatData>>(item.json);
                    m_ListRoomMatData = listRoomMatData.TargetToList();
                    break;
                default:
                    Debugger.Log("key dont , key ：" + item.key);
                    break;
            }
        }
    }


    //各个房间之间的邻接关系
    public List<RoomBaseInfo> ReadRoomBaseInfos()
    {
        return m_RoomBaseInfos;
    }

    public List<RoomInfo> ReadRoomInfos()
    {
        return m_RoomInfos;
    }

    public List<BorderEntityData> ReadListRoomBuilderInfo()
    {
        return m_BorderEntityDatas;
    }

    /// <summary>
    /// 所有实体放置的位置等信息
    /// </summary>
    public PostThingGraph ReadGetThingGraph_data_items()
    {
        return m_PostThingGraph;
    }

    /// <summary>
    /// 环境场景图,房间与房间的邻接关系
    /// </summary>
    /// <param name="getEnvGraph_Data"></param>
    public GetEnvGraph_data ReadGetEnvGraph_data()
    {
        return m_GetEnvGraphData;
    }

    /// <summary>
    /// 房间的地板、墙壁材质数据
    /// </summary>
    /// <returns></returns>
    public List<RoomMatData> ReadRoomMatData()
    {
        return m_ListRoomMatData;
    }

    [Serializable]
    /// <summary>
    /// 房间的地板、墙壁材质数据
    /// </summary>
    public class RoomMatData
    {
        public RoomType roomType;
        public string roomID;
        public int matIDFloor;
        public int matIDWall;
    }

}
