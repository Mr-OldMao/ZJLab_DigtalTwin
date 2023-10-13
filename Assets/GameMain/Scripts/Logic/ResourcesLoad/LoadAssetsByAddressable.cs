using System.Collections.Generic;
using UnityEngine;
using MFramework;
using UnityEngine.AddressableAssets;
using static UnityEngine.AddressableAssets.Addressables;
using System;
using System.IO;
using System.Collections;

/// <summary>
/// 标题：基于Addressable进行ab包的异步加载
/// 功能：资源批量加载，单个加载，
/// 描述：
/// 加载时机：
/// 1.饿汉式加载，程序启动时资源预加载
/// 2.懒汉式加载，使用资源时加载
/// 加载方式：
/// 1.批量加载根据标签Addressables Lable
/// 2.单个加载根据资源路径(Assets/GamMain/Ab/Prefab/...)
/// 3.批量加载根据编辑器目录
/// 作者：毛俊峰
/// 时间：2023.8.4
/// </summary>
public class LoadAssetsByAddressable : SingletonByMono<LoadAssetsByAddressable>
{
    /// <summary>
    /// 缓存实体资源  k-资源名称 v-资源实体信息
    /// </summary>
    public Dictionary<string, ResInfo> dicCacheAssets;

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
    /// 所有实体个数
    /// </summary>
    public uint AllAssetsCount
    {
        get;
        set;
    } = 19 + 7 + 1 + 1;

    private uint m_CurLoadedCount;

    public void DicIndex()
    {
        List<ResInfo> resInfos = new List<ResInfo>();
        resInfos[0] = new ResInfo();
    }

    private void Init()
    {
        dicCacheAssets = new Dictionary<string, ResInfo>();
        m_CurLoadedCount = 0;
    }

    #region LoadAssets

    /// <summary>
    /// 批量资源异步加载根据标签
    /// </summary>
    /// <param name="lables"></param>
    /// <param name="callbackLoadedComplete"></param>
    public void LoadAssetsAsyncByLable(List<string> lables, Action callbackLoadedComplete = null)
    {
        if (lables == null || lables.Count == 0)
        {
            return;
        }
        Debug.Log("Loading Assets ...");
        Init();
        Addressables.LoadAssetsAsync<GameObject>(lables, (p) =>
        {
            AddEntityRes(p.name, p);
            m_CurLoadedCount++;
            //GetLoadProgress(AllAssetsCount);
        }, MergeMode.Union, true).Completed += (UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<IList<GameObject>> obj) =>
        {
            Debug.Log("Loading Assets Complete");
            callbackLoadedComplete?.Invoke();
        };
    }

    /// <summary>
    /// 单个资源异步加载根据资源路径
    /// </summary>
    /// <param name="assetPath">Assets/GameMain/AB/xxx.xx</param>
    /// <param name="callbackLoadedComplete"></param>
    public void LoadAssetAsyncByAssetPath(string assetPath, Action<GameObject> callbackLoadedComplete = null, bool autoInstantiate = true)
    {
        LoadAssetAsyncByAssetPath<GameObject>(assetPath, callbackLoadedComplete, autoInstantiate);
    }

    /// <summary>
    /// 单个资源异步加载根据资源路径
    /// </summary>
    /// <param name="assetPath">Assets/GameMain/AB/xxx.xx</param>
    /// <param name="callbackLoadedComplete"></param>
    public void LoadAssetAsyncByAssetPath<T>(string assetPath, Action<T> callbackLoadedComplete = null, bool autoInstantiate = true) where T : UnityEngine.Object
    {
        LoadAssetAsync<T>(assetPath).Completed
            += (UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<T> obj) =>
            {
                T targetObject = autoInstantiate ? Instantiate<T>(obj.Result) : obj.Result;
                callbackLoadedComplete?.Invoke(targetObject);
            };
    }

