using System;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：消息系统
    /// 功能：基于事件的消息系统，
    /// 目的：降低不同模块之间的相互调用的耦合性
    /// 具体功能：消息注册(四种)：1.不带参、不带返回值 2.带参、不带返回值 3.不带参、带返回值 4.带参、带返回值
    ///           消息发送(四种)：1.不带参、不带返回值 2.带参、不带返回值 3.不带参、带返回值 4.带参、带返回值
    ///           消息注销(九种)：1.注销指定类型指定消息 2.注销指定类型所有消息 3.注销所有类型所有消息
    /// 作者：毛俊峰
    /// 时间：2022.07.16，2022.08.22
    /// 版本：1.0，1.1
    /// </summary>
    public class MsgEvent
    {
        /// <summary>
        /// 消息事件缓存容器（不带参、不带返回值）  key-消息名  value-消息列表
        /// </summary>
        private static Dictionary<string, List<MsgEventInfo>> m_DicMsgEventContainer = new Dictionary<string, List<MsgEventInfo>>();
        /// <summary>
        /// 消息事件缓存容器（带参、不带返回值）  key-消息名  value-消息列表
        /// </summary>
        private static Dictionary<string, List<MsgEventParamInfo>> m_DicMsgEventParamContainer = new Dictionary<string, List<MsgEventParamInfo>>();
        /// <summary>
        /// 消息事件缓存容器（不带参、带返回值）  key-消息名  value-消息列表
        /// </summary>
        private static Dictionary<string, List<MsgEventReturnInfo>> m_DicMsgEventReturnContainer = new Dictionary<string, List<MsgEventReturnInfo>>();
        /// <summary>
        /// 消息事件缓存容器（带参、带返回值）  key-消息名  value-消息列表
        /// </summary>
        private static Dictionary<string, List<MsgEventParamReturnInfo>> m_DicMsgEventParamReturnContainer = new Dictionary<string, List<MsgEventParamReturnInfo>>();
        /// <summary>
        /// 消息事件数据结构基类
        /// </summary>
        public class MsgEventBase
        {
            /// <summary>
            /// 当前消息事件描述
            /// </summary>
            public string msgEventDescript;
            /// <summary>
            /// 当前消息事件执行次数
            /// </summary>
            public int msgEventActiveCount;
        }
        /// <summary>
        /// 不带参、不带返回值消息事件数据结构
        /// </summary>
        public class MsgEventInfo : MsgEventBase
        {
            public Action msgEvent;
        }
        /// <summary>
        /// 带参、不带返回值消息事件数据结构
        /// </summary>
        public class MsgEventParamInfo : MsgEventBase
        {
            public Action<object> msgEvent;
        }
        /// <summary>
        /// 不带参、带返回值消息事件数据结构
        /// </summary>
        public class MsgEventReturnInfo : MsgEventBase
        {
            public Func<object> msgEvent;
        }
        /// <summary>
        /// 带参、带返回值消息事件数据结构
        /// </summary>
        public class MsgEventParamReturnInfo : MsgEventBase
        {
            public Func<object, object> msgEvent;
        }



        #region 对外接口
        #region 消息调用
        /// <summary>
        /// 发送不带参、不带返回值消息
        /// </summary>
        /// <param name="msgName">消息名</param>
        public static void SendMsg(MsgEventName msgName)
        {
            SendMsg(msgName.ToString());
        }
        /// <summary>
        /// 发送带参、不带返回值消息
        /// </summary>
        /// <param name="msgName">消息名</param>
        /// <param name="param"></param>
        public static void SendMsg(MsgEventName msgName, object param)
        {
            SendMsg(msgName.ToString(), param);
        }
        /// <summary>
        /// 发送不带参、带返回值消息
        /// </summary>
        /// <param name="msgName">消息名</param>
        /// <param name="callback"></param>
        public static void SendMsg(MsgEventName msgName, Action<object> callback)
        {
            SendMsg(msgName.ToString(), callback);
        }
        /// <summary>
        /// 发送带参、带返回值消息
        /// </summary>
        /// <param name="msgName">消息名</param>
        /// <param name="callback"></param>
        public static void SendMsg(MsgEventName msgName, object param, Action<object> callback)
        {
            SendMsg(msgName.ToString(), param, callback);
        }
        #endregion

        #region 消息注册
        /// <summary>
        /// 注册消息 不带参、不带返回值
        /// </summary>
        /// <param name="msgName">消息名枚举</param>
        /// <param name="msgEvent">消息事件</param>
        /// <param name="msgEventDescript">消息事件描述</param>
        public static void RegisterMsgEvent(MsgEventName msgName, Action msgEvent, string msgEventDescript = "")
        {
            RegisterMsgEvent(msgName.ToString(), msgEvent, msgEventDescript);
        }
        /// <summary>
        /// 注册消息 带参、不带返回值
        /// </summary>
        /// <param name="msgName">消息名枚举</param>
        /// <param name="msgEvent">消息事件</param>
        /// <param name="msgEventDescript">消息事件描述</param>
        public static void RegisterMsgEvent(MsgEventName msgName, Action<object> msgEvent, string msgEventDescript = "")
        {
            RegisterMsgEvent(msgName.ToString(), msgEvent, msgEventDescript);
        }
        /// <summary>
        /// 注册消息 不带参、带返回值
        /// </summary>
        /// <param name="msgName">消息名枚举</param>
        /// <param name="msgEvent">消息事件</param>
        /// <param name="msgEventDescript">消息事件描述</param>
        public static void RegisterMsgEvent(MsgEventName msgName, Func<object> msgEvent, string msgEventDescript = "")
        {
            RegisterMsgEvent(msgName.ToString(), msgEvent, msgEventDescript);
        }
        /// <summary>
        /// 注册消息 带参、带返回值
        /// </summary>
        /// <param name="msgName">消息名枚举</param>
        /// <param name="msgEvent">消息事件</param>
        /// <param name="msgEventDescript">消息事件描述</param>
        public static void RegisterMsgEvent(MsgEventName msgName, Func<object, object> msgEvent, string msgEventDescript = "")
        {
            RegisterMsgEvent(msgName.ToString(), msgEvent, msgEventDescript);
        }
        #endregion

        #region 消息注销
        /// <summary>
        /// 注销消息  注销消息名下的所有类型的所有消息（可用于注销使用Lamda和非Lamdba表达式注册过的事件消息）
        /// <param name="msgName">消息名</param>
        public static void UnregisterMsgEvent(MsgEventName msgName)
        {
            UnregisterMsgEventAll(msgName.ToString());
        }
        /// <summary>
        /// 注销消息  注销消息名下的指定类型的所有消息(可用于注销使用Lamda和非Lamdba表达式注册过的事件消息)
        /// </summary>
        /// <param name="msgName">消息名</param>
        /// <param name="msgEventEnum">消息类型</param>
        public static void UnregisterMsgEvent(MsgEventName msgName, MsgEventType msgEventEnum)
        {
            switch (msgEventEnum)
            {
                case MsgEventType.NoParamNoReturn:
                    UnregisterMsgEvent(msgName.ToString(), null);
                    break;
                case MsgEventType.HasParamNoReturn:
                    UnregisterMsgEventParam(msgName.ToString(), null);
                    break;
                case MsgEventType.NoParamHasReturn:
                    UnregisterMsgEventReturn(msgName.ToString(), null);
                    break;
                case MsgEventType.HasParamHasReturn:
                    UnregisterMsgEventParamReturn(msgName.ToString(), null);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 注销消息  注销消息名下指定的不带参、不带返回值消息(仅用于注销使用非Lamda表达式注册过的事件消息)
        /// </summary>
        /// <param name="msgName">消息名</param>
        /// <param name="msgEvent">消息事件</param>
        public static void UnregisterMsgEvent(MsgEventName msgName, Action msgEvent)
        {
            UnregisterMsgEvent(msgName.ToString(), msgEvent);
        }
        /// <summary>
        /// 注销消息  注销消息名下指定的带参、不带返回值消息(仅用于注销使用非Lamda表达式注册过的事件消息)
        /// </summary>
        /// <param name="msgName">消息名</param>
        /// <param name="msgEvent">消息事件</param>
        public static void UnregisterMsgEvent(MsgEventName msgName, Action<object> msgEvent)
        {
            UnregisterMsgEventParam(msgName.ToString(), msgEvent);
        }
        /// <summary>
        /// 注销消息  注销消息名下指定的不带参、带返回值消息(仅用于注销使用非Lamda表达式注册过的事件消息)
        /// </summary>
        /// <param name="msgName">消息名</param>
        /// <param name="msgEvent">消息事件</param>
        public static void UnregisterMsgEvent(MsgEventName msgName, Func<object> msgEvent)
        {
            UnregisterMsgEventReturn(msgName.ToString(), msgEvent);
        }
        /// <summary>
        /// 注销消息  注销消息名下指定的带参、带返回值消息(仅用于注销使用非Lamda表达式注册过的事件消息)
        /// </summary>
        /// <param name="msgName">消息名</param>
        /// <param name="msgEvent">消息事件</param>
        public static void UnregisterMsgEvent(MsgEventName msgName, Func<object, object> msgEvent)
        {
            UnregisterMsgEventParamReturn(msgName.ToString(), msgEvent);
        }
        #endregion
        #endregion


        #region 内部具体实现
        #region 消息注册
        /// <summary>
        /// 注册消息 不带参、不带返回值
        /// </summary>
        /// <param name="msgName">消息名</param>
        /// <param name="msgEvent">消息事件</param>
        /// <param name="msgEventDescript">消息事件描述</param>
        private static void RegisterMsgEvent(string msgName, Action msgEvent, string msgEventDescript = "")
        {
            if (m_DicMsgEventContainer.ContainsKey(msgName))
            {
                List<MsgEventInfo> msgEventInfoList = m_DicMsgEventContainer[msgName];
                MsgEventInfo msgEventParamInfo = new MsgEventInfo
                {
                    msgEvent = msgEvent,
                    msgEventActiveCount = 0,
                    msgEventDescript = msgEventDescript
                };
                msgEventInfoList.Add(msgEventParamInfo);
            }
            else
            {
                List<MsgEventInfo> msgEventInfoList = new List<MsgEventInfo>();
                MsgEventInfo msgEventParamStruct = new MsgEventInfo
                {
                    msgEvent = msgEvent,
                    msgEventDescript = msgEventDescript,
                    msgEventActiveCount = 0
                };
                msgEventInfoList.Add(msgEventParamStruct);
                m_DicMsgEventContainer.Add(msgName, msgEventInfoList);
            }
        }

        /// <summary>
        /// 注册消息 带参、不带返回值
        /// </summary>
        /// <param name="msgName">消息名</param>
        /// <param name="msgEvent">消息事件</param>
        /// <param name="msgEventDescript">消息事件描述</param>
        private static void RegisterMsgEvent(string msgName, Action<object> msgEvent, string msgEventDescript = "")
        {
            if (m_DicMsgEventParamContainer.ContainsKey(msgName))
            {
                List<MsgEventParamInfo> msgEventParamInfoList = m_DicMsgEventParamContainer[msgName];
                MsgEventParamInfo msgEventParamInfo = new MsgEventParamInfo
                {
                    msgEvent = msgEvent,
                    msgEventActiveCount = 0,
                    msgEventDescript = msgEventDescript
                };
                msgEventParamInfoList.Add(msgEventParamInfo);
            }
            else
            {
                List<MsgEventParamInfo> msgEventParamInfoList = new List<MsgEventParamInfo>();
                MsgEventParamInfo msgEventParamStruct = new MsgEventParamInfo
                {
                    msgEvent = msgEvent,
                    msgEventDescript = msgEventDescript,
                    msgEventActiveCount = 0
                };
                msgEventParamInfoList.Add(msgEventParamStruct);
                m_DicMsgEventParamContainer.Add(msgName, msgEventParamInfoList);
            }
        }

        /// <summary>
        /// 注册消息 不带参、带返回值
        /// </summary>
        /// <param name="msgName">消息名</param>
        /// <param name="msgEvent">消息事件</param>
        /// <param name="msgEventDescript">消息事件描述</param>
        private static void RegisterMsgEvent(string msgName, Func<object> msgEvent, string msgEventDescript = "")
        {
            if (m_DicMsgEventReturnContainer.ContainsKey(msgName))
            {
                List<MsgEventReturnInfo> msgEventInfoList = m_DicMsgEventReturnContainer[msgName];
                MsgEventReturnInfo msgEventParamInfo = new MsgEventReturnInfo
                {
                    msgEvent = msgEvent,
                    msgEventActiveCount = 0,
                    msgEventDescript = msgEventDescript
                };
                msgEventInfoList.Add(msgEventParamInfo);
            }
            else
            {
                List<MsgEventReturnInfo> msgEventInfoList = new List<MsgEventReturnInfo>();
                MsgEventReturnInfo msgEventParamStruct = new MsgEventReturnInfo
                {
                    msgEvent = msgEvent,
                    msgEventDescript = msgEventDescript,
                    msgEventActiveCount = 0
                };
                msgEventInfoList.Add(msgEventParamStruct);
                m_DicMsgEventReturnContainer.Add(msgName, msgEventInfoList);
            }
        }

        /// <summary>
        /// 注册消息 带参、带返回值
        /// </summary>
        /// <param name="msgName">消息名</param>
        /// <param name="msgEvent">消息事件</param>
        /// <param name="msgEventDescript">消息事件描述</param>
        private static void RegisterMsgEvent(string msgName, Func<object, object> msgEvent, string msgEventDescript = "")
        {
            if (m_DicMsgEventParamReturnContainer.ContainsKey(msgName))
            {
                List<MsgEventParamReturnInfo> msgEventInfoList = m_DicMsgEventParamReturnContainer[msgName];
                MsgEventParamReturnInfo msgEventParamInfo = new MsgEventParamReturnInfo
                {
                    msgEvent = msgEvent,
                    msgEventActiveCount = 0,
                    msgEventDescript = msgEventDescript
                };
                msgEventInfoList.Add(msgEventParamInfo);
            }
            else
            {
                List<MsgEventParamReturnInfo> msgEventInfoList = new List<MsgEventParamReturnInfo>();
                MsgEventParamReturnInfo msgEventParamStruct = new MsgEventParamReturnInfo
                {
                    msgEvent = msgEvent,
                    msgEventDescript = msgEventDescript,
                    msgEventActiveCount = 0
                };
                msgEventInfoList.Add(msgEventParamStruct);
                m_DicMsgEventParamReturnContainer.Add(msgName, msgEventInfoList);
            }
        }
        #endregion

        #region 消息注销
        /// <summary>
        /// 注销消息  注销消息名下的  所有消息（不带参、不带返回值消息，带参、带返回值消息，不带参、带返回值消息，带参、不带返回值消息）
        /// 一般用于注销 注册过lamda表达式的事件消息
        /// </summary>
        /// <param name="msgName"></param>
        private static void UnregisterMsgEventAll(string msgName)
        {
            bool isExist = false;
            if (m_DicMsgEventContainer.ContainsKey(msgName))
            {
                m_DicMsgEventContainer.Remove(msgName);
                isExist = true;
            }
            if (m_DicMsgEventParamContainer.ContainsKey(msgName))
            {
                m_DicMsgEventParamContainer.Remove(msgName);
                isExist = true;
            }
            if (m_DicMsgEventReturnContainer.ContainsKey(msgName))
            {
                m_DicMsgEventReturnContainer.Remove(msgName);
                isExist = true;
            }
            if (m_DicMsgEventParamReturnContainer.ContainsKey(msgName))
            {
                m_DicMsgEventParamReturnContainer.Remove(msgName);
                isExist = true;
            }
            if (!isExist)
            {
                Debug.LogError("消息注销失败 请检查消息名下是否有消息 msgName：" + msgName);
            }
        }
        /// <summary>
        /// 注销消息  注销消息名下 指定不带参、不带返回值消息
        /// 一般用于注销 注册过非lamda表达式的事件消息
        /// </summary>
        /// <param name="msgName"></param>
        private static void UnregisterMsgEvent(string msgName, Action msgEvent)
        {
            if (m_DicMsgEventContainer.ContainsKey(msgName))
            {
                if (msgEvent != null)
                {
                    List<MsgEventInfo> msgEventInfos = m_DicMsgEventContainer[msgName];
                    MsgEventInfo targetMsg = null;
                    for (int i = 0; i < msgEventInfos.Count; i++)
                    {
                        if (msgEventInfos[i].msgEvent == msgEvent)
                        {
                            targetMsg = msgEventInfos[i];
                            break;
                        }
                    }
                    if (targetMsg != null)
                    {
                        msgEventInfos.Remove(targetMsg);
                        Debug.Log("消息注销成功");
                    }
                    else
                    {
                        Debug.LogError("消息注销失败 未查询到该消息Action事件 msgName:" + msgName);
                    }
                }
                else
                {
                    m_DicMsgEventContainer.Remove(msgName);
                }
            }
            else
            {
                Debug.LogError("消息注销失败 请检查消息名下是否有，不带参、不带返回值消息 msgName：" + msgName);
            }
        }

        /// <summary>
        /// 注销消息  注销消息名下 指定带参、不带返回值消息
        /// 一般用于注销 注册过非lamda表达式的事件消息
        /// </summary>
        /// <param name="msgName"></param>
        private static void UnregisterMsgEventParam(string msgName, Action<object> msgEvent)
        {
            if (m_DicMsgEventParamContainer.ContainsKey(msgName))
            {
                if (msgEvent != null)
                {
                    List<MsgEventParamInfo> msgEventParamInfos = m_DicMsgEventParamContainer[msgName];
                    MsgEventParamInfo targetMsg = null;
                    for (int i = 0; i < msgEventParamInfos.Count; i++)
                    {
                        if (msgEventParamInfos[i].msgEvent == msgEvent)
                        {
                            targetMsg = msgEventParamInfos[i];
                            break;
                        }
                    }
                    if (targetMsg != null)
                    {
                        msgEventParamInfos.Remove(targetMsg);
                        Debug.Log("消息注销成功");
                    }
                    else
                    {
                        Debug.LogError("消息注销失败 未查询到该消息Action<object>事件 msgName:" + msgName);
                    }
                }
                else
                {
                    m_DicMsgEventParamContainer.Remove(msgName);
                }
            }
            else
            {
                Debug.LogError("消息注销失败 请检查消息名下是否有，带参、不带返回值 msgName：" + msgName);
            }
        }

        /// <summary>
        /// 注销消息  注销消息名下 指定不带参、带返回值消息
        /// 一般用于注销 注册过非lamda表达式的事件消息
        /// </summary>
        /// <param name="msgName"></param>
        private static void UnregisterMsgEventReturn(string msgName, Func<object> msgEvent)
        {
            if (m_DicMsgEventReturnContainer.ContainsKey(msgName))
            {
                if (msgEvent != null)
                {
                    List<MsgEventReturnInfo> msgEventInfos = m_DicMsgEventReturnContainer[msgName];
                    MsgEventReturnInfo targetMsg = null;
                    for (int i = 0; i < msgEventInfos.Count; i++)
                    {
                        if (msgEventInfos[i].msgEvent == msgEvent)
                        {
                            targetMsg = msgEventInfos[i];
                            break;
                        }
                    }
                    if (targetMsg != null)
                    {
                        msgEventInfos.Remove(targetMsg);
                        Debug.Log("消息注销成功");
                    }
                    else
                    {
                        Debug.LogError("消息注销失败 未查询到该消息Func<object>事件 msgName:" + msgName);
                    }
                }
                else
                {
                    m_DicMsgEventReturnContainer.Remove(msgName);
                }
            }
            else
            {
                Debug.LogError("消息注销失败 请检查消息名下是否有，带参、不带返回值 msgName：" + msgName);
            }
        }

        /// <summary>
        /// 注销消息  注销消息名下 指定不带参、带返回值消息
        /// 一般用于注销 注册过非lamda表达式的事件消息
        /// </summary>
        /// <param name="msgName"></param>
        private static void UnregisterMsgEventParamReturn(string msgName, Func<object, object> msgEvent)
        {
            if (m_DicMsgEventParamReturnContainer.ContainsKey(msgName))
            {
                if (msgEvent != null)
                {
                    List<MsgEventParamReturnInfo> msgEventInfos = m_DicMsgEventParamReturnContainer[msgName];
                    MsgEventParamReturnInfo targetMsg = null;
                    for (int i = 0; i < msgEventInfos.Count; i++)
                    {
                        if (msgEventInfos[i].msgEvent == msgEvent)
                        {
                            targetMsg = msgEventInfos[i];
                            break;
                        }
                    }
                    if (targetMsg != null)
                    {
                        msgEventInfos.Remove(targetMsg);
                        Debug.Log("消息注销成功");
                    }
                    else
                    {
                        Debug.LogError("消息注销失败 未查询到该消息Func<object,object>事件 msgName:" + msgName);
                    }
                }
                else
                {
                    m_DicMsgEventParamReturnContainer.Remove(msgName);
                }
            }
            else
            {
                Debug.LogError("消息注销失败 请检查消息名 msgName：" + msgName);
            }
        }
        #endregion

        #region 消息调用
        /// <summary>
        /// 发送不带参、不带返回值消息
        /// </summary>
        /// <param name="msgName"></param>
        /// <param name="param">消息事件参数</param>
        private static void SendMsg(string msgName)
        {
            if (m_DicMsgEventContainer.ContainsKey(msgName))
            {
                List<MsgEventInfo> msgEventInfos = m_DicMsgEventContainer[msgName];
                foreach (MsgEventInfo item in msgEventInfos)
                {
                    item.msgEvent.Invoke();
                }
            }
            else
            {
                Debug.LogError("消息系统-发送消息失败 检查消息名称 msgName：" + msgName);
            }
        }
        /// <summary>
        /// 发送带参、不带返回值消息
        /// </summary>
        /// <param name="msgName"></param>
        /// <param name="param">消息事件参数</param>
        private static void SendMsg(string msgName, object param)
        {
            if (m_DicMsgEventParamContainer.ContainsKey(msgName))
            {
                List<MsgEventParamInfo> msgEventParamInfos = m_DicMsgEventParamContainer[msgName];
                foreach (MsgEventParamInfo item in msgEventParamInfos)
                {
                    item.msgEvent.Invoke(param);
                }
            }
            else
            {
                Debug.LogError("消息系统-发送消息失败 检查消息名称 msgName：" + msgName);
            }
        }
        /// <summary>
        /// 发送不带参、带返回值消息
        /// </summary>
        /// <param name="msgName"></param>
        /// <param name="param">消息事件参数</param>
        private static void SendMsg(string msgName, Action<object> callback)
        {
            if (m_DicMsgEventReturnContainer.ContainsKey(msgName))
            {
                List<MsgEventReturnInfo> msgEventInfos = m_DicMsgEventReturnContainer[msgName];
                foreach (MsgEventReturnInfo item in msgEventInfos)
                {
                    object p = item.msgEvent.Invoke();
                    callback?.Invoke(p);
                }
            }
            else
            {
                Debug.LogError("消息系统-发送消息失败 检查消息名称 msgName：" + msgName);
            }
        }
        /// <summary>
        /// 发送带参、带返回值消息
        /// </summary>
        /// <param name="msgName"></param>
        /// <param name="param">消息事件参数</param>
        private static void SendMsg(string msgName, object param, Action<object> callback)
        {
            if (m_DicMsgEventParamReturnContainer.ContainsKey(msgName))
            {
                List<MsgEventParamReturnInfo> msgEventInfos = m_DicMsgEventParamReturnContainer[msgName];
                foreach (MsgEventParamReturnInfo item in msgEventInfos)
                {
                    object p = item.msgEvent.Invoke(param);
                    callback?.Invoke(p);
                }
            }
            else
            {
                Debug.LogError("消息系统-发送消息失败 检查消息名称 msgName：" + msgName);
            }
        }
        #endregion 
        #endregion
    }

    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MsgEventType
    {
        /// <summary>
        /// 不带参、不带返回值
        /// </summary>
        NoParamNoReturn,
        /// <summary>
        /// 带参、不带返回值
        /// </summary>
        HasParamNoReturn,
        /// <summary>
        /// 不带参、带返回值
        /// </summary>
        NoParamHasReturn,
        /// <summary>
        /// 带参、带返回值
        /// </summary>
        HasParamHasReturn
    }
}