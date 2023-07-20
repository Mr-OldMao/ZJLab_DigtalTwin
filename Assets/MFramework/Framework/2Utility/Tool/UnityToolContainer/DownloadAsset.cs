using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace MFramework
{
    /// <summary>
    /// 标题：Unity工具类
    /// 功能：根据URL下载资源
    /// 作者：毛俊峰
    /// 时间：2022.04.22
    /// 版本：1.0
    /// </summary>
    public class DownloadAsset : SingletonByMono<DownloadAsset>
    {
        /// <summary>
        /// 通过URL链接下载资源，下载完毕回调出去
        /// T：为所要下载的资源类型，目前支持的类型有 Texture2D,DownloadHandlerAudioClip,string,sprite,AudioClip(需要指定音频类型)
        /// </summary>
        /// <param name="url">资源的下载链接 本地URL(file://xxxx)或者外网URL(http://xxxxx)</param>
        /// <param name="downSucCallback">回调 执行时机：成功下载完毕后，参数：下载的资源</param>
        /// <param name="downFailCallback">回调 执行时机：下载失败回调</param>
        /// <returns></returns>
        public void DownLoadAssetsByURL<T>(string url, Action<T> downSucCallback, Action downFailCallback = null) where T : class
        {
            StartCoroutine(DownloadByURL<T>(url, downSucCallback, downFailCallback));
        }

        private IEnumerator DownloadByURL<T>(string url, Action<T> downSucCallback, Action downFailCallback = null) where T : class
        {
            using UnityWebRequest request = UnityWebRequest.Get(url);
            var type = typeof(T);
            #region 判定T 资源类型
            //图片文件
            if (type == typeof(Texture2D) || type == typeof(Sprite))
            {
                request.downloadHandler = new DownloadHandlerTexture(true);
            }
            //音频文件
            else if (type == typeof(DownloadHandlerAudioClip) || type == typeof(AudioClip))
            {
                //Debug.Log("解析URL音频资源 默认音频类型为WAV，如需更改需在此设置");
                //request.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);
                request.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.WAV);
            }
            //本地文本文件
            else if (type == typeof(string))
            {

            }

            #endregion
            //读取资源
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ProtocolError
                || request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(request.error + "检查 URL " + url);
                downFailCallback?.Invoke();
            }
            else
            {
                if (request.isDone)
                {  //资源实体 回调参数 
                    T changeType = null;
                    #region 获取资源实体
                    //判定T 资源类型
                    if (type == typeof(Texture2D) || type == typeof(Sprite))//图片资源
                    {
                        //获取资源
                        DownloadHandlerTexture downloadHandlerTexture = (DownloadHandlerTexture)request.downloadHandler;
                        if (type == typeof(Texture2D))
                        {
                            //强转为资源类型
                            changeType = downloadHandlerTexture.texture as T;
                        }
                        else if (type == typeof(Sprite))
                        {
                            Sprite sprite = Sprite.Create(downloadHandlerTexture.texture, new Rect(0, 0, downloadHandlerTexture.texture.width, downloadHandlerTexture.texture.height), new Vector2(0.5f, 0.5f));
                            changeType = sprite as T;
                        }

                    }
                    else if (type == typeof(DownloadHandlerAudioClip) || type == typeof(AudioClip))//音频资源
                    {
                        DownloadHandlerAudioClip audioClip = (DownloadHandlerAudioClip)request.downloadHandler;
                        if (type == typeof(DownloadHandlerAudioClip))
                        {
                            changeType = audioClip as T;
                        }
                        else if (type == typeof(AudioClip))
                        {
                            changeType = audioClip.audioClip as T;
                        }
                    }
                    else if (type == typeof(string))//文本资源
                    {
                        string txt = request.downloadHandler.text;
                        changeType = txt as T;
                    }
                    #endregion
                    //卸载未使用的资源
                    Resources.UnloadUnusedAssets();
                    //加载成功 回调 传出资源
                    downSucCallback?.Invoke(changeType);
                }
            }
        }
    }

    /// <summary>
    /// UnityTool工具类扩展-根据URL下载资源
    /// </summary>
    public partial class UnityTool
    {
        /// <summary>
        /// 通过URL链接下载资源，下载完毕回调出去
        /// T：为所要下载的资源类型，目前支持的类型有 Texture2D,DownloadHandlerAudioClip,string,sprite,AudioClip(需要指定音频类型)
        /// </summary>
        /// <param name="url">资源的下载链接 本地URL(file://xxxx)或者外网URL(http://xxxxx)</param>
        /// <param name="downSucCallback">回调 执行时机：成功下载完毕后，参数：下载的资源</param>
        /// <param name="downFailCallback">回调 执行时机：下载失败回调</param>
        /// <returns></returns>
        public void DownLoadAssetsByURL<T>(string url, Action<T> downSucCallback, Action downFailCallback = null) where T : class
        {
            DownloadAsset.GetInstance.DownLoadAssetsByURL<T>(url, downSucCallback, downFailCallback);
        }
    }

}