    /// <summary>
    /// 批量资源异步加载文件夹下的所有资源文件，自动识别资源类型
    /// </summary>
    /// <param name="dirPath">Assets下的路径 格式 /xxx/xxx/xx</param>
    /// <param name="callbackLoadedComplete"></param>
    public void LoadAssetsAsyncByDirectory(string dirPath = "/GameMain/AB/", Action callbackLoadedComplete = null)
    {
        dirPath = Application.dataPath + dirPath;
        List<string> assetsPathArr = new List<string>();
        GetAllAssetsPath(dirPath, ref assetsPathArr);
        Debug.Log(assetsPathArr);

        int curLoadedCount = 0;
        for (int i = 0; i < assetsPathArr.Count; i++)
        {
            //解析文件路径
            string assetPath = assetsPathArr[i].Split(@"\Assets\")[1].Replace('\\', '/');
            assetPath = "Assets/" + assetPath;
            //自动解析资源类型
            if (assetsPathArr[i].EndsWith(".prefab"))
            {
                LoadAssetAsyncByAssetPath<GameObject>(assetPath, (o) =>
                {
                    curLoadedCount++;
                    //缓存TODO
                }, false);
            }
            else if (assetsPathArr[i].EndsWith(".mat"))
            {
                LoadAssetAsyncByAssetPath<Material>(assetPath, (o) =>
                {
                    curLoadedCount++;
                    //缓存TODO
                }, false);
            }
            else if (assetsPathArr[i].EndsWith(".png") || assetsPathArr[i].EndsWith(".jpg") || assetsPathArr[i].EndsWith(".tga"))
            {
                LoadAssetAsyncByAssetPath<Texture>(assetPath, (o) =>
                {
                    curLoadedCount++;
                    //缓存TODO
                }, false);
            }
            else
            {
                Debug.LogError("自动解析资源类型失败，请新增");
            }
        }
        UnityTool.GetInstance.DelayCoroutineWaitReturnTrue(() => { return curLoadedCount == assetsPathArr.Count; }, () =>
        {
            Debug.Log("LoadAssetsAsyncByDirectory callbackLoadedComplete");
            callbackLoadedComplete?.Invoke();
        });
    }

    /// <summary>
    /// 获取文件夹下所有资源路径
    /// </summary>
    /// <param name="forderPath">目录路径</param>
    /// <param name="assetsName"></param>
    private void GetAllAssetsPath(string forderPath, ref List<string> assetsName)
    {
        DirectoryInfo direction = new DirectoryInfo(forderPath);
        FileInfo[] files = direction.GetFiles("*", SearchOption.TopDirectoryOnly);
        for (int i = 0; i < files.Length; i++)
        {
            int tempI = i;
            if (files[tempI].Name.EndsWith(".meta"))
            {
                continue;
            }
            if (!assetsName.Contains(files[tempI].FullName))
            {
                assetsName.Add(files[tempI].FullName);
            }
            else
            {
                Debug.LogError("assets exist，assetsName：" + files[tempI].FullName);
            }
        }

        //下一层所有文件夹
        //文件夹下一层的所有文件夹
        DirectoryInfo[] folders = direction.GetDirectories("*", SearchOption.TopDirectoryOnly);
        for (int i = 0; i < folders.Length; i++)
        {
            int tempI = i;
            GetAllAssetsPath(folders[tempI].FullName, ref assetsName);
        }

    }
    #endregion





    /// <summary>
    /// 获取实体资源
    /// </summary>
    /// <param name="entityName">实体类型</param>
    /// <param name="assetName">获取指定资源填写，默认assetName为空，获取随机资源，不为空则获取指定资源，若不为空且未找到指定资源则获取随机资源</param>
    /// <returns></returns>
    public GameObject GetEntityRes(string entityName, string assetName = "")
    {
        GameObject res = null;
        if (dicCacheAssets != null)
        {
            if (dicCacheAssets.ContainsKey(entityName))
            {
                //if (!string.IsNullOrEmpty(assetName))
                //{
                //    foreach (var item in dicCacheAssets[entityName].items)
                //    {
                //        Debug.Log(item);
                //        Debug.Log(item.ToString());
                //        Debug.Log(item.name);
                //        if (item.name == assetName)
                //        {
                //            res = item;
                //            break;
                //        }
                //    }
                //    if (res == null)
                //    {
                //        Debug.LogError("未找到指定资源，改用随机资源");
                //        int randomIndex = UnityEngine.Random.Range(0, dicCacheAssets[entityName].items.Count);
                //        res = dicCacheAssets[entityName].items[randomIndex];
                //    }
                //}
                //else
                //{
                int randomIndex = UnityEngine.Random.Range(0, dicCacheAssets[entityName].items.Count);
                res = dicCacheAssets[entityName].items[randomIndex];
                //}
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
        return res;
    }

    public void GetEntityRes(string entityName, string assetName, Action<GameObject> callback)
    {
        if (dicCacheAssets != null && dicCacheAssets.ContainsKey(entityName))
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                GameObject res = null;
                foreach (var item in dicCacheAssets[entityName].items)
                {
                    if (item.name == assetName)
                    {
                        res = item;
                        break;
                    }
                }
                if (res == null)
                {
                    Debug.LogError("未找到指定资源，改用随机资源");
                    int randomIndex = UnityEngine.Random.Range(0, dicCacheAssets[entityName].items.Count);
                    res = dicCacheAssets[entityName].items[randomIndex];
                }
                callback?.Invoke(res);

            });
        }
    }
    private IEnumerator Do()
    {
        // 只能在主线程执行的语句
        // ...
        Debug.Log(dicCacheAssets["Food"].items);
        yield return null;
    }


    private void AddEntityRes(string itemName, GameObject value)
    {
        //解析物品名称
        string parseItemName = itemName.Split('_')[0];
        ResInfo resInfo = null;
        if (!dicCacheAssets.ContainsKey(parseItemName))
        {
            resInfo = new ResInfo()
            {
                items = new List<GameObject>() { value },
                limitItems = null
            };
            dicCacheAssets.Add(parseItemName, resInfo);
        }
        else
        {
            resInfo = dicCacheAssets[parseItemName];
            resInfo.items.Add(value);
        }

    }

    private void GetLoadProgress(uint AllAssetsCount)
    {
        Debug.Log("Load Assets Progress：" + (((float)m_CurLoadedCount / AllAssetsCount) * 100).ToString("#0.0") + "%");
    }
}
