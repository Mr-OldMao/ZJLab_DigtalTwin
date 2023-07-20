using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：AssetBundle包的加载、卸载
    /// 功能：对于ab包的同步加载、异步加载、资源卸载具体实现
    /// 作者：毛俊峰
    /// 时间：2022.08.06
    /// 版本：1.0
    /// </summary>
    public class ResAssetBundlePack : AbRes
    {
        private static AssetBundleManifest m_Manifast;
        public static AssetBundleManifest Manifast
        {
            get
            {
                if (!m_Manifast)
                {
                    //加载ab包-AssetBundleManifest路径
                    string assetBundleManifestPath = ABSetting.assetBundleBuildPath + "/StreamingAssets";
                    var mainBundle = AssetBundle.LoadFromFile(assetBundleManifestPath);
                    m_Manifast = mainBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                }
                return m_Manifast;
            }
        }

        public ResAssetBundlePack(string assetAllPath) : base(assetAllPath)
        {
            base.loadMode = LoadMode.ResAssetBundlePack;
            base.AssetAllPath = assetAllPath;
            ResState = ResStateType.Waiting;
        }

        private AssetBundle m_AssetBundle
        {
            get
            {
                return base.Asset as AssetBundle;
            }
            set
            {
                base.Asset = value;
            }
        }

        public override bool LoadSync()
        {
            ResState = ResStateType.Loading;
            //同步加载目标AB包的 依赖AB包
            //string[] splitPath = AssetAllPath.Split('/');
            //string targetAbName = splitPath[splitPath.Length - 1];
            //string[] dependencisAbNameArr = Manifast.GetDirectDependencies(targetAbName);
            string[] dependencisAbNameArr = Manifast.GetDirectDependencies(AssetAllPath);
            foreach (string dependencisAbName in dependencisAbNameArr)
            {
                //目标AB所依赖的AB的全路径
                //string dependencisAbAllPath = AssetAllPath.Replace(targetAbName, dependencisAbName);
                //ResLoader.LoadSync<AssetBundle>(ResType.AssetBundle, dependencisAbAllPath);
                ResLoader.LoadSync<AssetBundle>(LoadMode.ResAssetBundlePack, dependencisAbName);
            }
            //加载AB文件
            string assetBundleAllPath = ABSetting.assetBundleBuildPath + "/" + AssetAllPath;


            m_AssetBundle = AssetBundle.LoadFromFile(assetBundleAllPath);
            ResState = ResStateType.Loaded;
            // todo直接加载AB文件中的资源
            // m_AssetBundle.LoadAsset("资源名")
            return m_AssetBundle;
        }



        public override void LoadAsync(Action<AbRes> callback)
        {
            ResState = ResStateType.Loading;
            //异步加载目标AB包的 依赖AB包
            AsyncLoadDependencisAB(() =>
            {
                //加载AB文件
                string assetBundleAllPath = ABSetting.assetBundleBuildPath + "/" + AssetAllPath;
                AssetBundleCreateRequest rr = AssetBundle.LoadFromFileAsync(assetBundleAllPath);
                rr.completed += (AsyncOperation ao) =>
                {
                    m_AssetBundle = rr.assetBundle;
                    callback?.Invoke(this);
                    ResState = ResStateType.Loaded;
                };
            });
        }

        /// <summary>
        /// 异步加载目标AB包的依赖AB包
        /// </summary>
        /// <param name="loadOverCallback">目标AB包的所有依赖包加载完成回调</param>
        private void AsyncLoadDependencisAB(Action loadOverCallback)
        {
            //string[] splitPath = AssetAllPath.Split('/');
            //string targetAbName = splitPath[splitPath.Length - 1];
            //string[] dependencisAbNameArr = Manifast.GetDirectDependencies(targetAbName);
            string[] dependencisAbNameArr = Manifast.GetDirectDependencies(AssetAllPath);
            if (dependencisAbNameArr.Length == 0)
            {
                loadOverCallback?.Invoke();
                return;
            }
            //已加载完成的依赖包个数
            int dependencisAbLoadedCount = 0;
            foreach (string dependencisAbName in dependencisAbNameArr)
            {
                //目标AB所依赖的AB的全路径
                //string dependencisAbAllPath = AssetAllPath.Replace(targetAbName, dependencisAbName);

                ResLoader.LoadAsync<AssetBundle>(LoadMode.ResAssetBundlePack, (ab) =>
                {
                    dependencisAbLoadedCount++;
                    if (dependencisAbLoadedCount == dependencisAbNameArr.Length)//目标AB包的所有依赖包加载完成
                    {
                        loadOverCallback?.Invoke();
                    }
                }, dependencisAbName);
            }
        }
        protected override void OnReleaseRes()
        {
            base.OnReleaseRes();
        }
    }
}