using System;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：日志系统
    /// 功能：对指定标签打印，控制台日志的显示样式
    /// 作者：毛俊峰
    /// 时间：2022.10.19
    /// 版本：1.0
    /// </summary>
    public class Debugger : MonoBehaviour
    {
        /// <summary>
        /// 日志回调(1日志index 2日志内容 3日志类型 4日志标签 5日志堆栈信息)
        /// </summary>
        public static Action<int, object, LogType, LogTag, string> logCallback;
        private static int m_CurLogIndex = 1;


        #region 对外接口
        public static void Log(object message, LogTag logTag = LogTag.Temp)
        {
            LogHandle(message, logTag, LogType.Log);
        }

        public static void LogWarning(object message, LogTag logTag = LogTag.Temp)
        {
            LogHandle(message, logTag, LogType.Warning);
        }

        public static void LogError(object message, LogTag logTag = LogTag.Temp)
        {
            LogHandle(message, logTag, LogType.Error);
        }

        #endregion

        /// <summary>
        /// 日志处理
        /// </summary>
        /// <param name="logMsg"></param>
        /// <param name="logTag"></param>
        /// <param name="logType"></param>
        private static void LogHandle(object logMsg, LogTag logTag, LogType logType)
        {
            if (Debug.unityLogger.logEnabled)
            {
                if (DebuggerConfig.canPrintLogTagList == null || DebuggerConfig.canPrintLogTagList.Count == 0)
                {
                    Debug.unityLogger.logEnabled = false;
                    return;
                }
                if (!DebuggerConfig.canPrintLogTagList.Contains(logTag))
                {
                    return;
                }
                if (DebuggerConfig.canSaveLogDataFile && !SaveLogData.IsListeneringWriteLog)
                {
                    SaveLogData.GetInstance.ListenerWriteLog();
                }
                if (DebuggerConfig.canChangeConsolePrintStyle)
                {
                    ChangeStyle(ref logMsg, logTag, logType);
                }
                Debug.unityLogger.Log(logType, logMsg);
                logCallback?.Invoke(m_CurLogIndex++, logMsg, logType, logTag, StackTraceUtility.ExtractStackTrace());
            }
        }

        /// <summary>
        /// 控制台打印样式调整
        /// </summary>
        /// <param name="logMsg"></param>
        /// <param name="logTag"></param>
        private static void ChangeStyle(ref object logMsg, LogTag logTag, LogType logType)
        {
            switch (logType)
            {
                case LogType.Error:
                    switch (logTag)
                    {
                        case LogTag.Temp:
                            logMsg = "<color=#FF5656>" + logMsg + "</color>";
                            break;
                        case LogTag.Test:
                            logMsg = "<B><color=#FF5656>" + logMsg + "</color></B>";
                            break;
                        case LogTag.Forever:
                            logMsg = "<B><color=red>" + logMsg + "</color></B>";
                            break;
                    }
                    break;
                case LogType.Warning:
                    switch (logTag)
                    {
                        case LogTag.Temp:
                            logMsg = "<color=#FFF556>" + logMsg + "</color>";
                            break;
                        case LogTag.Test:
                            logMsg = "<B><color=#FFF556>" + logMsg + "</color></B>";
                            break;
                        case LogTag.Forever:
                            logMsg = "<B><color=yellow>" + logMsg + "</color></B>";
                            break;
                    }
                    break;
                case LogType.Log:
                    switch (logTag)
                    {
                        case LogTag.Temp:
                            logMsg = "<color=#AFFFFF>" + logMsg + "</color>";
                            break;
                        case LogTag.Test:
                            logMsg = "<B><color=#97FFFF>" + logMsg + "</color></B>";
                            break;
                        case LogTag.Forever:
                            logMsg = "<B><color=#00FFFF>" + logMsg + "</color></B>";
                            break;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// 日志标签
    /// </summary>
    public enum LogTag
    {
        /// <summary>
        /// 临时日志
        /// </summary>
        Temp,
        /// <summary>
        /// 关键节点测试调试日志
        /// </summary>
        Test,
        /// <summary>
        /// 常驻日志
        /// </summary>
        Forever
    }
}