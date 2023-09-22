using MFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 标题：UnityTCP客户端
/// 功能：用于接收第一、三人称的相机视频流信息
/// 作者：毛俊峰
/// 时间：2023.09.22
/// </summary>
public class NetworkTCPClient : MonoBehaviour
{
    string ip = "127.0.0.1";//"127.0.0.1";//"10.11.80.54"
    int port = 10003;

    Socket socket = null;
    Thread thread = null;
    byte[] buffer = null;
    bool receState = true;

    int readTimes = 0;

    public RawImage rawImageFirst;
    public RawImage rawImageThree;

    private Queue<byte[]> datas;
    private Texture2D texture2DFirst;
    private Texture2D texture2DThree;
    void Start()
    {
        buffer = new byte[1024 * 1024 * 10]; //10m

        // 创建服务器, 以Tcp的方式
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(IPAddress.Parse(ip), port);

        // 开启一个线程, 用于接受数据
        thread = new Thread(new ThreadStart(Receive));
        thread.Start();

        datas = new Queue<byte[]>();
    }

    private void Receive()
    {
        while (thread.ThreadState == ThreadState.Running && socket.Connected)
        {
            // 接受数据Buffer count是数据的长度
            int count = socket.Receive(buffer);
            if (receState && count > 0)
            {
                receState = false;
                BytesToImage(count, buffer);
            }
        }
    }

    MemoryStream ms = null;
    public void BytesToImage(int count, byte[] bytes)
    {
        try
        {
            ms = new MemoryStream(bytes, 0, count);
            datas.Enqueue(ms.ToArray());    // 将数据存储在一个队列中，在主线程中解析数据。这是一个多线程的处理。

            readTimes++;

            if (readTimes > 5000)
            {
                readTimes = 0;
                GC.Collect(2);  // 达到一定次数的时候，开启GC，释放内存
            }
        }
        catch
        {

        }
        receState = true;
    }


    void Update()
    {
        if (datas.Count > 0)
        {
            if (texture2DFirst == null)
            {
                texture2DFirst = new Texture2D(Screen.width, Screen.height);
            }
            if (texture2DThree == null)
            {
                texture2DThree = new Texture2D(Screen.width, Screen.height);
            }

            string dataJson = System.Text.Encoding.Default.GetString(datas.Dequeue());
            Debug.Log(dataJson);


            // 处理纹理数据，并显示
            Msg msg =  JsonTool.GetInstance.JsonToObjectByLitJson<Msg>(dataJson);
             
            texture2DFirst.LoadImage(Convert.FromBase64String(msg.msgData[0].data));
            rawImageFirst.texture = texture2DFirst;
            //texture2DFirst.width = msg.msgData[0].width;
            //texture2DFirst.height = msg.msgData[0].height;

            texture2DThree.LoadImage(Convert.FromBase64String(msg.msgData[1].data));
            rawImageThree.texture = texture2DThree;
            //texture2DThree.width = msg.msgData[1].width;
            //texture2DThree.height = msg.msgData[1].height;
        }
    }

    public class Msg
    {
        public Data[] msgData;

        public class Data
        {
            public string title;
            //视屏流 二进制byte[]转string数据
            public string data;
            public int width;
            public int height;
        }
    }

    void OnDestroy()
    {
        try
        {
            if (socket != null)
            {
                socket.Shutdown(SocketShutdown.Both);
            }
        }
        catch { }

        try
        {
            if (thread != null)
            {
                thread.Abort();
            }
        }
        catch { }

        datas.Clear();
    }
}
