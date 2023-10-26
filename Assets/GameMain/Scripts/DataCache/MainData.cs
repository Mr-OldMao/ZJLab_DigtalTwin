using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    /// 当前是否使用测试数据
    /// </summary>
    public static bool UseTestData { get; private set; } = true;
    /// <summary>
    /// 配置文件数据
    /// </summary>
    public static ConfigData ConfigData { get; set; }

    /// <summary>
    /// 场景ID 接口参数ID
    /// </summary>
    public static string IDScene = string.Empty;

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
    public static Dictionary<string, GameObject> CacheItemsEntity = new Dictionary<string, GameObject>();

    /// <summary>
    /// 指令控制数据
    /// </summary>
    public static Queue<ControlCommit> controlCommit { get; set; } = new Queue<ControlCommit>();



    /// <summary>
    /// 机器人坐标信息
    /// </summary>
    public static Queue<Feature_Robot_Pos> feature_robot_pos { get; set; } = new Queue<Feature_Robot_Pos> { };
    /// <summary>
    /// 访客坐标信息
    /// </summary>
    public static Queue<Feature_People_Perception> feature_People_Perceptions { get; set; } = new Queue<Feature_People_Perception> { };
}
