/// <summary>
/// 标题：测试mqtt协议接收
/// 功能：
/// 作者：弓利剑
/// 时间：20230816
/// /// </summary>

using UnityEngine;
using System.Collections;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using System;

public class MqttInstance 
{
    public MqttClient client;
    public event Action<string, string> MessageReceived;


    public void Initialize(string ip, int port)
    {
        try
        {
            client = new MqttClient(IPAddress.Parse(ip), port, false, null);
        }
        catch (Exception e)
        {
            Debug.LogError("Error creating MQTT client: " + e.Message);
        }
    }

    public bool Connect(string clientId,string username,string password)
    {
        try
        {
            if (!client.IsConnected)
            {
                client.Connect(clientId, username, password);
                Debug.Log("Connected to MQTT broker");
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error connecting to MQTT broker: " + e.Message);
        }

        return false;
    }

    public bool Disconnect()
    {
        try
        {
            if (client.IsConnected)
            {
                client.Disconnect();
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error disconnecting from MQTT broker: " + e.Message);
        }

        return false;
    }

    public void Publish(string topic, string message)
    {
        try
        {
            if (client.IsConnected)
            {
                client.Publish(topic, System.Text.Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error publishing message: " + e.Message);
        }
    }

    public bool Subscribe(string topic)
    {
        try
        {
            if (client.IsConnected)
            {
                client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
                Debug.Log("Subscribed to topic: " + topic);
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error subscribing to topic: " + e.Message);
        }

        return false;
    }

    public bool Unsubscribe(string topic)
    {
        try
        {
            if (client.IsConnected)
            {
                client.Unsubscribe(new string[] { topic });
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error unsubscribing from topic: " + e.Message);
        }

        return false;
    }
}
