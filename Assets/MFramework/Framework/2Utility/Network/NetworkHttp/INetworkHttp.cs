using System;
using System.Collections.Generic;
namespace MFramework
{
    /// <summary>
    /// 标题：网络通信HTTP协议接口
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2022.12.10
    /// 版本：1.0
    /// </summary>
    public interface INetworkHttp 
    {
        #region 请求对外封装
        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="url">接口URL地址</param>
        /// <param name="dataParaDic">请求参数集合</param>
        /// <param name="callBack">回调函数，收到服务器消息执行，服务器传递json参数</param>
        /// <param name="dicHeader">Authorization授权参数 验证是否拥有从服务器访问所需数据的权限 (Token)</param>
        /// <param name="bodyRaw">Body-raw参数 一般传入json数据</param>
        /// <param name="reqErrorCallback">发送请求失败回调，参数：错误信息，请求失败啊的接口地址</param>
        public void SendRequest(RequestType requestType, string url, Dictionary<string, string> dataParaDic, Action<string> callBack = null, Dictionary<string, string> dicHeader = null, string bodyRaw = "", Action<string, string> reqErrorCallback = null);
        #endregion

    }
    /// <summary>
    /// 请求方式
    /// </summary>
    public enum RequestType
    {
        Post,
        Get
    }
}
