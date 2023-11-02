using System.Collections.Generic;

namespace MFramework
{
    /// <summary>
    /// 标题：日志系统配置
    /// 功能：对指定标签打印，修改控制台日志显示样式，缓存到日志信息到本地
    /// 作者：毛俊峰
    /// 时间：2022.10.19   2023.10.27
    /// 版本：1.0
    /// </summary>
    public class DebuggerConfig
    {
        private static bool m_CanPrintConsoleLog = UnityEngine.PlayerPrefs.GetInt("CanPrintConsoleLog", 1) == 1;
        private static bool m_CanPrintConsoleLogError = UnityEngine.PlayerPrefs.GetInt("CanPrintConsoleLogError", 1) == 1;
        private static bool m_CanSaveLogDataFile = UnityEngine.PlayerPrefs.GetInt("m_CanSaveLogDataFile", 1) == 1;
        /// <summary>
        /// 是否允许控制台打印 所有非错误日志
        /// </summary>
        public static bool CanPrintConsoleLog
        {
            get
            {
                return m_CanPrintConsoleLog;
            }
            set
            {
                UnityEngine.PlayerPrefs.SetInt("CanPrintConsoleLog", value ? 1 : 0);
                m_CanPrintConsoleLog = value;
            }
        }

        /// <summary>
        /// 允许控制台打印 错误日志
        /// </summary>
        public static bool CanPrintConsoleLogError
        {
            get
            {
                return m_CanPrintConsoleLogError;
            }
            set
            {
                UnityEngine.PlayerPrefs.SetInt("CanPrintConsoleLogError", value ? 1 : 0);
                m_CanPrintConsoleLogError = value;
            }
        }


        /// <summary>
        /// 允许在控制台显示打印的日志标签
        /// </summary>
        public static List<LogTag> canPrintLogTagList = new List<LogTag>
        {
            LogTag.Temp,
            //LogTag.Test,
            LogTag.Forever
        };

        /// <summary>
        /// 运行改变控制台打印样式
        /// </summary>
        public static bool canChangeConsolePrintStyle = true;


        #region 缓存日志文件到本地
        /// <summary>
        /// 允许日志文件到缓存本地
        /// </summary>
        public static bool CanSaveLogDataFile
        {
            get
            {
                return m_CanSaveLogDataFile;
            }
            set
            {
                UnityEngine.PlayerPrefs.SetInt("CanSaveLogDataFile", value ? 1 : 0);
                m_CanSaveLogDataFile = value;
            }
        }
        /// <summary>
        /// 允许写入硬件数据信息
        /// </summary>
        public const bool canWriteDeviceHardwareData = true;
        /// <summary>
        /// 缓存历史日志文件最大数量
        /// </summary>
        public const uint logFileMaxCount = 10;
        #endregion
    }
}