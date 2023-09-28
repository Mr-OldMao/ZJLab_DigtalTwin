using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GetThingGraph;

/// <summary>
/// 标题：核心数据缓存
/// 功能：
/// 作者：毛俊峰
/// 时间：2023.08.21
/// </summary>
public class MainData
{
    /// <summary>
    /// 配置文件数据
    /// </summary>
    public static ConfigData ConfigData { get; set; }

    /// <summary>
    /// 接口参数ID
    /// </summary>
    public const string ID = "test";

    /// <summary>
    /// 物体与房间的邻接关系数据
    /// </summary>
    public static GetThingGraph getThingGraph { get; set; }
    /// <summary>
    /// 房间与房间的邻接关系数据
    /// </summary>
    public static GetEnvGraph getEnvGraph { get; set; }

    /// <summary>
    /// 缓存整个场景 物品实体信息
    /// </summary>
    public static PostThingGraph CacheSceneItemsInfo { get; set; }
    /// <summary>
    /// 缓存摄像机视野范围内的 物品实体信息
    /// </summary>
    public static PostThingGraph CacheCameraItemsInfo { get; set; }
    /// <summary>
    /// 缓存场景中所有实体
    /// </summary>
    public static Dictionary<string,GameObject> CacheItemsEntity = new Dictionary<string,GameObject>(); 

    /// <summary>
    /// 指令控制数据
    /// </summary>
    public static Queue<ControlCommit> controlCommit { get; set; } = new Queue<ControlCommit>();



}
