using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MFramework
{
    /// <summary>
    /// 标题：测试热更新
    /// 功能：初始化版本文件、检测更新、尝试更新
    /// 作者：毛俊峰
    /// 时间：2022.10.09
    /// 版本：1.0
    /// </summary>
    public class TestHotUpdate : MonoBehaviour
    {
        public Button btnInitWriteVersionInfo;
        public Button btnCheckHotUpdate;
        public Button btnTryHotUpdate;

        private void Start()
        {
            btnInitWriteVersionInfo.onClick.AddListener(() =>
            {
                try
                {
                    File.Delete(HotUpdateSetting.localVersionRootPath + "/" + HotUpdateSetting.hotUpdateVersionFileName);
                    File.Delete(HotUpdateSetting.hotUpdatedLocalVersionRootPath + "/" + HotUpdateSetting.hotUpdateVersionFileName);
                }
                catch (System.Exception)
                {

                }
                //todo 写入测试数据 未热更的本地版本信息
                Debug.Log("初始化写入测试数据 未热更的本地版本信息 localVersionRootPath：" + HotUpdateSetting.localVersionRootPath + " localVersionFileName：" + HotUpdateSetting.hotUpdateVersionFileName);
                HotUpdateManager.WriteResHotUpdateData();
            });
            btnCheckHotUpdate.onClick.AddListener(() =>
            {
                HotUpdateManager.GetInstance.CheckHotUpdate((isNeedHotUpdate, newVersion) =>
                {
                    if (isNeedHotUpdate)
                    {
                        Debug.Log("需要热更 curVersion：" + VersionData.GetLocalVersionInfo().version + ",newVersion：" + newVersion.version + ",state：" + HotUpdateManager.CurHotUpdateState);
                    }
                    else
                    {
                        Debug.Log("不需要热更 curVersion：" + VersionData.GetLocalVersionInfo().version + ",newVersion：" + newVersion.version + ",state：" + HotUpdateManager.CurHotUpdateState);
                    }
                });
            });
            btnTryHotUpdate.onClick.AddListener(() => { HotUpdateManager.GetInstance.TryHotUpdate(); });
        }
    }
}