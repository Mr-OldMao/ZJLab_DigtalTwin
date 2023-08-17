using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework
{
    /// <summary>
    /// 标题：Mqtt网络通信协议配置表
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2023.08.17
    /// </summary>
    public class MqttConfig
    {
        public string clientID = "ClientIDTest";
        public string userName = "UserName";
        public string password = "PassWord";
        public string clientIP = "127.0.0.1";
        public int clientPort = 1883;
        /*
        1883 MQTT 协议端口
        8883 MQTT/SSL 端口
        8083 MQTT/WebSocket 端口
        8080 HTTP API 端口
        18083 Dashboard 管理控制台端口
        */
    }

    /// <summary>
    /// 主题名称枚举
    /// </summary>
    public enum MqttTopicName
    {
        TopicTest,
    }
}
