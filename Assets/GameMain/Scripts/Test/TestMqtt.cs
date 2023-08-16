/// <summary>
/// 标题：测试mqtt协议接收
/// 功能：
/// 作者：弓利剑
/// 时间：20230731
/// /// </summary>


using UnityEngine;
using System.Collections;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using System;
using System.Text;


public class TestMqtt : MonoBehaviour {
    private MqttClient client;
    public string ip = "127.0.0.1";//服务器IP
    public int port = 1883;//服务器端口
    public string username = "admin";//服务器端账号
    public string password = "g7020176";//服务器端密码
    private string lastReceivedMessage = ""; // 上次获取的消息
    public string clientId = "Unity Mqtt Client";//客户端ID
    public string topic1 = "Robot1_command"; // 替换为你要订阅的主题名称

    private MqttInstance mqttInstance = new MqttInstance();
    //初始化MqttClient实例，连接，订阅
    void Start() {



        
        mqttInstance.Initialize(ip, port);//初始化客户端实例

        // 连接，订阅，发布测试
        mqttInstance.Connect(clientId,username,password );
        mqttInstance.Subscribe(topic1);
        //mqttInstance.Publish(topic1, "Hello, MQTT!");
        mqttInstance.client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

        
    }

    void Update() {


    }


    void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e) {
        //Debug.Log("Received: " + System.Text.Encoding.UTF8.GetString(e.Message));
        CheckAndUpdateMessage(System.Text.Encoding.UTF8.GetString(e.Message));
    }


    void CheckAndUpdateMessage(string newMessage) {
        if (newMessage != lastReceivedMessage) {

            // 获取最新消息的逻辑操作
            Debug.Log("Getting the latest message: " + newMessage);


            lastReceivedMessage = newMessage; // 更新上次获取的消息
            
            
        }
    }

}
