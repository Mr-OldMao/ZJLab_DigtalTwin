/// <summary>
/// 描述：消息系统枚举
/// 作者：毛俊峰
/// 时间：2022.08.22
/// 版本：1.0
/// </summary>
public class MsgEventEnum { }
/// <summary>
/// 消息系统枚举
/// </summary>
public enum MsgEventName
{
    Test,
    /// <summary>
    /// 所有的房间、物体生成完毕
    /// </summary>
    GenerateSceneComplete,

    /// <summary>
    /// 切换相机事件
    /// </summary>
    ChangeCamera,

    /// <summary>
    /// HTTP请求成功
    /// </summary>
    HttpRequestSucceed,
    /// <summary>
    /// HTTP请求失败
    /// </summary>
    HttpRequestFail,
    /// <summary>
    /// 开始移动
    /// </summary>
    RobotMoveBegin,
    /// <summary>
    /// 正在移动
    /// </summary>
    RobotMoveStay,
    /// <summary>
    /// 结束移动
    /// </summary>
    RobotMoveEnd,
    /// <summary>
    /// 机器人到达目标位置
    /// </summary>
    RobotArriveTargetPos,

    /// <summary>
    /// 门动画播放开始
    /// </summary>
    DoorAnimBegin,
    /// <summary>
    /// 门动画播放结束
    /// </summary>
    DoorAnimEnd,
}

