using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：AB包中的具体资源 加载、卸载
    /// 功能：对于ab包中资源的同步加载、异步加载、资源卸载具体实现
    /// 作者：毛俊峰
    /// 时间：2022.
    /// 版本：1.0
    /// </summary>
    public class ResAssetBundleAsset : AbRes
    {
        /// <summary>
        /// ab包中的资源名
        /// </summary>
        private string m_AssetName;

        public ResAssetBundleAsset(string assetAllPath, string assetName) : base(assetAllPath)
        {
            base.loadMode = LoadMode.ResAssetBundleAsset;
            m_AssetName = assetName;
            base.AssetAllPath = assetAllPath;
            ResState = ResStateType.Waiting;
        }

        public override void LoadAsync(Action<AbRes> callback)
        {
            ResState = ResStateType.Loading;
            ResLoader.LoadAsync<AssetBundle>(LoadMode.ResAssetBundlePack, (ab) =>
            {
                base.Asset = ab.LoadAsset(m_AssetName); //需要加后缀
                callback(this);
                //base.AssetAllPath = base.AssetAllPath + "/" + m_AssetName;//把base.AssetAllPath从ab包路径更新为ab包具体资源路径
                ResState = ResStateType.Loaded;
            }, base.AssetAllPath);
        }

        public override bool LoadSync()
        {
            ResState = ResStateType.Loading;
            AssetBundle ab = null;
            if (ResLoader.CheckResExist(base.AssetAllPath, LoadMode.ResAssetBundlePack) == null)
            {
                ab = ResLoader.LoadSync<AssetBundle>(LoadMode.ResAssetBundlePack, base.AssetAllPath);
            }
            else
            {
                ab = ResLoader.CheckResExist(base.AssetAllPath, LoadMode.ResAssetBundlePack).Asset as AssetBundle;
            }
            //ab = ResLoader.LoadSync<AssetBundle>(ResType.ResAssetBundlePack, base.AssetAllPath);
            base.Asset = ab.LoadAsset(m_AssetName); //需要加后缀
            //base.AssetAllPath = base.AssetAllPath + "/" + m_AssetName;

            ResState = ResStateType.Loaded;
            return base.Asset;
        }

        protected override void OnReleaseRes()
        {
            base.OnReleaseRes();
        }
    }
}