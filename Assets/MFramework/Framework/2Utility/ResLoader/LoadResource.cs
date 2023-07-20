using System;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：资源同步加载、异步加载、卸载、计数，对ResLoader进行封装
    /// 功能：对以下五类资源支持，
    ///       1.编辑器下的资源类型 UnityEditor.AssetDatabase.LoadAssetAtPath
    ///       2.AB包
    ///       3.AB包中资源
    ///       4.Resources目录下资源
    /// 作者：毛俊峰
    /// 时间：2022.09.29
    /// 版本：1.0
    /// </summary>
    public class LoadResource : SingletonByMono<LoadResource>
    {
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="resPath">资源路径，具体格式根据加载方式loadModel而定</param>
        /// <param name="loadModel">加载方式</param>
        /// <param name="goCloneReturn">T:GameObject 是否自动克隆并返回</param>
        /// <returns></returns>
        public static T LoadSync<T>(string resPath, LoadMode loadModel = LoadMode.Default, bool goCloneReturn = true) where T : UnityEngine.Object
        {
            string assetName = string.Empty; //具体资源名称 仅ResType.ResAssetBundleAsset资源种类填写
            string parsedAssetPath = resPath;

            switch (loadModel)
            {
                case LoadMode.Default:
                    //编辑器下默认加载类型
#if UNITY_EDITOR
                    if (GameLaunch.GetInstance.LaunchModel == LaunchModel.EditorModel)
                    {
                        loadModel = ABSetting.resTypeDefaultEditor;
                    }
                    else
                    {
                        loadModel = ABSetting.resTypeDefaultNotEditor;
                        parsedAssetPath = ParseAssetPath(resPath);
                        assetName = ParseAssetName(resPath);
                    }
#else
                    loadModel = ABSetting.resTypeDefaultNotEditor;
                    parsedAssetPath = ParseAssetPath(resPath);
                    assetName = ParseAssetName(resPath);
#endif
                    break;
                case LoadMode.ResEditor:
                    break;
                case LoadMode.ResResources:
                    break;
                case LoadMode.ResAssetBundlePack:
                    parsedAssetPath = ParseAssetPath(resPath);
                    break;
                case LoadMode.ResAssetBundleAsset:
                    loadModel = ABSetting.resTypeDefaultNotEditor;
                    parsedAssetPath = ParseAssetPath(resPath);
                    assetName = ParseAssetName(resPath);
                    break;
                default:
                    break;
            }
            T asset = ResLoader.LoadSync<T>(loadModel, parsedAssetPath, assetName);
            if (typeof(T) == typeof(GameObject) && goCloneReturn)
            {
                if (asset != null)
                {
                    return Instantiate(asset);
                }
            }
            return asset;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resPath">资源路径，具体格式根据加载方式loadModel而定</param>
        /// <param name="callback">完成回调</param>
        /// <param name="loadModel">资源加载方式</param>
        public static void LoadAsync<T>(string resPath, Action<T> callback, LoadMode loadModel = LoadMode.Default) where T : UnityEngine.Object
        {
            string assetName = string.Empty; //具体资源名称 仅ResType.ResAssetBundleAsset资源种类填写
            string parsedAssetPath = resPath;

            switch (loadModel)
            {
                case LoadMode.Default:
#if UNITY_EDITOR
                    if (GameLaunch.GetInstance.LaunchModel == LaunchModel.EditorModel)
                    {
                        loadModel = ABSetting.resTypeDefaultEditor;
                    }
                    else
                    {
                        loadModel = ABSetting.resTypeDefaultNotEditor;
                        parsedAssetPath = ParseAssetPath(resPath);
                        assetName = ParseAssetName(resPath);
                    }
#else
                    loadModel = ABSetting.resTypeDefaultNotEditor;
                    parsedAssetPath = ParseAssetPath(resPath);
                    assetName = ParseAssetName(resPath);
#endif
                    break;
                case LoadMode.ResEditor:
                    break;
                case LoadMode.ResResources:
                    break;
                case LoadMode.ResAssetBundlePack:
                    parsedAssetPath = ParseAssetPath(resPath);
                    break;
                case LoadMode.ResAssetBundleAsset:
                    parsedAssetPath = ParseAssetPath(resPath);
                    assetName = ParseAssetName(resPath);
                    break;
                default:
                    break;
            }
            ResLoader.LoadAsync<T>(loadModel, callback, parsedAssetPath, assetName);
        }



        /// <summary>
        /// 解析目标资源的AB路径
        /// </summary>
        public static string ParseAssetPath(string path)
        {
            string[] pathSplitArr = path.Split('/', '.');
            //字符串转换 去头去后缀转小写,路径全为小写，且不允许有后缀 Assets/AssetsRes/ABRes/Prefab/Cube1.prefab =》assetsres/abres/prefab/cube1
            string abPath = path.Replace(pathSplitArr[0] + "/", "").Replace("." + pathSplitArr[pathSplitArr.Length - 1], "").ToLower();
            //Debug.Log("TAB abPath   " + abPath);
            return abPath;
        }

        /// <summary>
        /// 解析目标资源的AB名称
        /// </summary>
        private static string ParseAssetName(string path)
        {
            string[] pathSplitArr = path.Split('/', '.');
            //提取资源名称 Assets/AssetsRes/ABRes/Prefab/Cube1.prefab =》cube1
            string abName = pathSplitArr[pathSplitArr.Length - 2].ToLower();
            //Debug.Log("TAB abName   " + abName);
            return abName;
        }

    }
}