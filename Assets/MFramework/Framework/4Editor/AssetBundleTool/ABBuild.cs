using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：一键打包AssetBundle
    /// 功能：编辑器工具类 一键打包生成AB
    /// 作者：毛俊峰
    /// 时间：2022.
    /// 版本：1.0
    /// </summary>
    public class ABBuild : MonoBehaviour
    {
#if UNITY_EDITOR
        public static void BuildAssetBundle()
        {
            string buildPath = ABSetting.assetBundleBuildPath;
            if (!Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
            }
            BuildPipeline.BuildAssetBundles(buildPath, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
            ////写入资源热更json文件数据
            //ResHotUpdateData resHotUpdateData = new ResHotUpdateData()
            //{
            //    version = "1.2.0",
            //    assetBundleNames = AssetDatabase.GetAllAssetBundleNames()
            //};
            //HotUpdateManager.WriteResHotUpdateData(resHotUpdateData);
            AssetDatabase.Refresh();
        }
#endif
    }
}