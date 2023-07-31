using UnityEngine;
using System.Collections;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;

using System;
/// <summary>
/// 标题：测试mqtt协议接收
/// 功能：
/// 作者：弓利剑
/// 时间：20230731
/// </summary>
public class TestMqtt : MonoBehaviour {
    private MqttClient client;
    public string ip = "127.0.0.1";
    public int port = 1883;
    public string username = "admin";
    public string password = "g7020176";
    private string lastReceivedMessage = ""; // 上次获取的消息

    // Use this for initialization
    void Start() {
        // create client instance 
        client = new MqttClient(IPAddress.Parse(ip), port, false, null);
        // register to message received 
        client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
        string clientId = Guid.NewGuid().ToString();

        client.Connect(clientId, username, password);

        // subscribe to the topic with QoS 2 
        
    }

    void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e) {
        //Debug.Log("Received: " + System.Text.Encoding.UTF8.GetString(e.Message));
        CheckAndUpdateMessage(System.Text.Encoding.UTF8.GetString(e.Message));
    }

    void Update() {
        client.Subscribe(new string[] { "robot_command" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
    }

    void CheckAndUpdateMessage(string newMessage) {
        if (newMessage != lastReceivedMessage) {
            // 获取最新消息的逻辑操作
            Debug.Log("Getting the latest message: " + newMessage);
            // ...
            lastReceivedMessage = newMessage; // 更新上次获取的消息
        }
    }
}
