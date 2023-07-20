using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace MFramework
{
    /// <summary>
    /// 标题：Socket客户端
    /// 功能：连接Socket服务器，收发消息
    /// 作者：毛俊峰
    /// 时间：2022.12.26
    /// </summary>
    public class SocketClient : MonoBehaviour
    {
        private Socket m_SocketClient;
        private static byte[] m_Datas = new byte[2048];
        /// <summary>
        /// 客户端连接状态
        /// </summary>
        public bool IsConnected { get => m_SocketClient.Connected; }

        private void Start()
        {
            Debug.Log("Q客服端链接服务器，W客户端发送信息到服务器，E断开socket连接");
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        private void ConnentSerive()
        {
            m_SocketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            EndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.110.69"), 23123);
            m_SocketClient.Connect(endPoint);

            //监听 接收服务器消息
            Thread thread = new Thread(ReceiveMsg);
            thread.Start();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ConnentSerive();
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                SendMsg("我是客户端");
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                CloseConnect();
            }
        }

        private void SendMsg(string msg)
        {
            byte[] data = Encoding.UTF8.GetBytes(msg);
            if (!m_SocketClient.Connected)
            {
                Debug.Log("Socket断开连接，尝试重新连接服务器");
                ConnentSerive();
            }
            m_SocketClient.Send(data);
            Debug.Log("ClientSend：" + msg);
        }

        private void ReceiveMsg()
        {
            while (true)
            {
                //每十毫秒响应一次，返回ture表示与服务端断开连接
                if (m_SocketClient.Poll(10, SelectMode.SelectRead))
                {
                    CloseConnect();
                    break;
                }

                int length = m_SocketClient.Receive(m_Datas);
                string msg = Encoding.UTF8.GetString(m_Datas, 0, length);
                Debug.Log("ClientReceive：" + msg);
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        private void CloseConnect()
        {
            Debug.Log("Socket断开连接");
            m_SocketClient.Close();
        }
    }
}
