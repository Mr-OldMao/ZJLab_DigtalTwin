using System.Collections.Generic;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt;
using System.Text;
using System.Net;

namespace MFramework
{
    public delegate void MqttRecvMsgCallback(string topic, string msg);
    public delegate void ConnSucCallback();
    /// <summary>
    /// 标题：Mqtt网络通信协议
    /// 功能：封装Mqtt常用功能，与代理连接、断开，发布消息，订阅主题，收发消息事件回调
    /// 作者：毛俊峰
    /// 时间：2023.08.17
    /// </summary>
    public abstract class AbNetworkMqtt<T> : SingletonByMono<T>, INetworkMqtt where T : Component
    {
        internal MqttClient mqttClient = null;
        /// <summary>
        /// 已经订阅的主题
        /// </summary>
        public List<string> listSubscribedTopics;

        /// <summary>
        /// 是否为webgl平台，web包真机打包需要为true，测试阶段可以选择false
        /// </summary>
        public bool IsWebgl
        {
            get; set;
        } = false;

        public virtual void Init(string clientIP, int clientPort, string clientId, string username, string password)
        {
            if (!IsWebgl)
            {
                mqttClient = new MqttClient(IPAddress.Parse(clientIP), clientPort, false, null);
            }
            listSubscribedTopics = new List<string>();
            Connect(clientIP, clientPort, clientId, username, password);
        }

        #region 与代理连接

        public void Connect(string clientIP, int clientPort, string clientId)
        {
            Connect(clientIP, clientPort, clientId, null, null);
        }

        /// <summary>
        /// 连接代理
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void Connect(string clientIP, int clientPort, string clientId, string username, string password)
        {
            if (IsWebgl)
            {
                MqttWebglCenter.GetInstance.Connect(clientIP, clientPort, clientId, username, password);
            }
            else
            {
                if (mqttClient == null)
                {
                    return;
                }
                if (mqttClient.IsConnected)
                {
                    return;
                }
                mqttClient.Connect(clientId, username, password);
                ConnSucCallbackHandle?.Invoke();
            }
        }

        public virtual void DisConnect()
        {
            mqttClient.Disconnect();
            mqttClient = null;
        }
        #endregion

        #region 发布消息
        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        public void Publish(MqttTopicName topic, string msg)
        {
            Publish(topic.ToString(), msg);
        }
        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        public void Publish(string topic, string msg)
        {
            Publish(topic, Encoding.UTF8.GetBytes(msg));
        }
        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        public void Publish(string topic, byte[] msg)
        {
            if (string.IsNullOrEmpty(topic))
            {
                Debug.LogError("MQTT topic is null");
            }
            string msgStr = Encoding.UTF8.GetString(msg);
            if (IsWebgl)
            {
                MqttWebglCenter.GetInstance.Publish(topic, msgStr);
            }
            else
            {
                if (mqttClient == null)
                {
                    Debug.LogError("MQTT mqttClient is null");
                    return;
                }
                mqttClient.Publish(topic, msg);
            }
        }
        #endregion

        #region 订阅主题
        /// <summary>
        /// 订阅、监听指定主题
        /// </summary>
        /// <param name="topics"></param>
        public void Subscribe(params MqttTopicName[] topics)
        {
            foreach (MqttTopicName topic in topics)
            {
                Subscribe(topic.ToString());
            }
        }

