using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：热更新路径配置
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2023.05.26
    /// 版本：1.1
    /// </summary>
    public class HotUpdateSetting
    {
        /// <summary>
        /// 本地版本信息文件名
        /// </summary>
        public static string hotUpdateVersionFileName = "LocalVersion.json";
        /// <summary>
        /// 热更前本地版本信息文件根目录
        /// </summary>
        public static string localVersionRootPath = Application.streamingAssetsPath + "/Version";
        /// <summary>
        /// 热更后本地版本信息文件根目录
        /// </summary>
        public static string hotUpdatedLocalVersionRootPath = Application.persistentDataPath + "/Version";



        /// <summary>
        /// 导出热更包.bundle文件 默认文件夹目录
        /// </summary>
        public static string folderFullPath_HoutUpdateBundle = Application.dataPath + "/../" + "_HotUpdate";
        /// <summary>
        /// 写入/读取AB包MD5信息到本地项目 文件夹目录
        /// </summary>
        public static string folderFullPath_MD5 = Application.dataPath + "/../_MD5";
        /// <summary>
        /// AB包的MD5等信息 文件名称
        /// </summary>
        public static string fileName_ABMD5 = "ABMD5Data";
        /// <summary>
        /// 服务器热更列表MD5等信息 文件名称
        /// </summary>
        public static string fileName_HotUpdateConfig = "HotUpdateConfig";

        /// <summary>
        /// 获取引擎项目版本号
        /// </summary>
        public static string GetProjectVersion
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.PlayerSettings.bundleVersion;
#else
                return "x.x.x";
#endif
            }
        }
    }
}