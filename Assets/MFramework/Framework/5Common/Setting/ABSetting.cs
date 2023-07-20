using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：AB自动化标记、打包设置
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2022.09.28
    /// 版本：1.0
    /// </summary>
    public class ABSetting
    {
        /// <summary>
        /// 标记AB，对当前(Assets目录下)子路径下资源进行自动标记，
        /// </summary>
        public static string assetsSubPath = "GameMain/AB";
        /// <summary>
        /// 打包AB，AB包存放路径
        /// </summary>
        public static string assetBundleBuildPath = Application.streamingAssetsPath;
        /// <summary>
        /// 默认加载方式 编辑器模式下
        /// </summary>
        public static LoadMode resTypeDefaultEditor = LoadMode.ResEditor;
        /// <summary>
        /// 默认加载方式 非编辑器模式下
        /// </summary>
        public static LoadMode resTypeDefaultNotEditor = LoadMode.ResAssetBundleAsset;
    }
    /// <summary>
    /// 加载方式
    /// </summary>
    public enum LoadMode
    {
        /// <summary>
        /// 根据GameLaunch工程模式 编辑器模式为ABSetting.resTypeDefaultEditor ，打包模式ABSetting.resTypeDefaultNotEditor
        /// </summary>
        Default = 0,
        /// <summary>
        /// 编辑器模式下资源类型  path格式：Assets/AssetsRes/ABRes/Prefab/Cube3.prefab
        /// </summary>
        ResEditor,
        /// <summary>
        /// Resources目录下资源   path格式：(Resources/) + xxx/xxx/xx
        /// </summary>
        ResResources,
        /// <summary>
        /// ab包 .manifest资源     path格式：ab标签名称   (.manifest=>AssetBundleManifest=>Name)
        /// </summary>
        ResAssetBundlePack,
        /// <summary>
        /// ab包中的资源     path格式：ab标签名称   (.manifest=>AssetBundleManifest=>Name)
        /// </summary>
        ResAssetBundleAsset
    }
}