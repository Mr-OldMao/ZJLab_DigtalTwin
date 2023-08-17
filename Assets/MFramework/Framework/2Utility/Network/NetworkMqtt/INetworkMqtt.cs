using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework
{
    /// <summary>
    /// 标题：Mqtt网络通信协议接口
    /// 功能：通信连接，订阅主题，发布主题消息
    /// 作者：毛俊峰
    /// 时间：2023.08.17
    /// </summary>
    public interface INetworkMqtt
    {
        void Connect(string clientId);
        void Connect(string clientId, string username, string password);

        void Subscribe(params string[] topics);
        void Subscribe(string[] topics, byte[] qosLevels);
        void Unsubscribe(params string[] topics);

        void Publish(string topic, string msg);
        void Publish(string topic, byte[] msg);
    }
}
