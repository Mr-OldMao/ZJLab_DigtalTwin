using System.Collections.Generic;
using UnityEngine;
using MFramework;
using UnityEngine.AddressableAssets;
using System;

/// <summary>
/// 标题：资源加载
/// 功能：基于Addressable进行ab包的异步加载
/// 作者：毛俊峰
/// 时间：2023.8.4
/// </summary>
public class ResourcesLoad : SingletonByMono<ResourcesLoad>
{
    /// <summary>
    /// 缓存实体资源  k-资源名称 v-资源实体
    /// </summary>
    public Dictionary<string, GameObject> dicCacheEntityRes;

    public const uint AllEntityResCount = 11;

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        AsyncLoadResources(() => Debug.Log("todo"));
    //    }
    //}

    /// <summary>
    /// 是否已加载过资源
    /// </summary>
    private bool m_IsLoadedRes = false;

    private void Awake()
    {
        dicCacheEntityRes = new Dictionary<string, GameObject>();
    }

    /// <summary>
    /// 异步加载实体资源
    /// </summary>
    /// <param name="loadCompleteCallback">所有资源加载完成回调</param>
    public void AsyncLoadResources(Action loadCompleteCallback)
    {
        if (m_IsLoadedRes)
        {
            loadCompleteCallback?.Invoke();
            return;
        }

        Debug.Log("开始加载资源");
        Addressables.LoadAssetsAsync<GameObject>("ItemLable", (p) =>
        {
            AddEntityRes(p.name, p);
            if (dicCacheEntityRes.Count == AllEntityResCount)
            {
                Debug.Log("所有资源加载完成");
                m_IsLoadedRes = true;
                loadCompleteCallback?.Invoke();
            }
        });
    }

    private void AddEntityRes(string key, GameObject value)
    {
        if (!dicCacheEntityRes.ContainsKey(key))
        {
            Debug.Log("add key:" + key);
            dicCacheEntityRes.Add(key, value);
        }
        else
        {
            Debug.LogError("resources is exist , resName:" + key);
        }
    }
}
