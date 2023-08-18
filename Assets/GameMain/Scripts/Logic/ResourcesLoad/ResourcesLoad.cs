using System.Collections.Generic;
using UnityEngine;
using MFramework;
using UnityEngine.AddressableAssets;
using static UnityEngine.AddressableAssets.Addressables;
using System;
using Unity.VisualScripting;

/// <summary>
/// 标题：资源加载
/// 功能：基于Addressable进行ab包的异步加载
/// 作者：毛俊峰
/// 时间：2023.8.4
/// </summary>
public class ResourcesLoad : SingletonByMono<ResourcesLoad>
{
    /// <summary>
    /// 缓存实体资源  k-资源名称 v-资源实体信息
    /// </summary>
    public Dictionary<string, ResInfo> dicCacheEntityRes;

    /// <summary>
    /// 实体资源信息
    /// </summary>
    public class ResInfo
    {
        public List<GameObject> items = null;
        /// <summary>
        /// 当前实体放置限制，只能放置在此类对象上
        /// </summary>
        public List<GameObject> limitItems = null;
    }

    /// <summary>
    /// 所有实体个数  "ItemLable"+"RoomBorderLable"
    /// </summary>
    public const uint AllEntityResCount = 14 + 7;
    private uint m_CurLoadedCount;

    private List<string> m_AddressableLable = new List<string> { "ItemLable", "RoomBorderLable" };
    public void DicIndex()
    {
        List<ResInfo> resInfos = new List<ResInfo>();
        resInfos[0] = new ResInfo();
    }
    /// <summary>
    /// 是否已加载过资源
    /// </summary>
    private bool m_IsLoadedRes = false;

    private void Init()
    {
        dicCacheEntityRes = new Dictionary<string, ResInfo>();
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

    /// <summary>
    /// 获取实体资源
    /// </summary>
    /// <param name="entityName"></param>
    /// <param name="index">默认index = -1获取随机资源，index > -1获取指定资源，若index越界则获取随机资源</param>
    /// <returns></returns>
    public GameObject GetEntityRes(string entityName, int index = -1)
    {
        GameObject res = null;
        if (m_IsLoadedRes)
        {
            if (dicCacheEntityRes != null)
            {
                if (dicCacheEntityRes.ContainsKey(entityName))
                {
                    if (index != -1 && index < dicCacheEntityRes[entityName].items.Count)
                    {
                        res = dicCacheEntityRes[entityName].items[index];
                    }
                    else
                    {
                        int randomIndex = UnityEngine.Random.Range(0, dicCacheEntityRes[entityName].items.Count);
                        res = dicCacheEntityRes[entityName].items[randomIndex];
                    }
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

    private void AddEntityRes(string itemName, GameObject value)
    {
        //解析物品名称
        string parseItemName = itemName.Split('_')[0];
        ResInfo resInfo = null;
        if (!dicCacheEntityRes.ContainsKey(parseItemName))
        {
            resInfo = new ResInfo()
            {
                items = new List<GameObject>() { value },
                limitItems = null
            };
            dicCacheEntityRes.Add(parseItemName, resInfo);
        }
        else
        {
            resInfo = dicCacheEntityRes[parseItemName];
            resInfo.items.Add(value);
        }
        
    }

    private void GetLoadProgress()
    {
        Debug.Log("LoadProgress：" + (((float)m_CurLoadedCount / AllEntityResCount) * 100).ToString("#0.0") + "%");
    }
}
