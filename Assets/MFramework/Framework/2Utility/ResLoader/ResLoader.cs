using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：资源加载器
    /// 功能：管理Editor资源、Resource资源、ab包、ab包中具体资源的同步加载、异步加载、资源卸载
    /// 作者：毛俊峰
    /// 时间：2022.07.24
    /// 版本：1.0
    /// </summary>
    public class ResLoader
    {
        /// <summary>
        /// 缓存全局正在使用的资源容器    唯一标识：资源路径
        /// </summary>
        public static List<AbRes> resContainer = new List<AbRes>();

        #region 加载资源

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <typeparam name="T">U3D资源类型</typeparam>
        /// <param name="loadMode">加载方式</param>
        /// <param name="resPath">资源种类所对应路径   ab包资源、resources资源全路径  格式：xx/xxx/xxx    </param>
        /// <param name="assetName">具体资源名称 仅ResType.ResAssetBundleAsset资源种类填写</param>
        /// <returns></returns>
        public static T LoadSync<T>(LoadMode loadMode, string resPath, string assetName = "") where T : UnityEngine.Object
        {
            AbRes resData = FindResInfoByResContainer(loadMode, resPath);
            if (resData != null)
            {
                //判断资源加载的状态
                switch (resData.ResState)
                {
                    case AbRes.ResStateType.Loading:
                        throw new Exception("当前资源正在加载中！注意：对于同一个资源，请不要在异步加载当前资源未加载完毕时，使用同步加载该资源  assetAllPath：" + resPath);
                    case AbRes.ResStateType.Loaded:
                        resData.Release();
                        Debug.Log("同步加载资源 资源类型：" + loadMode + "，当前资源引用次数:" + CheckResExist(resPath, loadMode)?.RefCount + "，资源池资源总数：" + resContainer.Count);
                        return resData.Asset as T;
                }
            }
            AbRes newResData = ResFactory.Create(loadMode, resPath, assetName);
            newResData.LoadSync();
            newResData.Release();
            resContainer.Add(newResData);
            Debug.Log("同步加载新资源 资源类型：" + loadMode + "，当前资源引用次数:" + CheckResExist(resPath, loadMode)?.RefCount + "，资源池资源总数：" + resContainer.Count);
            return newResData.Asset as T;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="resType">资源种类</param>
        /// <param name="callback">回调<U3D资源类型></param>
        /// <param name="resPath">资源 Resources下的全路径  格式：xx/xxx/xxx</param>
        /// <param name="resName">具体资源名称 仅ResType.ResAssetBundleAsset资源种类填写</param>
        /// <returns></returns>
        public static void LoadAsync<T>(LoadMode resType, Action<T> callback, string resPath, string resName = "") where T : UnityEngine.Object
        {
            AbRes resData = FindResInfoByResContainer(resType, resPath);
            if (resData != null)
            {
                //判断资源加载的状态
                switch (resData.ResState)
                {
                    case AbRes.ResStateType.Loading:
                        //等待资源加载完毕 回调
                        //Debug.Log("该资源正在异步加载中，等待资源加载完毕 回调");
                        Debug.Log("异步加载资源开始 资源类型：" + resType + "，当前资源引用次数:" + CheckResExist(resPath, resType)?.RefCount + "，资源池资源总数：" + resContainer.Count);
                        UnityTool.GetInstance.DelayCoroutineWaitReturnTrue(() =>
                        {
                            return resData.ResState == AbRes.ResStateType.Loaded;
                        }, () =>
                        {
                            resData.Release();
                            callback(resData.Asset as T);
                            Debug.Log("异步加载新资源结束 资源类型：" + resType + "，当前资源引用次数:" + CheckResExist(resPath, resType)?.RefCount + "，资源池资源总数：" + resContainer.Count);
                        });
                        return;
                    case AbRes.ResStateType.Loaded:
                        resData.Release();
                        callback(resData.Asset as T);
                        Debug.Log("异步加载资源结束 资源类型：" + resType + "，当前资源引用次数:" + CheckResExist(resPath, resType)?.RefCount + "，资源池资源总数：" + resContainer.Count);
                        return;
                }
            }
            AbRes newResData = ResFactory.Create(resType, resPath, resName);
            newResData.LoadAsync((AbRes p) => callback(p.Asset as T));
            newResData.Release();
            resContainer.Add(newResData);
        }

        /// <summary>
        /// 在资源容器根据查找资源根据资源路径
        /// </summary>
        /// <returns></returns>
        private static AbRes FindResInfoByResContainer(LoadMode resType, string assetAllPath)
        {
            return resContainer.Find(loadedAsset => loadedAsset.loadMode == resType && loadedAsset.AssetAllPath == assetAllPath);
        }
        #endregion

        #region 卸载资源

        /// <summary>
        /// 卸载所有资源
        /// </summary>
        public static void UnLoadAllAssets()
        {
            for (int i = 0; i < resContainer.Count; i++)
            {
                try
                {
                    if (resContainer[i].Asset?.GetType() == typeof(AssetBundle))
                    {
                        AssetBundle ab = resContainer[i].Asset as AssetBundle;
                        ab.Unload(true);
                    }
                    else
                    {
                        Resources.UnloadAsset(resContainer[i].Asset);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("UnLoadAllAssets e: " + e);
                }
            }
            resContainer.Clear();
        }


        /// <summary>
        /// 卸载指定资源
        /// </summary>
        /// <param name="resPath">资源路径</param>
        //public static void UnLoadAssets(string resPath)
        //{
        //    for (int i = 0; i < resContainer.Count; i++)
        //    {
        //        if (resContainer[i].AssetAllPath == resPath)
        //        {
        //            resContainer[i].Retain();
        //            break;
        //        }
        //    }
        //    if (CheckResExist(resPath) == null)
        //    {
        //        Debug.Log("卸载资源 ，资源路径：" + resPath + "，当前资源已被释放，资源池资源总数：" + resContainer.Count);
        //    }
        //    else
        //    {
        //        Debug.Log("卸载资源 ，资源路径：" + resPath + "，引用次数:" + CheckResExist(resPath)?.RefCount + "，资源池资源总数：" + resContainer.Count);
        //    }
        //}

        /// <summary>
        /// 卸载指定资源
        /// </summary>
        /// <param name="resPath">资源路径</param>
        public static void UnLoadAssets(string resPath, LoadMode resType = (LoadMode)(-1))
        {
            for (int i = 0; i < resContainer.Count; i++)
            {
                if (resContainer[i].loadMode == resType && resContainer[i].AssetAllPath == resPath)
                {
                    resContainer[i].Retain();
                    break;
                }
            }
            if (CheckResExist(resPath, resType) == null)
            {
                Debug.Log("卸载资源 资源类型：" + resType + "，资源路径：" + resPath + "，当前资源已被释放，资源池资源总数：" + resContainer.Count);
            }
            else
            {
                Debug.Log("卸载资源 资源类型：" + resType + "，资源路径：" + resPath + "，引用次数:" + CheckResExist(resPath, resType)?.RefCount + "，资源池资源总数：" + resContainer.Count);
            }
        }
        #endregion

        #region 获取资源信息
        /// <summary>
        /// 判定资源是否已被加载
        /// </summary>
        /// <param name="assetAllPath">资源 Resources下的全路径  格式：xx/xxx/xxx</param>
        public static AbRes CheckResExist(string assetAllPath, LoadMode resType)
        {
            AbRes resData = resContainer.Find(loadedAsset => loadedAsset.AssetAllPath == assetAllPath && loadedAsset.loadMode == resType);
            return resData;
        }

        /// <summary>
        /// 判定资源是否已被加载
        /// </summary>
        /// <param name="assetAllPath">资源 Resources下的全路径  格式：xx/xxx/xxx</param>
        public static AbRes CheckResExist(string assetAllPath)
        {
            AbRes resData = resContainer.Find(loadedAsset => loadedAsset.AssetAllPath == assetAllPath);
            return resData;
        }

        /// <summary>
        /// 判定资源是否已被加载
        /// </summary>
        /// <typeparam name="T">资源的类型</typeparam>
        /// <param name="assetAllPath">资源 Resources下的全路径  格式：xx/xxx/xxx</param>
        /// <returns></returns>
        public static AbRes CheckResExist<T>(string assetAllPath) where T : UnityEngine.Object
        {
            AbRes resData = resContainer.Find(loadedAsset => loadedAsset.AssetAllPath == assetAllPath && typeof(T) == loadedAsset.Asset.GetType());
            return resData;
        }

        /// <summary>
        /// 显示当前资源信息
        /// </summary>
        public static void ShowResLogInfo()
        {
            Debug.Log("显示当前资源信息\n资源总个数：" + resContainer.Count);
            foreach (AbRes resData in resContainer)
            {
                Debug.Log(string.Format("资源类型：{0} 资源实例{1}，位置{2}，引用次数{3}", resData.loadMode, resData.Asset, resData.AssetAllPath, resData.RefCount));
            }
        }
        #endregion
    }
}