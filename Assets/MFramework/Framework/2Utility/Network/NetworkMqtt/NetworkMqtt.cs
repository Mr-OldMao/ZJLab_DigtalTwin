using UnityEngine;

namespace MFramework
{
    /// <summary>
    /// 标题：Mqtt网络通信协议对外接口
    /// 功能：初始化，与代理连接、断开，发布消息，订阅主题，收发消息事件回调
    /// 作者：毛俊峰
    /// 时间：2023.08.17
    /// </summary>
    public class NetworkMqtt : AbNetworkMqtt<NetworkMqtt>
    {
        private bool m_IsInited = false;
        public NetworkMqtt Init(string clientIP, int clientPort, string clientId)
        {
            return Init(clientIP, clientPort, clientId, null, null);
        }

        public NetworkMqtt Init(MqttConfig mqttConfig)
        {
            return Init(mqttConfig.clientIP, mqttConfig.clientPort, mqttConfig.clientID, mqttConfig.userName, mqttConfig.password);
        }

        public new NetworkMqtt Init(string clientIP, int clientPort, string clientId, string username, string password)
        {
            if (m_IsInited)
            {
                return this;
            }
            Debug.Log("Init Mqtt , IsWebgl:" + IsWebgl);
            if (IsWebgl)
            {
                MqttWebglCenter.GetInstance.Connect(clientIP, clientPort, clientId, username, password);
            }
            else
            {
                base.Init(clientIP, clientPort, clientId, username, password);
            }
            m_IsInited = true;
            return this;
        }

        /// <summary>
        /// 断开代理连接
        /// </summary>
        public override void DisConnect()
        {
            if (IsWebgl)
            {
                MqttWebglCenter.GetInstance.DisConnect();
            }
            else
            {
                base.DisConnect();
            }
            m_IsInited = false;
        }
    }
}
