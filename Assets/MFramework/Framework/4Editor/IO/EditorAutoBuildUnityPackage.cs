#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：自动导出UnityPackage包
    /// 功能：自动打包指定文件到固定目录
    /// 作者：毛俊峰
    /// 时间：2022.07
    /// 版本：1.0
    /// </summary>
    public class EditorAutoBuildUnityPackage
    {
        /// <summary>
        /// 导出unitypackage格式项目资源
        /// 导出位置固定  导出完成自动打开unitypackage所在文件夹
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="exportPathName"></param>
        public static void ExportUnityPactage(string packageName, string exportPathName)
        {
            AssetDatabase.ExportPackage(exportPathName, packageName, ExportPackageOptions.Recurse);
            //打开本地文件夹  目录不允许有中文
            Application.OpenURL("file://" + Application.dataPath + "../../");
        }
    }
} 
#endif