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

    /// <summary>
    /// 计时器
    /// </summary>
    private float m_CurTime = 0;
    /// <summary>
    /// 帧动画帧数
    /// </summary>
    private int m_FrameCount = 10;

    public string ip = "10.11.80.54";//"127.0.0.1";//"10.11.80.54";
    public int port = 10003;
    /// <summary>
    /// 渲染的质量 [0-100],100-原画质
    /// </summary>
    [Range(1, 100)]
    public int renderValue = 20;

    public RenderTexture firstCameraView = null;
    RenderTexture ThreeCameraView = null;
    Vector3 oldPosFirstCam;
    Quaternion oldRotFirstCam;
    Vector3 oldPosThreeCam;
    Quaternion oldRotThreeCam;


    Socket socket = null;
    Thread thread = null;
    bool success = true;
    Dictionary<string, Client> clients = new Dictionary<string, Client>();



    public void Init(Transform firstCameraParent, Transform threeCameraParent)
    {
        m_FrameCount = MainData.ConfigData.VideoStreaming.Frame;
        renderValue = MainData.ConfigData.VideoStreaming.Quality;
        Debugger.Log($"m_FrameCount:{m_FrameCount},renderValue:{renderValue}");

        CameraFirstRenderer = GameObject.Find("CameraFirstRenderer")?.GetComponent<Camera>();
        CameraThreeRenderer = GameObject.Find("CameraThreeRenderer")?.GetComponent<Camera>();

        CameraFirstRenderer.transform.parent = firstCameraParent;
        CameraThreeRenderer.transform.parent = threeCameraParent;

        CameraFirstRenderer.transform.localPosition = Vector3.zero;
        CameraFirstRenderer.transform.localRotation = Quaternion.Euler(Vector3.zero);
        CameraThreeRenderer.transform.localPosition = Vector3.zero;
        CameraThreeRenderer.transform.localRotation = Quaternion.Euler(Vector3.zero);

        //firstCameraView = new RenderTexture((int)(Screen.width), (int)(Screen.height), 0);
        //firstCameraView.enableRandomWrite = true;
        //Debugger.Log("LiveStreaming.init2");
        //ThreeCameraView = new RenderTexture((int)(Screen.width), (int)(Screen.height), 0);
        //ThreeCameraView.enableRandomWrite = true;
        //Debugger.Log("LiveStreaming.init3");
        //CameraFirstRenderer.targetTexture = firstCameraView;
        //Debugger.Log("LiveStreaming.init4");
        //CameraThreeRenderer.targetTexture = ThreeCameraView;
        //Debugger.Log("LiveStreaming.init5");

        firstCameraView = CameraFirstRenderer.targetTexture;
        ThreeCameraView = CameraThreeRenderer.targetTexture;
        firstCameraView.enableRandomWrite = true;
        ThreeCameraView.enableRandomWrite = true;


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

    private int isNewAdd = 0;

    void OnStart()
    {
        Debugger.Log("Socket创建成功");
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



    void FixedUpdate()
    {
        if (IsBeginLiveStreaming)
        {
            m_CurTime += Time.deltaTime;
            if (m_UseMQTT)
            {
                if (m_CurTime >= 1f / m_FrameCount)
                {
                    SendTexture();
                    m_CurTime = 0;
                }
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



    Texture2D firstScreenShot = null;
    Texture2D threeScreenShot = null;
    private int gc_count = 0;

    void SendTexture(int isInt = 0)
    {
        if ((!oldPosFirstCam.Equals(CameraFirstRenderer.transform.position) || !oldRotFirstCam.Equals(CameraFirstRenderer.transform.rotation))
            || (!oldPosThreeCam.Equals(CameraThreeRenderer.transform.position) || !oldRotThreeCam.Equals(CameraThreeRenderer.transform.rotation))
            || isInt == 1)
        {
            byte[] msgBytes = GetLiveData();
            if (msgBytes == null || msgBytes.Length == 0)
            {
                Debugger.LogError("msgBytes is null");
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
        if (firstScreenShot == null)
        {
            //firstScreenShot = new Texture2D((int)(Screen.width), (int)(Screen.height), TextureFormat.RGB24, false);
            //threeScreenShot = new Texture2D((int)(Screen.width), (int)(Screen.height), TextureFormat.RGB24, false); 
            firstScreenShot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            threeScreenShot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        }

        // 读取屏幕像素进行渲染
        RenderTexture.active = firstCameraView;
        firstScreenShot.ReadPixels(new Rect(0, 0, firstCameraView.width, firstCameraView.height), 0, 0, false);
        RenderTexture.active = null;
        // 读取屏幕像素进行渲染
        RenderTexture.active = ThreeCameraView;
        threeScreenShot.ReadPixels(new Rect(0, 0, ThreeCameraView.width, ThreeCameraView.height), 0, 0, false);
        RenderTexture.active = null;
        byte[] bytesFirst = firstScreenShot.EncodeToJPG(renderValue);
        byte[] bytesThree = threeScreenShot.EncodeToJPG(renderValue);
        Msg msg = new Msg()
        {
            msgData = new Msg.Data[]
            {
               new Msg.Data {
                    data =  Convert.ToBase64String(bytesFirst),
                    title = "FirstCameraVideoStreaming",
                    width = 1920,// (int)(Screen.width ),
                    height = 1080//(int)(Screen.height )
                },
               new Msg.Data
               {
                    data = Convert.ToBase64String(bytesThree),
                    title = "ThreeCameraVideoStreaming",
                    width = 1920,//(int)(Screen.width),
                    height =1080//(int)(Screen.height)
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
        //Debugger.Log("发送数据:" + (float)msgBytes.Length / 1024f + "KB");


        oldPosFirstCam = CameraFirstRenderer.transform.position;
        oldRotFirstCam = CameraFirstRenderer.transform.rotation;
        oldPosThreeCam = CameraThreeRenderer.transform.position;
        oldRotThreeCam = CameraThreeRenderer.transform.rotation;

        return msgBytes;
    }

    public class Msg
    {
        public string sceneID = MainData.SceneID;
        public string tmpId = MainData.tmpID;
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