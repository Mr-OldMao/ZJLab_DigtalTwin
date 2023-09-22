using MFramework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

/// <summary>
/// 标题：UnityTCP服务端
/// 功能：用于传输第一、三人称的相机视频流信息，动态调整视频流的画质
/// 作者：毛俊峰
/// 时间：2023.09.22
/// </summary>
public class NetworkTCPServer : SingletonByMono<NetworkTCPServer>
{
    public Camera CameraFirstRenderer;
    public Camera CameraThreeRenderer;



    public string ip = "127.0.0.1";//"127.0.0.1";//"10.11.80.54";
    public int port = 10003;
    /// <summary>
    /// 渲染的质量 [0-1],1-原画质
    /// </summary>
    [Range(0, 1f)]
    public float renderValue = 1f;

    private float m_CurRenderValue;

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

        m_CurRenderValue = renderValue;
        firstCameraView = new RenderTexture((int)(Screen.width * m_CurRenderValue), (int)(Screen.height * m_CurRenderValue), 24);
        firstCameraView.enableRandomWrite = true;

        ThreeCameraView = new RenderTexture((int)(Screen.width * m_CurRenderValue), (int)(Screen.height * m_CurRenderValue), 24);
        ThreeCameraView.enableRandomWrite = true;

        CameraFirstRenderer.targetTexture = firstCameraView;
        CameraThreeRenderer.targetTexture = ThreeCameraView;

        oldPosFirstCam = CameraFirstRenderer.transform.position;
        oldRotFirstCam = CameraFirstRenderer.transform.rotation;

        oldPosThreeCam = CameraThreeRenderer.transform.position;
        oldRotThreeCam = CameraThreeRenderer.transform.rotation;

        StartThread();
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
            if (clients.ContainsKey(_socket.RemoteEndPoint.ToString()))
            {
                try
                {
                    clients[_socket.RemoteEndPoint.ToString()].socket.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                }
                clients.Remove(_socket.RemoteEndPoint.ToString());
            }

            Client client = new Client
            {
                socket = _socket
            };

            clients.Add(_socket.RemoteEndPoint.ToString(), client);

            isNewAdd = 1;
        }
    }

    void Update()
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendMsg();
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
            SendMsg();
        }
        success = true;
    }

    private void SendMsg()
    {
        if (screenShot == null || m_CurRenderValue != renderValue)
        {
            m_CurRenderValue = renderValue;
            screenShot = new Texture2D((int)(Screen.width * m_CurRenderValue), (int)(Screen.height * m_CurRenderValue), TextureFormat.RGB24, false);
            threeScreenShot = new Texture2D((int)(Screen.width * m_CurRenderValue), (int)(Screen.height * m_CurRenderValue), TextureFormat.RGB24, false);
            Debug.Log("1111111");
        }
        // 读取屏幕像素进行渲染
        RenderTexture.active = firstCameraView;
        screenShot.ReadPixels(new Rect(0, 0, firstCameraView.width, firstCameraView.height), 0, 0);
        RenderTexture.active = null;


        // 读取屏幕像素进行渲染
        RenderTexture.active = ThreeCameraView;
        threeScreenShot.ReadPixels(new Rect(0, 0, ThreeCameraView.width, ThreeCameraView.height), 0, 0);
        RenderTexture.active = null;

        byte[] bytesFirst = screenShot.EncodeToJPG(100);
        byte[] bytesThree = threeScreenShot.EncodeToJPG(100);


        Msg msg = new Msg()
        {
            msgData = new Msg.Data[]
            {
               new Msg.Data {
                    data =  Convert.ToBase64String(bytesFirst),
                    title = "FirstCameraVideoStreaming",
                    width = (int)(Screen.width * m_CurRenderValue),
                    height =(int)(Screen.height * m_CurRenderValue)
                },
               new Msg.Data
               {
                    data = Convert.ToBase64String(bytesThree),
                    title = "ThreeCameraVideoStreaming",
                    width = (int)(Screen.width * m_CurRenderValue),
                    height =(int)(Screen.height * m_CurRenderValue)
               }
             }
        };
        string msgJson = JsonTool.GetInstance.ObjectToJsonStringByLitJson(msg);
        byte[] msgBytes = System.Text.Encoding.Default.GetBytes(msgJson);


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
                    clients.Remove(val.socket.RemoteEndPoint.ToString());
                }
            }
        }
        gc_count++;
        if (gc_count > 5000)
        {
            gc_count = 0;
            GC.Collect(2);
        }
        Debug.Log("发送数据:" + (float)msgBytes.Length / 1024f + "KB");

        oldPosFirstCam = CameraFirstRenderer.transform.position;
        oldRotFirstCam = CameraFirstRenderer.transform.rotation;

        oldPosThreeCam = CameraThreeRenderer.transform.position;
        oldRotThreeCam = CameraThreeRenderer.transform.rotation;

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

class Client
{
    public Socket socket = null;
}