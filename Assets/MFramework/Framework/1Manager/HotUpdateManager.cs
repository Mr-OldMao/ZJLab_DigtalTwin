using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MFramework
{
    /// <summary>
    /// 标题：热更新管理器
    /// 功能：1.检查是否需要热更，通过本地版本号与服务器版本号对比判断
    ///       2.资源下载
    ///       3.资源替换
    /// 作者：毛俊峰
    /// 时间：2022.10.09
    /// 版本：1.0
    /// </summary>
    public class HotUpdateManager : SingletonByMono<HotUpdateManager>
    {
        private static HotUpdateState m_CurHotUpdateState;

        /// <summary>
        /// 获取当前热更状态
        /// </summary>
        public static HotUpdateState CurHotUpdateState
        {
            get
            {
                //本地版本信息文件全路径
                string filePath = HotUpdateSetting.hotUpdatedLocalVersionRootPath + "/" + HotUpdateSetting.hotUpdateVersionFileName;
                if (!File.Exists(filePath))
                {
                    m_CurHotUpdateState = HotUpdateState.NeverUpdate;
                    Debug.Log("CurHotUpdateState：" + m_CurHotUpdateState + "，version：" +
                        JsonUtility.FromJson<ResHotUpdateData>(new FileIOTxt(HotUpdateSetting.localVersionRootPath, HotUpdateSetting.hotUpdateVersionFileName).Read()).version);
                }
                else //Application.persistentDataPath资源版本 > Application.streamingAssetsPath资源版本
                {
                    m_CurHotUpdateState = HotUpdateState.Updated;
                    string hotUpdatedVersionJson = new FileIOTxt(HotUpdateSetting.hotUpdatedLocalVersionRootPath, HotUpdateSetting.hotUpdateVersionFileName).Read();
                    string versionJson = new FileIOTxt(HotUpdateSetting.localVersionRootPath, HotUpdateSetting.hotUpdateVersionFileName).Read();
                    ResHotUpdateData hotUpdatedVersionInfo = JsonUtility.FromJson<ResHotUpdateData>(hotUpdatedVersionJson);
                    ResHotUpdateData versionInfo = JsonUtility.FromJson<ResHotUpdateData>(versionJson);
                    if (VersionData.JudgeVersionSize(versionInfo.version, hotUpdatedVersionInfo.version))
                    {
                        m_CurHotUpdateState = HotUpdateState.Updated;
                        Debug.Log("CurHotUpdateState：" + m_CurHotUpdateState + "，version：" + hotUpdatedVersionInfo.version);
                    }
                    else
                    {
                        m_CurHotUpdateState = HotUpdateState.Overrided;
                        Debug.Log("CurHotUpdateState：" + m_CurHotUpdateState + "，version：" + versionInfo.version);
                    }
                }
                return m_CurHotUpdateState;
            }
            set
            {
                m_CurHotUpdateState = value;
            }
        }
        /// <summary>
        /// 尝试热更
        /// </summary>
        public void TryHotUpdate()
        {
            CheckHotUpdate((isNeedHotUpdate, serverVersionInfo) =>
            {
                if (isNeedHotUpdate)
                {
                    Debug.Log("开始热更 当前版本：" + VersionData.GetLocalVersionInfo().version + "，目标版本：" + serverVersionInfo.version);
                    DownAssets();
                    ReplaceLocalAssets();
                    //修改本地版本号
                    VersionData.SetLocalVersion(serverVersionInfo);
                    Debug.Log("修改本地版本号 热更结束");
                }
                else
                {
                    Debug.Log("无需热更 当前版本：" + VersionData.GetLocalVersionInfo().version + "，目标版本：" + serverVersionInfo.version);
                }
                Debug.Log("GoOn");
            });
        }
        /// <summary>
        /// 检查是否需要热更通过版本号判断
        /// </summary>
        /// <param name="callback">回调 p1是否需要热更 p2最新热更版本号</param>
        /// <returns></returns>
        public void CheckHotUpdate(Action<bool, ResHotUpdateData> callback)
        {
            VersionData.GetServiceVersion((ResHotUpdateData serviceVer) =>
            {
                bool needHotUpdate = VersionData.JudgeVersionSize(VersionData.GetLocalVersionInfo().version, serviceVer.version); ;
                callback?.Invoke(needHotUpdate, serviceVer);
            });
        }
        private void DownAssets()
        {
            Debug.Log("下载资源");
        }
        private void ReplaceLocalAssets()
        {
            Debug.Log("替换本地资源");
        }

        /// <summary>
        /// 写入资源热更前json文件数据(StreamingAssets目录)
        /// </summary>
        /// <param name="resHotUpdateData"></param>
        public static void WriteResHotUpdateData(ResHotUpdateData resHotUpdateData = null)
        {
            if (resHotUpdateData == null)
            {
                resHotUpdateData = new ResHotUpdateData();
                resHotUpdateData.version = "1.2.0";//Test
#if UNITY_EDITOR
                resHotUpdateData.assetBundleNames = AssetDatabaseTool.GetAllAssetBundleNames();
#endif
            }
            string versionJson = JsonUtility.ToJson(resHotUpdateData, true);
            new FileIOTxt(HotUpdateSetting.localVersionRootPath, HotUpdateSetting.hotUpdateVersionFileName).Write(versionJson);
        }
    }

    /// <summary>
    /// 热更新状态
    /// </summary>
    public enum HotUpdateState
    {
        /// <summary>
        /// 从未热更过
        /// </summary>
        NeverUpdate,
        /// <summary>
        /// 已经热更过
        /// </summary>
        Updated,
        /// <summary>
        /// 覆盖安装过(未热更过)
        /// </summary>
        Overrided,
    }
    /// <summary>
    /// 资源热更数据结构
    /// </summary>
    [Serializable]
    public class ResHotUpdateData
    {
        /// <summary>
        /// 版本号 格式x.x.x
        /// </summary>
        public string version;
        public string[] assetBundleNames;
    }
}