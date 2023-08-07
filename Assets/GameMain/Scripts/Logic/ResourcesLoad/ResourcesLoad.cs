using System.Collections.Generic;
using UnityEngine;
using MFramework;
using UnityEngine.AddressableAssets;
using static UnityEngine.AddressableAssets.Addressables;

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

    /// <summary>
    /// 所有实体个数  "ItemLable"+"RoomBorderLable"
    /// </summary>
    public const uint AllEntityResCount = 11 + 7;
    private uint m_CurLoadedCount;

    private List<string> m_AddressableLable = new List<string> { "ItemLable", "RoomBorderLable" };

    /// <summary>
    /// 是否已加载过资源
    /// </summary>
    private bool m_IsLoadedRes = false;

    private void Init()
    {
        dicCacheEntityRes = new Dictionary<string, GameObject>();
        m_IsLoadedRes = false;
        m_CurLoadedCount = 0;
    }

    /// <summary>
    /// 异步加载所有实体资源
    /// </summary>
    /// <param name="loadCompleteCallback">所有资源加载完成回调</param>
    public void AsyncLoadAllResources()
    {
        if (m_IsLoadedRes)
        {
            MsgEvent.SendMsg(MsgEventName.AsyncLoadedComplete);
            return;
        }
        Debug.Log("开始加载资源 ");
        Init();
        Addressables.LoadAssetsAsync<GameObject>(m_AddressableLable, (p) =>
        {
            AddEntityRes(p.name, p);
            m_CurLoadedCount++;
            GetLoadProgress();
        }, MergeMode.Union, true).Completed += (UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<IList<GameObject>> obj) =>
        {
            Debug.Log("所有资源加载完成");
            m_IsLoadedRes = true;
            MsgEvent.SendMsg(MsgEventName.AsyncLoadedComplete);
        };
    }

    public GameObject GetEntityRes(string entityName)
    {
        GameObject res = null;
        if (m_IsLoadedRes)
        {
            if (dicCacheEntityRes != null)
            {
                if (dicCacheEntityRes.ContainsKey(entityName))
                {
                    res = dicCacheEntityRes[entityName];
                }
                else
                {
                    Debug.LogError("dicCacheEntityRes not exist，entityName：" + entityName);
                }
            }
            else
            {
                Debug.LogError("dicCacheEntityRes is null");
            }
        }
        else
        {
            Debug.LogError("Load Res not Complete");
        }
        return res;
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

    private void GetLoadProgress()
    {
        Debug.Log("LoadProgress：" + (((float)m_CurLoadedCount / AllEntityResCount) * 100).ToString("#0.0") + "%");
    }
}