        /// <summary>
        /// 订阅、监听指定主题
        /// </summary>
        /// <param name="topics"></param>
        public void Subscribe(params string[] topics)
        {
            Subscribe(topics, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }

        /// <summary>
        /// 订阅、监听指定主题
        /// </summary>
        /// <param name="topics"></param>
        /// <param name="qosLevels"></param>
        public void Subscribe(string[] topics, byte[] qosLevels)
        {
            if (topics != null && topics.Length == 0)
            {
                Debug.LogError("MQTT topics is null");
                return;
            }
            foreach (string topic in topics)
            {
                if (!listSubscribedTopics.Contains(topic))
                {
                    if (IsWebgl)
                    {
                        MqttWebglCenter.GetInstance.Subscribe(new string[] { topic });
                    }
                    else
                    {
                        if (mqttClient == null)
                        {
                            Debug.LogError("MQTT mqttClient is null");
                            return;
                        }
                        mqttClient.Subscribe(new string[] { topic }, qosLevels);
                    }
                    listSubscribedTopics.Add(topic);
                }
                else
                {
                    Debug.LogError("MQTT topic is exist : topic：" + topic);
                }
            }
        }

        /// <summary>
        /// 取消订阅监听
        /// </summary>
        /// <param name="topics"></param>
        public void Unsubscribe(params string[] topics)
        {
            if (mqttClient == null)
            {
                Debug.LogError("MQTT mqttClient is null");
                return;
            }
            foreach (var topic in topics)
            {
                if (!listSubscribedTopics.Contains(topic))
                {
                    if (IsWebgl)
                    {
                        MqttWebglCenter.GetInstance.Unsubscribe(new string[] { topic });
                    }
                    else
                    {
                        mqttClient.Unsubscribe(new string[] { topic });
                    }
                    listSubscribedTopics.Remove(topic);
                }
                else
                {
                    Debug.LogError("MQTT topic is dont Exist : topic：" + topic);
                }
            }
        }

        /// <summary>
        /// 获取当前订阅监听的主题
        /// </summary>
        /// <returns></returns>
        public List<string> GetCurSubscribe()
        {
            return listSubscribedTopics;
        }

        public ConnSucCallback ConnSucCallbackHandle;
        public void AddConnectedSucEvent(ConnSucCallback connSucCallback)
        {
            ConnSucCallbackHandle += connSucCallback;
        }
        #endregion


        #region 事件回调注册、注销

        private MqttRecvMsgCallback m_RecvMsgCallback;

        /// <summary>
        /// 注册监听 收到消息回调
        /// </summary>
        /// <param name="handle"></param>
        public void AddListenerSubscribe(MqttRecvMsgCallback mqttRecvMsgCallback)
        {
            if (IsWebgl)
            {
                MqttWebglCenter.GetInstance.AddListenerSubscribe(mqttRecvMsgCallback);
            }
            else
            {
                m_RecvMsgCallback += mqttRecvMsgCallback;
                mqttClient.MqttMsgPublishReceived += (object sender, MqttMsgPublishEventArgs e) =>
                {
                    //Debug.Log("mqtt recv, topic:" + e.Topic + ",msg:" + Encoding.UTF8.GetString(e.Message));
                    m_RecvMsgCallback?.Invoke(e.Topic, Encoding.UTF8.GetString(e.Message));
                };
            }
        }

        /// <summary>
        /// 注册监听 收到消息回调
        /// </summary>
        /// <param name="handle"></param>
        public void RemoveListenerSubscribe(MqttRecvMsgCallback mqttRecvMsgCallback)
        {
            if (IsWebgl)
            {
                MqttWebglCenter.GetInstance.RemoveListenerSubscribe(mqttRecvMsgCallback);
            }
            else
            {
                m_RecvMsgCallback -= mqttRecvMsgCallback;
            }
        }

        ///// <summary>
        ///// 注册监听 收到消息回调
        ///// </summary>
        ///// <param name="handle"></param>
        //public void AddListener(MqttMsgPublishEventHandler handle)
        //{
        //    mqttClient.MqttMsgPublishReceived += handle;
        //}



        ///// <summary>
        ///// 注销收到消息回调
        ///// </summary>
        //public void RemoveListener(MqttMsgPublishEventHandler handle)
        //{
        //    mqttClient.MqttMsgPublishReceived -= handle;
        //}

        ///// <summary>
        ///// 注册监听 客户端订阅消息成功回调
        ///// </summary>
        ///// <param name="handle"></param>
        //public void AddListener(MqttMsgSubscribedEventHandler handle)
        //{
        //    mqttClient.MqttMsgSubscribed += handle;
        //}


        ///// <summary>
        ///// 注销客户端订阅消息成功监听回调
        ///// </summary>
        ///// <param name="handle"></param>
        //public void RemoveListener(MqttMsgSubscribedEventHandler handle)
        //{
        //    mqttClient.MqttMsgSubscribed -= handle;
        //}

        //public void AddListener(MqttMsgPublishedEventHandler handle)
        //{
        //    mqttClient.MqttMsgPublished += handle;
        //}
        //public void RemoveListener(MqttMsgPublishedEventHandler handle)
        //{
        //    mqttClient.MqttMsgPublished -= handle;
        //}
        //public void AddListener(MqttMsgUnsubscribedEventHandler handle)
        //{
        //    mqttClient.MqttMsgUnsubscribed += handle;
        //}
        //public void RemoveListener(MqttMsgUnsubscribedEventHandler handle)
        //{
        //    mqttClient.MqttMsgUnsubscribed -= handle;
        //}
        //public void AddListener(MqttMsgDisconnectEventHandler handle)
        //{
        //    mqttClient.MqttMsgDisconnected += handle;
        //}
        //public void RemoveListener(MqttMsgDisconnectEventHandler handle)
        //{
        //    mqttClient.MqttMsgDisconnected -= handle;
        //}
        #endregion
    }
}
