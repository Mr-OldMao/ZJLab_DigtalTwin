using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 标题：核心数据缓存
/// 功能：
/// 作者：毛俊峰
/// 时间：2023.08.21
/// </summary>
public class MainData
{
    /// <summary>
    /// 物体与房间的邻接关系数据
    /// </summary>
    public static GetThingGraph getThingGraph { get; set; }
    /// <summary>
    /// 房间与房间的邻接关系数据
    /// </summary>
    public static GetEnvGraph getEnvGraph { get; set; }

    /// <summary>
    /// 指令控制数据
    /// </summary>
    public static List<ControlCommit> controlCommit { get; set; } = new List<ControlCommit>();

}
