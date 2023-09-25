using MFramework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine;
using System.Net.Sockets;

/// <summary>
/// 标题：视频流
/// 功能：用于传输第一、三人称的相机视频流信息，通过Mqtt通信，动态调整视频流的画质
/// 作者：毛俊峰
/// 时间：2023.09.22
/// </summary>
public class LiveStreaming : SingletonByMono<LiveStreaming>
{
    public Camera CameraFirstRenderer;
    public Camera CameraThreeRenderer;
    /// <summary>
    /// 是否开启视频流
    /// </summary>
    public bool IsBeginLiveStreaming { get; set; } = false;

    /// <summary>
    /// 使用mqtt通信 否则使用tcp通信
    /// </summary>
    private bool m_UseMQTT = true;


    public string ip = "10.11.80.54";//"127.0.0.1";//"10.11.80.54";
    public int port = 10003;
    /// <summary>
    /// 渲染的质量 [0-1],1-原画质
    /// </summary>
    [Range(1, 100)]
    public int renderValue = 50;

    RenderTexture firstCameraView = null;
    RenderTexture ThreeCameraView = null;

    Socket socket = null;

    Thread thread = null;

    bool success = true;

    Dictionary<string, Client> clients = new Dictionary<string, Client>();

    Vector3 oldPosFirstCam;   // 旧位置
    Quaternion oldRotFirstCam;    // 旧旋转
    Vector3 oldPosThreeCam;
    Quaternion oldRotThreeCam;

    public void Init(Transform firstCameraParent, Transform threeCameraParent)
    {
        CameraFirstRenderer = GameObject.Find("CameraFirstRenderer")?.GetComponent<Camera>();
        CameraThreeRenderer = GameObject.Find("CameraThreeRenderer")?.GetComponent<Camera>();

        CameraFirstRenderer.transform.parent = firstCameraParent;
        CameraThreeRenderer.transform.parent = threeCameraParent;

        CameraFirstRenderer.transform.localPosition = Vector3.zero;
        CameraFirstRenderer.transform.localRotation = Quaternion.Euler(Vector3.zero);
        CameraThreeRenderer.transform.localPosition = Vector3.zero;
        CameraThreeRenderer.transform.localRotation = Quaternion.Euler(Vector3.zero);


        firstCameraView = new RenderTexture((int)(Screen.width), (int)(Screen.height), 24);
        firstCameraView.enableRandomWrite = true;

        ThreeCameraView = new RenderTexture((int)(Screen.width), (int)(Screen.height), 24);
        ThreeCameraView.enableRandomWrite = true;

        CameraFirstRenderer.targetTexture = firstCameraView;
        CameraThreeRenderer.targetTexture = ThreeCameraView;

        oldPosFirstCam = CameraFirstRenderer.transform.position;
        oldRotFirstCam = CameraFirstRenderer.transform.rotation;

        oldPosThreeCam = CameraThreeRenderer.transform.position;
        oldRotThreeCam = CameraThreeRenderer.transform.rotation;

        if (!m_UseMQTT)
        {
            StartThread();
        }
    }


    void StartThread()
    {
        // 开启Socket
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
        socket.Listen(100);

        // 开启一个线程发送渲染数据
        thread = new Thread(new ThreadStart(OnStart));
        thread.Start();
    }

    int isNewAdd = 0;

    void OnStart()
    {
        Debug.Log("Socket创建成功");
        while (thread.ThreadState == ThreadState.Running)
        {
            Socket _socket = socket.Accept();
            if (clients.ContainsKey(_socket.LocalEndPoint.ToString()))
            {
                try
                {
                    clients[_socket.LocalEndPoint.ToString()].socket.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                }
                clients.Remove(_socket.LocalEndPoint.ToString());
            }

            Client client = new Client
            {
                socket = _socket
            };

            clients.Add(_socket.LocalEndPoint.ToString(), client);

            isNewAdd = 1;
        }
    }

    void Update()
    {
        if (IsBeginLiveStreaming)
        {
            if (m_UseMQTT)
            {
                SendTexture();
            }
            else
            {
                if (success && clients.Count > 0)
                {
                    success = false;
                    SendTexture();
                }
                if (isNewAdd > 0)
                {
                    isNewAdd = 0;
                    SendTexture(1);
                }
            } 
        }
    }

