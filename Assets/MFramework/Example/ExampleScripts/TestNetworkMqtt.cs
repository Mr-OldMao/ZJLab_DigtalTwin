using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;

namespace MFramework
{
    /// <summary>
    /// 标题：测试Mqtt网络通信协议
    /// 功能：测试初始化，与代理连接、注销，订阅主题，发布主题消息，监听消息回调
    /// 作者：毛俊峰
    /// 时间：2023.08.17
    /// </summary>
    public class TestNetworkMqtt : MonoBehaviour
    {
        //private void OnEnable()
        //{
        //    //核心API
        //    //初始化并订阅主题
        //    NetworkMqtt.GetInstance.Init(new MqttConfig()).Subscribe(MqttTopicName.TopicTest);
        //    //监听消息回调
        //    NetworkMqtt.GetInstance.AddListener((object sender, MqttMsgPublishEventArgs e) =>
        //    {
        //        Debug.Log($"通过代理收到消息：{Encoding.UTF8.GetString(e.Message)}");
        //    });
        //    NetworkMqtt.GetInstance.AddListener((object sender, MqttMsgSubscribedEventArgs e) =>
        //    {
        //        Debug.Log($"客户端订阅消息成功回调 ，sender：{sender}");
        //    });
        //    //订阅多个主题
        //    NetworkMqtt.GetInstance.Subscribe("TopicTest1", "TopicTest2");
        //}

        //void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.Space))
        //    {
        //        //发送消息
        //        NetworkMqtt.GetInstance.Publish(MqttTopicName.TopicTest, "Unity Send Msg:" + System.DateTime.Now.ToString());
        //    }
        //    if (Input.GetKeyDown(KeyCode.Escape))
        //    {
        //        NetworkMqtt.GetInstance.DisConnect();
        //    }
        //}

        //private void OnDisable()
        //{
        //    NetworkMqtt.GetInstance.DisConnect();
        //}


        private void OnEnable()
        {
            //核心API

            NetworkMqtt.GetInstance.IsWebgl = false;

            //#if !UNITY_EDITOR && UNITY_WEBGL
            //            NetworkMqtt.GetInstance.IsWebgl = true;
            //#else
            //            NetworkMqtt.GetInstance.IsWebgl = false;
            //#endif

            //订阅多个主题
            NetworkMqtt.GetInstance.AddConnectedSucEvent(() =>
            {
                NetworkMqtt.GetInstance.Subscribe(MqttTopicName.TopicTest);
                NetworkMqtt.GetInstance.Subscribe("simulator/roomInfoData");
                NetworkMqtt.GetInstance.Subscribe("TopicTest", "TopicTest2");
            });

            //初始化并订阅主题
            NetworkMqtt.GetInstance.Init(new MqttConfig()
            {
                clientIP = "10.5.24.28",
                clientPort = 1883,
                clientID = "ass"
            });

            //监听消息回调
            NetworkMqtt.GetInstance.AddListenerSubscribe((string topic, string msg) =>
            {
                Debug.Log($"通过代理收到消息 topic:" + topic + ",msg:" + msg);
            });

        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //发送消息
                NetworkMqtt.GetInstance.Publish(MqttTopicName.TopicTest, "Unity Send TopicTest Msg:" + System.DateTime.Now.ToString());
                NetworkMqtt.GetInstance.Publish("simulator/roomInfoData", "Unity Send simulator/roomInfoData Msg:" + System.DateTime.Now.ToString());
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                NetworkMqtt.GetInstance.DisConnect();
            }
        }

        private void OnDisable()
        {
            NetworkMqtt.GetInstance.DisConnect();
        }
    }
}

