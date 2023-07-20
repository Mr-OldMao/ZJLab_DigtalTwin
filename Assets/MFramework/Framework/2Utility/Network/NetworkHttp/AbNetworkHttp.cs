using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace MFramework
{
    /// <summary>
    /// 标题：网络通信HTTP协议抽象类
    /// 功能：具体实现发送Http请求
    /// 作者：毛俊峰
    /// 时间：2022.12.10
    /// 版本：1.0
    /// </summary>
    public abstract class AbNetworkHttp<T> : SingletonByMono<T>, INetworkHttp where T : Component
    {
        //请求集合容器
        private Dictionary<int, Coroutine> m_DicRequestContainer = new Dictionary<int, Coroutine>();
        //当前请求的索引号
        private int m_CurRequestIndex = 0;

        /// <summary>
        /// 请求成功回调参数
        /// </summary>
        private class HttpRequestSucceedParam
        {
            public RequestType RequestType;
            public string url;
            public string token;
        }
        /// <summary>
        /// 请求失败回调参数
        /// </summary>
        private class HttpRequestFailParam
        {
            public RequestType RequestType;
            public string url;
            public string token;
            public string errorDescript;
        }
        private void Awake()
        {
            RegisterMsgEvent();
        }
        private void OnDestroy()
        {
            UnregisterMsgEvent();
        }

        #region 请求对外封装
        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="requestType">请求方式</param>
        /// <param name="url">接口URL地址</param>
        /// <param name="dataParaDic">请求参数集合</param>
        /// <param name="callBack">回调函数，收到服务器消息执行，服务器传递json参数</param>
        /// <param name="dicHeader">Authorization授权参数 验证是否拥有从服务器访问所需数据的权限 (Token)</param>
        /// <param name="bodyRaw">Body-raw参数 一般传入json数据</param>
        /// <param name="reqErrorCallback">发送请求失败回调，参数：错误信息，请求失败啊的接口地址</param>
        public virtual void SendRequest(RequestType requestType, string url, Dictionary<string, string> dataParaDic, Action<string> callBack = null, Dictionary<string, string> dicHeader = null, string bodyRaw = "", Action<string, string> reqErrorCallback = null)
        {
            Coroutine curRequest = null;
            switch (requestType)
            {
                case RequestType.Post:
                    curRequest = StartCoroutine(RequestPost(m_CurRequestIndex, url, dataParaDic, callBack, dicHeader, bodyRaw, reqErrorCallback));
                    m_DicRequestContainer.Add(m_CurRequestIndex, curRequest);
                    break;
                case RequestType.Get:
                    curRequest = StartCoroutine(RequestGet(m_CurRequestIndex, url, dataParaDic, callBack, dicHeader, bodyRaw, reqErrorCallback));
                    m_DicRequestContainer.Add(m_CurRequestIndex, curRequest);
                    break;
            }
            m_CurRequestIndex++;
        }
        #endregion

        #region UnityWebRequest实现HTTP请求
        /// <summary>
        /// Post请求具体实现
        /// </summary>
        /// <param name="requestID">当前请求唯一标识</param>
        /// <param name="url">接口URL地址</param>
        /// <param name="paramDataDic">请求参数集合</param>
        /// <param name="reqSucCallback">发送请求成功回调，参数传入服务器下发的Json作为形参</param>
        /// <param name="dicHeader">Authorization授权参数 验证是否拥有从服务器访问所需数据的权限</param>
        /// <param name="bodyRaw">Body体-raw参数</param>
        /// <param name="reqErrorCallback">发送请求失败回调，参数：错误信息，请求失败啊的接口地址</param>
        /// <returns></returns>
        private IEnumerator RequestPost(int requestID, string url, Dictionary<string, string> paramDataDic, Action<string> reqSucCallback, Dictionary<string, string> dicHeader = null, string bodyRaw = "", Action<string, string> reqErrorCallback = null)
        {
            //所提交的表单
            WWWForm form = new WWWForm();
            //加上请求参数，如参数名“Content-Type",内容”application/json“
            //form.AddField("Content-Type", "application/json");
            foreach (var item in paramDataDic)
            {
                if (!string.IsNullOrEmpty(item.Key) && item.Value != null)
                {
                    form.AddField(item.Key, item.Value);
                }
            }
            string fullUrl = "";
            bool isFirstParam = true;
            foreach (var item in paramDataDic)
            {
                if (!string.IsNullOrEmpty(item.Key) && item.Value != null)
                {
                    if (isFirstParam)
                    {
                        fullUrl += item.Key + "=" + item.Value;
                        isFirstParam = !isFirstParam;
                    }
                    else
                    {
                        fullUrl += "&" + item.Key + "=" + item.Value;
                    }
                }
            }
            //发送Post请求
            using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
            {
                //Authorization-API Key  (验证是否拥有从服务器访问所需数据的权限)
                if (dicHeader != null)
                {
                    foreach (var item in dicHeader)
                    {
                        webRequest.SetRequestHeader(item.Key, item.Value);
                    }
                }
                //Body体-raw参数
                if (!string.IsNullOrEmpty(bodyRaw))
                {
                    byte[] bodyRawByte = System.Text.Encoding.UTF8.GetBytes(bodyRaw);
                    webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRawByte);
                }

                yield return webRequest.SendWebRequest();
                if (!string.IsNullOrEmpty(webRequest.error))
                {
                    //请求失败
                    MsgEvent.SendMsg(MsgEventName.HttpRequestFail, new HttpRequestFailParam() { RequestType = RequestType.Post, url = url, token = dicHeader?["Authorization"], errorDescript = webRequest.error });
                    reqErrorCallback?.Invoke(webRequest.error, url);
                }
                else
                {
                    //请求成功
                    string json = webRequest.downloadHandler.text;
                    MsgEvent.SendMsg(MsgEventName.HttpRequestSucceed, new HttpRequestSucceedParam() { RequestType = RequestType.Post, url = url, token = dicHeader?["Authorization"], });
                    reqSucCallback?.Invoke(json);
                    //移除请求集合
                    if (m_DicRequestContainer.ContainsKey(requestID))
                    {
                        m_DicRequestContainer.Remove(requestID);
                    }
                    else
                    {
                        Debugger.LogError("requestListPost is null   requestID：" + requestID + ",url:" + url);
                    }
                }
            }

        }

        /// <summary>
        /// Get请求具体实现
        /// </summary>
        /// <param name="requestID">当前请求唯一标识</param>
        /// <param name="url">URL路径</param>
        /// <param name="paramDataDic">参数</param>
        /// <param name="reqSucCallBack">发送请求成功回调，参数传入服务器下发的Json作为形参</param>    
        /// <param name="dicHeader">Authorization授权参数 验证是否拥有从服务器访问所需数据的权限</param>
        /// <param name="bodyRaw">Body体-raw参数</param>
        /// <param name="reqErrorCallback">发送请求失败回调，参数：错误信息，请求失败啊的接口地址</param>
        /// <returns></returns>
        private IEnumerator RequestGet(int requestID, string url, Dictionary<string, string> paramDataDic, Action<string> reqSucCallBack, Dictionary<string, string> dicHeader = null, string bodyRaw = "", Action<string, string> reqErrorCallback = null)
        {
            //URL与参数拼接
            if (paramDataDic != null && paramDataDic.Count != 0)
            {
                url += "?";
                bool isFirstParam = true;
                foreach (var item in paramDataDic)
                {
                    if (isFirstParam)
                    {
                        url += item.Key + "=" + item.Value;
                        isFirstParam = !isFirstParam;
                    }
                    else
                    {
                        url += "&" + item.Key + "=" + item.Value;
                    }
                }
            }
            //发送Get请求
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                //Authorization-API Key  (验证是否拥有从服务器访问所需数据的权限)
                if (dicHeader != null)
                {
                    foreach (var item in dicHeader)
                    {
                        webRequest.SetRequestHeader(item.Key, item.Value);
                    }
                }
                //Body体-raw参数
                if (!string.IsNullOrEmpty(bodyRaw))
                {
                    byte[] bodyRawByte = System.Text.Encoding.UTF8.GetBytes(bodyRaw);
                    webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRawByte);
                }

                yield return webRequest.SendWebRequest();
                if (!string.IsNullOrEmpty(webRequest.error))
                {
                    //请求失败
                    MsgEvent.SendMsg(MsgEventName.HttpRequestFail, new HttpRequestFailParam() { RequestType = RequestType.Get, url = url, token = dicHeader?["Authorization"], errorDescript = webRequest.error });
                    reqErrorCallback?.Invoke(webRequest.error, url);
                }
                else
                {
                    //请求成功
                    string json = webRequest.downloadHandler.text;
                    MsgEvent.SendMsg(MsgEventName.HttpRequestSucceed, new HttpRequestSucceedParam() { RequestType = RequestType.Get, url = url, token = dicHeader?["Authorization"], });
                    reqSucCallBack?.Invoke(json);
                    //移除请求集合
                    if (m_DicRequestContainer.ContainsKey(requestID))
                    {
                        m_DicRequestContainer.Remove(requestID);
                    }
                    else
                    {
                        Debug.LogError("requestListPost is null   requestID：" + requestID + ",url:" + url);
                    }
                }
            }
        }
        #endregion

        #region MsgEvent
        /// <summary>
        /// 注册请求结果事件
        /// </summary>
        private void RegisterMsgEvent()
        {
            MsgEvent.RegisterMsgEvent(MsgEventName.HttpRequestSucceed, (p) =>
            {
                if (p is HttpRequestSucceedParam param)
                {
                    Debugger.Log($"HTTPReqSuc URL：{param.url}，Type：{param.RequestType}，Token:{param.token}");
                }
            });
            MsgEvent.RegisterMsgEvent(MsgEventName.HttpRequestFail, (p) =>
            {
                if (p is HttpRequestFailParam param)
                {
                    Debugger.Log($"HTTPReqFail URL：{param.url}，ErrorDescript：{param.errorDescript}，Type：{param.RequestType}，Token:{param.token}");
                }
            });

        }
        /// <summary>
        /// 注销请求结果事件
        /// </summary>
        private void UnregisterMsgEvent()
        {
            MsgEvent.UnregisterMsgEvent(MsgEventName.HttpRequestSucceed);
            MsgEvent.UnregisterMsgEvent(MsgEventName.HttpRequestFail);
        }
        #endregion

    }
}