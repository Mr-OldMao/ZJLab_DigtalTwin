using System.Collections.Generic;
using System;

namespace MFramework
{
    /// <summary>
    /// 标题：网络通信基于Http应用层协议(高层协议)
    /// 功能：发送HTTP请求，获取请求结果
    /// 作者：毛俊峰
    /// 时间：2022.12.10
    /// </summary>
    public class NetworkHttp : AbNetworkHttp<NetworkHttp>
    {
        /// <summary>
        /// 是否自动填充Token
        /// </summary>
        private bool m_CanAutoFillToken = true;
        /// <summary>
        /// 是否打印请求回调日志
        /// </summary>
        private bool m_CanPrintCallbackLog = true;

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
        public override void SendRequest(RequestType requestType, string url, Dictionary<string, string> dataParaDic, Action<string> callBack = null, Dictionary<string, string> dicHeader = null, string bodyRaw = "", Action<string, string> reqErrorCallback = null)
        {
            if (m_CanAutoFillToken && dicHeader == null)
            {
                dicHeader = new Dictionary<string, string> { { "Authorization", "TokenValue" } };
            }
            if (m_CanPrintCallbackLog)
            {
                callBack += (json) => Debugger.Log($"Http Callback Json：{json}");
            }
            base.SendRequest(requestType, url, dataParaDic, callBack, dicHeader, bodyRaw, reqErrorCallback);
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="requestType">请求方式</param>
        /// <param name="url">接口URL地址</param>
        /// <param name="dataParaDic">请求参数集合</param>
        /// <param name="callBack">回调函数，收到服务器消息执行，服务器传递json参数</param>
        public void SendRequest(RequestType requestType, string url, Dictionary<string, string> dataParaDic, Action<string> callBack = null)
        {
            SendRequest(requestType, url, dataParaDic, callBack, null, null);
        }
    }
}
