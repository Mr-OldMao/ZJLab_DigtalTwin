using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
/// <summary>
/// 标题：Socket服务端
/// 功能：可直接移植.net控制台程序
/// 作者：毛俊峰
/// 时间：2022.12.26
/// </summary>
namespace MFramework
{
    class SocketServer : MonoBehaviour
    {
        /// <summary>
        /// 用于与客户端连接Socket
        /// </summary>
        private static Socket m_SocketConnect;
        /// <summary>
        /// 用于与客户端通信Socket容器
        /// </summary>
        private static List<SocketMsgInfo> m_ListSocketInfo = new List<SocketMsgInfo>();
        private static byte[] datas = new byte[1024];

        private void Start()
        {
            Main(null);
        }

        private static void Main(string[] args)
        {
            m_SocketConnect = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            EndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.110.69"), 23123);
            m_SocketConnect.Bind(endPoint);
            Debug.Log("服务器启动成功！");
            m_SocketConnect.Listen(3);
            Debug.Log("开始监听...");
            Thread thread = new Thread(Connect);
            thread.Start();
        }

        private static void Connect()
        {
            while (true)
            {
                Socket socketMsg = m_SocketConnect.Accept();
                Debug.Log("客户端链接成功 Point：" + socketMsg.RemoteEndPoint);
                m_ListSocketInfo.Add(new SocketMsgInfo(socketMsg));
            }
        }

        #region SendMsg
        /// <summary>
        /// 向所有客户端发送消息
        /// </summary>
        /// <param name="msg"></param>
        public void SendMsg(string msg)
        {
            foreach (var item in m_ListSocketInfo)
            {
                if (item.IsConnected)
                {
                    SendMsg(msg, item.socketMsg);
                }
            }
        }

        /// <summary>
        /// 向指定客户端发送消息
        /// </summary>
        /// <param name="msg"></param>
        public void SendMsg(string msg, Socket socketMsg)
        {
            SocketMsgInfo socketMsgInfo = m_ListSocketInfo.Find((p) => p.socketMsg == socketMsg);
            if (socketMsgInfo != null)
            {
                SendMsg(msg, socketMsgInfo);
            }
            else
            {
                Debug.Log("服务器发送消息失败，容器中未找到该Socket缓存 socketMsg：" + socketMsg.ToString() + "，msg：" + msg);
            }
        }

        /// <summary>
        /// 向指定客户端发送消息
        /// </summary>
        /// <param name="msg"></param>
        public void SendMsg(string msg, SocketMsgInfo socketMsg)
        {
            socketMsg?.SendMsg(msg);
        }
        #endregion

        /// <summary>
        /// 服务端与客户端通信数据信息
        /// </summary>
        public class SocketMsgInfo
        {
            /// <summary>
            /// 通信Socket
            /// </summary>
            public Socket socketMsg;
            public bool IsConnected { get => socketMsg.Connected; }

            public SocketMsgInfo(Socket socketMsg)
            {
                this.socketMsg = socketMsg;
                Thread thread = new Thread(ReceiveMsg);
                thread.Start();
            }
            public void ReceiveMsg()
            {
                while (true)
                {
                    //每十毫秒响应一次，返回ture表示与客户端断开连接
                    if (socketMsg.Poll(10, SelectMode.SelectRead))
                    {
                        socketMsg.Close();
                        break;
                    }
                    int length = socketMsg.Receive(datas);
                    string msg = Encoding.UTF8.GetString(datas, 0, length);
                    Debug.Log("ServerReceive：" + msg);

                    SendMsg("我是服务端");
                    //SendMsg(Console.ReadLine());
                }
            }

            public void SendMsg(string msg)
            {
                if (IsConnected)
                {
                    datas = Encoding.UTF8.GetBytes(msg);
                    socketMsg.Send(datas);
                    Debug.Log("ServerSend：" + msg);
                }
                else
                {
                    Debug.Log("服务器发送消息失败，该Socket断开连接 socketMsg：" + socketMsg.ToString() + "，msg：" + msg);
                }
            }
        }
    }
}
