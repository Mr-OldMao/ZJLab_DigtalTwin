using System;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：Editor自定义路径资源类型 同步加载、异步加载、卸载
    /// 功能：UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
    /// 作者：毛俊峰
    /// 时间：2022.09.29
    /// 版本：1.0
    /// </summary>
    public class ResEditor : AbRes
    {
        public ResEditor(string assetAllPath) : base(assetAllPath)
        {
            base.loadMode = LoadMode.ResEditor;
            base.AssetAllPath = assetAllPath;
            ResState = ResStateType.Waiting;
        }

        public override bool LoadSync()
        {
            ResState = ResStateType.Loading;
#if UNITY_EDITOR
            Asset = AssetDatabaseTool.LoadAssetAtPath<GameObject>(AssetAllPath);
#else
            Debug.LogError("非编辑器模式下无法进行UnityEditor.AssetDatabase.LoadAssetAtPath 方式资源加载");
#endif
            ResState = ResStateType.Loaded;
            return Asset;
        }

        public override void LoadAsync(Action<AbRes> callback)
        {
            ResState = ResStateType.Loading;
            ResourceRequest rr = Resources.LoadAsync(AssetAllPath);
            rr.completed += (AsyncOperation ao) =>
            {
                Asset = rr.asset;
                ResState = ResStateType.Loaded;
                callback?.Invoke(this);
            };
        }

        protected override void OnReleaseRes()
        {
            base.OnReleaseRes();
        }
    }
}