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
    /// HTTP请求成功
    /// </summary>
    HttpRequestSucceed,
    /// <summary>
    /// HTTP请求失败
    /// </summary>
    HttpRequestFail,

    AsyncLoadedComplete,

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
}

