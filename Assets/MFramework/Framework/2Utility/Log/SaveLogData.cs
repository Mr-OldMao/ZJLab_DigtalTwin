using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：存储日志数据
    /// 功能：对日志实时监听、日志文件的存储
    /// 作者：毛俊峰
    /// 时间：2022.10.19
    /// 版本：1.0
    /// </summary>
    public class SaveLogData : SingletonByMono<SaveLogData>
    {
        /// <summary>
        /// 存储日志文件根目录
        /// </summary>
        private static string logRootPath = Application.persistentDataPath + "/Log";
        /// <summary>
        /// 日志文件名
        /// </summary>
        private static string logFileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_Log.txt";
        /// <summary>
        /// 存储日志文件全路径
        /// </summary>
        private static string logFilePath = logRootPath + '/' + logFileName;
        /// <summary>
        /// 是否正在监听日志写入
        /// </summary>
        public static bool IsListeneringWriteLog
        {
            get
            {
                return m_IsListeneringWriteLog;
            }
        }

        private static bool m_IsListeneringWriteLog = false;
        private static FileStream FileWriter;
        private static UTF8Encoding encoding;

        /// <summary>
        /// 监听日志写入
        /// </summary>
        public void ListenerWriteLog()
        {
            if (m_IsListeneringWriteLog)
            {
                return;
            }
            m_IsListeneringWriteLog = !m_IsListeneringWriteLog;
            MFramework.Debugger.Log("开始监听日志信息写入 logFilePath：" + logFilePath);

            DirectoryConfig();
            if (DebuggerConfig.canWriteDeviceHardwareData)
            {
                PhoneSystemInfo(logFilePath);
            }
            FileInfo fileInfo = new FileInfo(logFilePath);
            //设置Log文件输出地址
            FileWriter = fileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            encoding = new UTF8Encoding();
            Debugger.logCallback += LogCallback;
        }

        /// <summary>
        /// 日志根路径配置
        /// </summary>
        private void DirectoryConfig()
        {
            if (!Directory.Exists(logRootPath))
            {
                Directory.CreateDirectory(logRootPath);
            }
            string[] logFilePathArr = Directory.GetFiles(logRootPath);
            if (logFilePathArr.Length >= DebuggerConfig.logFileMaxCount)
            {
                MFramework.Debugger.LogError("当前历史日志文件数量已超过限制(" + DebuggerConfig.logFileMaxCount + ")，即将删除超出的历史日志文件");
                //删除历史文件
                int delectCount = logFilePathArr.Length - (int)DebuggerConfig.logFileMaxCount + 1;
                for (int i = 0; i < delectCount; i++)
                {
                    File.Delete(logFilePathArr[i]);
                }
            }
        }

        /// <summary>
        /// 自定义Log回调
        /// </summary>
        /// <param name="index"></param>
        /// <param name="logType"></param>
        /// <param name="logTag"></param>
        /// <param name="msg"></param>
        private void LogCallback(int index, object msg, LogType logType, LogTag logTag, string stackData)
        {
            //清除日志信息中富文本标签
            msg = Regex.Replace(msg.ToString(), @"(<.*?>)", "");
            //输出的日志类型可以自定义
            string content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "【" + index + "】" + "【" + logTag + "】" + "【" + logType + "】"
                + "\n" + msg
                + "\n" + stackData + "\n";
            FileWriter.Position = FileWriter.Length;
            FileWriter.Write(encoding.GetBytes(content),0, encoding.GetBytes(content).Length);
            FileWriter.Flush();
        }

        private void OnDestroy() //关闭写入
        {
            if ((FileWriter != null))
            {
                Debugger.Log("结束监听日志写入 logFilePath：" + logFilePath);
                FileWriter.Close();
                Debugger.logCallback -= LogCallback;
            }
        }

        private static void PhoneSystemInfo(string filePath)
        {
            using StreamWriter sw = new StreamWriter(filePath);
            sw.WriteLine("*********************************************************************************************************start");
            sw.WriteLine("By " + SystemInfo.deviceName);
            DateTime now = DateTime.Now;
            sw.WriteLine(string.Concat(new object[] { now.Year.ToString(), "年", now.Month.ToString(), "月", now.Day, "日  ", now.Hour.ToString(), ":", now.Minute.ToString(), ":", now.Second.ToString() }));
            sw.WriteLine();
            sw.WriteLine("操作系统:  " + SystemInfo.operatingSystem);
            sw.WriteLine("系统内存大小:  " + SystemInfo.systemMemorySize);
            sw.WriteLine("设备模型:  " + SystemInfo.deviceModel);
            sw.WriteLine("设备唯一标识符:  " + SystemInfo.deviceUniqueIdentifier);
            sw.WriteLine("处理器数量:  " + SystemInfo.processorCount);
            sw.WriteLine("处理器类型:  " + SystemInfo.processorType);
            sw.WriteLine("显卡标识符:  " + SystemInfo.graphicsDeviceID);
            sw.WriteLine("显卡名称:  " + SystemInfo.graphicsDeviceName);
            sw.WriteLine("显卡标识符:  " + SystemInfo.graphicsDeviceVendorID);
            sw.WriteLine("显卡厂商:  " + SystemInfo.graphicsDeviceVendor);
            sw.WriteLine("显卡版本:  " + SystemInfo.graphicsDeviceVersion);
            sw.WriteLine("显存大小:  " + SystemInfo.graphicsMemorySize);
            sw.WriteLine("显卡着色器级别:  " + SystemInfo.graphicsShaderLevel);
            sw.WriteLine("是否支持内置阴影:  " + SystemInfo.supportsShadows);
            sw.WriteLine("*********************************************************************************************************end");
            sw.WriteLine();

            sw.Close();
            sw.Dispose();
        }
    }
}