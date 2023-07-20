using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：资源实现类的抽象基类
    /// 功能：资源的加载、卸载  缓存资源的状态、路径、被引用的次数、未引用资源自动卸载
    /// 作者：毛俊峰
    /// 时间：2022.08.02
    /// 版本：1.0
    /// </summary>
    public abstract class AbRes : AbRefCounter
    {
        /// <summary>
        /// 加载方式
        /// </summary>
        public LoadMode loadMode;

        /// <summary>
        /// 资源状态类型
        /// </summary>
        public enum ResStateType
        {
            /// <summary>
            /// 资源未加载 刚创建好Res对象
            /// </summary>
            Waiting,
            /// <summary>
            /// 资源正在加载
            /// </summary>
            Loading,
            /// <summary>
            /// 资源加载结束
            /// </summary>
            Loaded
        }

        /// <summary>
        /// 资源状态
        /// </summary>
        public ResStateType ResState { get; protected set; }

        /// <summary>
        /// 资源实例路径
        /// </summary>
        /// <param name="assetAllPath"></param>
        public AbRes(string assetAllPath) { AssetAllPath = assetAllPath; }
        /// <summary>
        /// 资源实例
        /// </summary>
        public UnityEngine.Object Asset { get; set; }
        /// <summary>
        /// 资源(roesouce、ab包、ab包中具体具体资源)的全路径 xx/xxx/xxx
        /// </summary>
        public string AssetAllPath { get; set; }
        /// <summary>
        /// 同步加载资源
        /// </summary>
        public abstract bool LoadSync();
        public abstract void LoadAsync(Action<AbRes> callback);

        /// <summary>
        /// 卸载资源
        /// </summary>
        protected virtual void OnReleaseRes()
        {
            if (Asset)
            {
                Debug.Log("自动释放资源：" + Asset + "，AssetPath：" + AssetAllPath);
                if (Asset is AssetBundle)
                {
                    (Asset as AssetBundle).Unload(true);
                    Asset = null;
                    Resources.UnloadUnusedAssets(); //卸载所有未引用的资源 可能会造成卡顿
                }
                else if (Asset is GameObject || Asset is Component)
                {
                    Asset = null;
                    Resources.UnloadUnusedAssets(); //卸载所有未引用的资源 可能会造成卡顿
                }
                else
                {
                    Resources.UnloadAsset(Asset); //卸载非GameObject、AssetBundle、Component类型资源
                    Asset = null;
                }
            }
            else
            {
                Debug.LogError("自动释放资源失败 Asset is null");
            }
            ResLoader.resContainer.Remove(this);
        }
        /// <summary>
        ///回收资源 资源引用次数为0自动调用
        /// </summary>
        protected override void OnZeroRef()
        {
            OnReleaseRes();
        }
    }
}