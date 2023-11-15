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
    public static bool UseTestData { get; set; } = true;

    /// <summary>
    /// 是否为首次启动程序创建场景
    /// </summary>
    public static bool IsFirstGenerate { get; set; } = true;

    /// <summary>
    /// 配置文件数据
    /// </summary>
    public static ConfigData ConfigData { get; set; }

    /// <summary>
    /// 是否正在读档
    /// 是否读档生成场景实例，T-读档，获取历史场景实例数据，根据SceneID获取历史房间布局数据以及物体位置数据    F-生成随机新场景实例
    /// </summary>
    public static bool ReadingFile { get; set; }

    /// <summary>
    /// 是否允许读档
    /// </summary>
    public static bool CanReadFile { get; set; } = true;
    /// <summary>
    /// 场景ID 接口参数ID
    /// </summary>
    public static string SceneID = string.Empty;

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
    /// 需要执行的指令控制数据队列
    /// </summary>
    public static Queue<ControlCommit> ControlCommit { get; set; } = new Queue<ControlCommit>();
    /// <summary>
    /// 已经完成的指令队列
    /// </summary>
    public static List<ControlResult> ControlCommitCompletedList { get; set; } = new List<ControlResult>();




    #region 数字孪生

    /// <summary>
    /// 机器人坐标信息
    /// </summary>
    public static Queue<Feature_Robot_Pos> feature_robot_pos { get; set; } = new Queue<Feature_Robot_Pos> { };
    /// <summary>
    /// 访客坐标信息
    /// </summary>
    public static Queue<Feature_People_Perception> feature_People_Perceptions { get; set; } = new Queue<Feature_People_Perception> { }; 
    #endregion
}
