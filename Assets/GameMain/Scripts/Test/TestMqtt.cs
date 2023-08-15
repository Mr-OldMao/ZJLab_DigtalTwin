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


public class TestMqtt : MonoBehaviour {
    private MqttClient client;
    public string ip = "127.0.0.1";//服务器IP
    public int port = 1883;//服务器端口
    public string username = "admin";//服务器端账号
    public string password = "g7020176";//服务器端密码
    private string lastReceivedMessage = ""; // 上次获取的消息
    public string clientId = "Unity Mqtt Client";//客户端ID
    public string topic1 = "Robot1_command"; // 替换为你要订阅的主题名称

    public GameObject roleobject;
    public Transform roletransform;
    public MainRec mainrecscript;
    //初始化MqttClient实例，连接，订阅
    void Start() {


        mainrecscript = this.GetComponent<MainRec>();
        //创建客户端实例
        client = new MqttClient(IPAddress.Parse(ip), port, false, null);
        // register to message received 
        client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

        
        ConnectToMQTT();
        Subscribenewtopic(topic1);


        // string message = "test,robot,mqtt"; // 替换为你要发布的主题内容
        // PublishMessage(topic1,message);
        
        
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
  


        // if (!client.IsConnected)//保持连接
        // {
        //     ConnectToMQTT(); 
        // }

        client.Subscribe(new string[] { topic1 }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

       

    }

    void CheckAndUpdateMessage(string newMessage) {
        if (newMessage != lastReceivedMessage) {



            // 获取最新消息的逻辑操作
            Debug.Log("Getting the latest message: " + newMessage);

            // 查找指令中role对应的游戏对象
            string[] messagepart = newMessage.Split(" ");
            string Role = messagepart[0];
            //GameObject roletransform =GameObject.Find(Role);之后初始化时，先获取所有存在的role对象，再根据指令的role，传递指令参数


            if (mainrecscript != null) {
                // 设置 MainRec 脚本的 current_command 属性
                mainrecscript.current_command = newMessage;
            } else {
                Debug.LogWarning("MainRec script component not found on "+Role);
            }
   

     

            lastReceivedMessage = newMessage; // 更新上次获取的消息
            
            
        }
    }

}
