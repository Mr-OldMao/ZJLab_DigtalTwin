using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：Res工厂类
    /// 功能：根据资源种类使用不同加载方式
    /// 作者：毛俊峰
    /// 时间：2022.09.29
    /// 版本：1.0
    /// </summary>
    public class ResFactory : MonoBehaviour
    {
        /// <summary>
        /// 创建资源
        /// </summary>
        /// <param name="assetAllPath"></param>
        /// <param name="resType"></param>
        /// <returns></returns>
        public static AbRes Create(LoadMode resType, string assetAllPath, string assetName)
        {
            AbRes newResData = null;
            switch (resType)
            {
                case LoadMode.ResEditor:
                    newResData = new ResEditor(assetAllPath);
                    break;
                case LoadMode.ResResources:
                    newResData = new ResResources(assetAllPath);
                    break;
                case LoadMode.ResAssetBundlePack:
                    newResData = new ResAssetBundlePack(assetAllPath);
                    break;
                case LoadMode.ResAssetBundleAsset:
                    if (string.IsNullOrEmpty(assetName))
                    {
                        Debug.LogError("assetName is null !  ResType.Asset方式加载资源，需要填写具体资源名 assetName");
                        break;
                    }
                    newResData = new ResAssetBundleAsset(assetAllPath, assetName);
                    break;
                default:
                    break;
            }
            return newResData;
        }
    }
}