    void OnGUI()
    {
        //GUI.DrawTexture(new Rect(10, 10, 240, 135), cameraView, ScaleMode.StretchToFill);
    }

    void OnApplicationQuit()
    {
        try
        {
            socket.Shutdown(SocketShutdown.Both);
        }
        catch { }

        try
        {
            thread.Abort();
        }
        catch { }
    }



    Texture2D screenShot = null;
    Texture2D threeScreenShot = null;
    int gc_count = 0;

    void SendTexture(int isInt = 0)
    {
        if ((!oldPosFirstCam.Equals(CameraFirstRenderer.transform.position) || !oldRotFirstCam.Equals(CameraFirstRenderer.transform.rotation))
            || (!oldPosThreeCam.Equals(CameraThreeRenderer.transform.position) || !oldRotThreeCam.Equals(CameraThreeRenderer.transform.rotation))
            || isInt == 1)
        {
            byte[] msgBytes = GetLiveData();
            if (msgBytes == null || msgBytes.Length == 0)
            {
                Debug.LogError("msgBytes is null");
                return;
            }
            if (m_UseMQTT)
            {
                InterfaceDataCenter.GetInstance.SendMQTTLiveData(msgBytes);
            }
            else
            {
                //del
                List<string> removeClient = null;
                foreach (var val in clients.Values)
                {
                    try
                    {
                        val.socket.Send(msgBytes);
                    }
                    catch
                    {
                        if (!val.socket.Connected)
                        {
                            if (removeClient == null)
                            {
                                removeClient = new List<string>();
                            }
                            removeClient.Add(val.socket.LocalEndPoint.ToString());
                        }
                    }
                }
                if (removeClient?.Count > 0)
                {
                    for (int i = 0; i < removeClient.Count; i++)
                    {
                        clients.Remove(removeClient[i]);
                    }
                }
            }
        }
        success = true;
    }

    private byte[] GetLiveData()
    {
        byte[] msgBytes = null;
        if (screenShot == null)
        {
            screenShot = new Texture2D((int)(Screen.width), (int)(Screen.height), TextureFormat.RGB24, false);
            threeScreenShot = new Texture2D((int)(Screen.width), (int)(Screen.height), TextureFormat.RGB24, false);
        }
        // 读取屏幕像素进行渲染
        RenderTexture.active = firstCameraView;
        screenShot.ReadPixels(new Rect(0, 0, firstCameraView.width, firstCameraView.height), 0, 0, false);
        RenderTexture.active = null;

        // 读取屏幕像素进行渲染
        RenderTexture.active = ThreeCameraView;
        threeScreenShot.ReadPixels(new Rect(0, 0, ThreeCameraView.width, ThreeCameraView.height), 0, 0, false);
        RenderTexture.active = null;

        byte[] bytesFirst = screenShot.EncodeToJPG(renderValue);
        byte[] bytesThree = threeScreenShot.EncodeToJPG(renderValue);

        Msg msg = new Msg()
        {
            msgData = new Msg.Data[]
            {
               new Msg.Data {
                    data =  Convert.ToBase64String(bytesFirst),
                    title = "FirstCameraVideoStreaming",
                    width = (int)(Screen.width ),
                    height =(int)(Screen.height )
                },
               new Msg.Data
               {
                    data = Convert.ToBase64String(bytesThree),
                    title = "ThreeCameraVideoStreaming",
                    width = (int)(Screen.width),
                    height =(int)(Screen.height)
               }
             }
        };
        string msgJson = JsonTool.GetInstance.ObjectToJsonStringByLitJson(msg);
        msgBytes = System.Text.Encoding.Default.GetBytes(msgJson);

        gc_count++;
        if (gc_count > 5000)
        {
            gc_count = 0;
            GC.Collect(2);
        }
        //Debug.Log("发送数据:" + (float)msgBytes.Length / 1024f + "KB");


        oldPosFirstCam = CameraFirstRenderer.transform.position;
        oldRotFirstCam = CameraFirstRenderer.transform.rotation;
        oldPosThreeCam = CameraThreeRenderer.transform.position;
        oldRotThreeCam = CameraThreeRenderer.transform.rotation;

        return msgBytes;
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
        if (!m_UseMQTT)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch { }

            try
            {
                thread.Abort();
            }
            catch { }
        }
    }

}

class Client
{
    public Socket socket = null;
}