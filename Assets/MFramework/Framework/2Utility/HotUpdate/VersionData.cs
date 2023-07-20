using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：版本信息数据
    /// 功能：热更文件信息的读写
    /// 作者：毛俊峰
    /// 时间：2022.10.09
    /// 版本：1.0
    /// </summary>
    public class VersionData : MonoBehaviour
    {
        /// <summary>
        /// 获取最新(服务器)版本号
        /// </summary>
        /// <returns></returns>
        public static void GetServiceVersion(Action<ResHotUpdateData> callback)
        {
            //todo最新版本信息  需要通过服务器获取，当前未测试数据
            string serviceVersionJson = "{ \"version\" : \"1.2.3\"}";//end todo
            ResHotUpdateData resHotUpdateData = JsonUtility.FromJson<ResHotUpdateData>(serviceVersionJson);
            callback?.Invoke(resHotUpdateData);
        }

        /// <summary>
        /// 获取本地版本信息
        /// </summary>
        /// <returns></returns>
        public static ResHotUpdateData GetLocalVersionInfo()
        {
            string localVersionContent = string.Empty;
            switch (HotUpdateManager.CurHotUpdateState)
            {
                case HotUpdateState.NeverUpdate:
                    //localVersionContent = new FileIOTxt(localVersionRootPath, versionFileName).Read();    File类真机读取不到StreamingAssets中文件
                    localVersionContent = ReadStreamingAssetFileByPlatform(HotUpdateSetting.localVersionRootPath + "/" + HotUpdateSetting.hotUpdateVersionFileName);
                    break;
                case HotUpdateState.Updated:
                    localVersionContent = new FileIOTxt(HotUpdateSetting.hotUpdatedLocalVersionRootPath, HotUpdateSetting.hotUpdateVersionFileName).Read();
                    break;
                case HotUpdateState.Overrided:
                    localVersionContent = ReadStreamingAssetFileByPlatform(HotUpdateSetting.localVersionRootPath + "/" + HotUpdateSetting.hotUpdateVersionFileName);
                    break;
                default:
                    break;
            }
            ResHotUpdateData resHotUpdateData = JsonUtility.FromJson<ResHotUpdateData>(localVersionContent);
            return resHotUpdateData;
        }

        /// <summary>
        /// 设置(热更后)本地版本信息
        /// </summary>
        /// <param name="resHotUpdateData"></param>
        public static void SetLocalVersion(ResHotUpdateData resHotUpdateData)
        {
            string versionJson = JsonUtility.ToJson(resHotUpdateData);
            new FileIOTxt(HotUpdateSetting.hotUpdatedLocalVersionRootPath, HotUpdateSetting.hotUpdateVersionFileName).Write(versionJson);
        }

        /// <summary>
        /// 判定两个版本号高低
        /// 格式x.x.x  version1 < version2 返回true否则false
        /// </summary>
        /// <param name="version1"></param>
        /// <param name="version2"></param>
        public static bool JudgeVersionSize(string version1, string version2)
        {
            bool res = false;
            string[] version1Arr = version1.Split('.');
            string[] version2Arr = version2.Split('.');
            if (version2Arr.Length == 3 && version1Arr.Length == 3)
            {
                try
                {
                    if (int.Parse(version1Arr[0]) < int.Parse(version2Arr[0]) || int.Parse(version1Arr[1]) < int.Parse(version2Arr[1]) || int.Parse(version1Arr[2]) < int.Parse(version2Arr[2]))
                    {
                        res = true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            else
            {
                Debug.LogError("版本号格式错误 version1：" + version1 + "，version2：" + version2);
            }
            return res;
        }

        #region todo 临时读取StreamingAssets文件夹文件根据不同平台，后期需要封装
        /// <summary>
        /// 读取StreamingAssets文件夹文件内容根据不同平台
        /// </summary>
        /// <param name="filePath">StreamingAsset文件全路径 eg：Application.streamingAssetsPath + "/Josn/modelname.json</param>
        private static string ReadStreamingAssetFileByPlatform(string filePath)
        {
            string path =
#if UNITY_ANDROID && !UNITY_EDITOR
                    filePath;
#elif UNITY_IPHONE && !UNITY_EDITOR
                    "file://" + filePath;
#elif UNITY_STANDLONE_WIN || UNITY_EDITOR
                    "file://" + filePath;
#else
                    string.Empty;
#endif
            UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(path);
            request.SendWebRequest();//读取数据
            if (request.error == null)
            {
                while (true)
                {
                    if (request.downloadHandler.isDone)//是否读取完数据
                    {
                        //Debug.Log(request.downloadHandler.text);
                        return request.downloadHandler.text;
                    }
                }
            }
            else
            {
                return null;
            }
        }
        #endregion
    }


}