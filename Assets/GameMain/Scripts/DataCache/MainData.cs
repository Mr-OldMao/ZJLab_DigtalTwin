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
    /// 缓存生成的物品实体信息，发松到服务端
    /// </summary>
    public static PostThingGraph CacheItemsInfo { get; set; }
    /// <summary>
    /// 指令控制数据
    /// </summary>
    public static List<ControlCommit> controlCommit { get; set; } = new List<ControlCommit>();



}
