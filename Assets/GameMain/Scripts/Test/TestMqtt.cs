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
    public string ip = "127.0.0.1";//服务器IP
    public int port = 8083;//服务器端口
    public string username = "admin";//服务器端账号
    public string password = "g7020176";//服务器端密码
    private string lastReceivedMessage = ""; // 上次获取的消息
    public string clientId = "Unity Mqtt Client";//客户端ID

    private string topic1 = "robot1"; // 替换为你要订阅的主题名称

    //初始化MqttClient实例，连接，订阅
    void Start() {
        // create client instance 
        client = new MqttClient(IPAddress.Parse(ip), port, false, null);
        // register to message received 
        client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

        
        //clientId = Guid.NewGuid().ToString();
        ConnectToMQTT();
       
        
        Subscribenewtopic(topic1);
        string message = "test,robot,mqtt"; // 替换为你要订阅的主题名称
        PublishMessage(topic1,message);
    


        
    }

    void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e) {
        //Debug.Log("Received: " + System.Text.Encoding.UTF8.GetString(e.Message));
        CheckAndUpdateMessage(System.Text.Encoding.UTF8.GetString(e.Message));
    }

    void ConnectToMQTT(){//连接到mqtt客户端
        try
        {
            if (!client.IsConnected)
            {
                client.Connect(clientId, username, password);
                Debug.Log("Connected to MQTT Broker!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("MQTT Connection Error: " + e.Message);
        }
    }

    void Subscribenewtopic(string topic){//订阅新主题
        client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
    }

    void PublishMessage(string topic, string message) {
        byte[] payload = System.Text.Encoding.UTF8.GetBytes(message);
        client.Publish(topic, payload, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
    }


    void Update() {
  

        if (!client.IsConnected)//保持连接
        {
            ConnectToMQTT(); 
        }

        client.Subscribe(new string[] { topic1 }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

       

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
