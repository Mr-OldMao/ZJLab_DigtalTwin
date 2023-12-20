using System;
/// <summary>
/// 标题：web端新增物体后回调数据结构
/// 功能：
/// 作者：毛俊峰
/// 时间：2023.12.14
/// </summary>
[Serializable]
public class JsonWebGlobalEntityData
{
    public string sceneID = MainData.SceneID;
    public string tmpId = MainData.tmpID;
    public string entityID;
    public string entityType;
    /// <summary>
    /// 新增、删除物体结果 0-失败 1-成功
    /// </summary>
    public int result;
    /// <summary>
    /// 新增、删除完毕后的当前场景所有物品信息
    /// </summary>
    public PostThingGraph postThingGraph;
}
