using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：资源加载器管理器
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2022.
    /// 版本：1.0
    /// </summary>
    public class ResManager : SingletonByMono<ResManager>
    {
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="resPath">资源路径，具体格式根据加载方式loadModel而定</param>
        /// <param name="loadModel">加载方式</param>
        /// <param name="goCloneReturn">T:GameObject 是否自动克隆并返回</param>
        /// <returns></returns>
        public static T LoadSync<T>(string resPath, LoadMode resType = LoadMode.Default, bool goCloneReturn = true) where T : UnityEngine.Object
        {
            return LoadResource.LoadSync<T>(resPath, resType, goCloneReturn);
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resPath">资源路径，具体格式根据加载方式loadModel而定</param>
        /// <param name="callback">完成回调</param>
        /// <param name="loadModel">资源加载方式</param>
        public static void LoadAsync<T>(string resPath, Action<T> callback, LoadMode resType = LoadMode.Default) where T : UnityEngine.Object
        {
            LoadResource.LoadAsync<T>(resPath, callback, resType);
        }

        /// <summary>
        /// 卸载指定资源
        /// </summary>
        /// <param name="resPath">资源路径</param>
        public static void UnLoadAssets(string resPath, LoadMode loadMode = (LoadMode)(-1))
        {
            ResLoader.UnLoadAssets(resPath, loadMode);
        }
    }
}