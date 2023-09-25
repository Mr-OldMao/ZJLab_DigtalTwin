using MFramework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 标题：场景摄像机控制器
/// 功能：切换不同摄像机
/// 作者：毛俊峰
/// 时间：2023.09.01
/// </summary>
public class CameraControl : SingletonByMono<CameraControl>
{
    private Dictionary<CameraType, Camera> m_CameraArr;
    private Camera m_CameraTop;
    private Camera m_CameraFirst;
    private Camera m_CameraThree;
    private Camera m_CameraFree;

    public CameraType CurMainCamera
    {
        get;
        private set;
    } = CameraType.Top;

    public enum CameraType
    {
        Top = 0,
        First,
        Three,
        Free
    }

    void Awake()
    {
        m_CameraTop = GameObject.Find("CameraTop")?.GetComponent<Camera>();
        m_CameraFirst = GameObject.Find("CameraFirst")?.GetComponent<Camera>();
        m_CameraThree = GameObject.Find("CameraThree")?.GetComponent<Camera>();
        m_CameraFree = GameObject.Find("CameraFree")?.GetComponent<Camera>();

    }

    private void Start()
    {
        m_CameraFree?.gameObject.SetActive(false);
        m_CameraFree.depth = 1;
        m_CameraTop.depth = 0;
        m_CameraFirst.depth = 2;
        m_CameraThree.depth = 2;

        if (m_CameraTop.gameObject.GetComponent<TopCameraConfig>() == null)
        {
            m_CameraTop.gameObject.AddComponent<TopCameraConfig>();
        }
    }

    public void Init()
    {
        m_CameraArr = new Dictionary<CameraType, Camera>
        {
            { CameraType.Top, m_CameraTop},
            { CameraType.First, m_CameraFirst },
            { CameraType.Three, m_CameraThree},
            { CameraType.Free , m_CameraFree}
        };
        UIManager.GetInstance.Show<UIFormCameraHint>();
        MsgEvent.SendMsg(MsgEventName.ChangeCamera);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ChangeCamera();
        }
    }
    private void ChangeCamera()
    {
        int nextCameraTypeIndex = ((int)CurMainCamera + 1) % 3;
        CameraType nextCamera = (CameraType)nextCameraTypeIndex;

        Camera curCameraInfo = m_CameraArr[CurMainCamera];
        float curCameraDepth = curCameraInfo.depth;
        Rect curCameraRect = curCameraInfo.rect;


        m_CameraArr[CurMainCamera].depth = m_CameraArr[nextCamera].depth;
        m_CameraArr[CurMainCamera].rect = m_CameraArr[nextCamera].rect;

        m_CameraArr[nextCamera].depth = curCameraDepth;
        m_CameraArr[nextCamera].rect = curCameraRect;

        CurMainCamera = nextCamera;
        MsgEvent.SendMsg(MsgEventName.ChangeCamera);
    }

    //缓存切换至自由视角前的主摄像机
    private CameraType beforeMainCamera;
    public void ClickCameraFree()
    {
        //if (CurMainCamera != CameraType.Free)
        //{
        //    beforeMainCamera = CurMainCamera;
        //}
        m_CameraFree?.gameObject.SetActive(!m_CameraFree.gameObject.activeSelf);
        //CurMainCamera = m_CameraFree.gameObject.activeSelf ? CameraType.Free : beforeMainCamera;
        string des = m_CameraFree.gameObject.activeSelf ? "关闭自由视角" : "开启自由视角";
        UIManager.GetInstance.GetUIFormLogicScript<UIFormMain>().TxtCameraFree.text = des;

        MsgEvent.SendMsg(MsgEventName.ChangeCamera);
    }

    public Camera GetCameraEntity(CameraType cameraType)
    {
        Camera res = null;
        switch (cameraType)
        {
            case CameraType.First:
                res = m_CameraFirst;
                break;
            case CameraType.Three:
                res = m_CameraThree;

                break;
            case CameraType.Top:
                res = m_CameraTop;
                break;
            case CameraType.Free:
                res = m_CameraFree;
                break;
        }
        return res;
    }

    /// <summary>
    /// 获取当前摄像机所在屏幕上的位置
    /// </summary>
    /// <returns>0不在屏幕上，1-主屏幕，2-右上角，3-右下角</returns>
    public int GetCameraLocation(CameraType cameraType)
    {
        int res = 0;
        Camera camera = GetCameraEntity(cameraType);
        if (camera?.gameObject != null && camera.gameObject.activeSelf)
        {
            Rect cameraRect = camera.rect;
            if (cameraRect.x == 0 && cameraRect.y == 0 && cameraRect.width == 1 && cameraRect.height == 1)
            {
                res = 1;
            }else if (cameraRect.x == 0.7f && cameraRect.y == 0.7f && cameraRect.width == 1 && cameraRect.height == 1)
            {
                res = 2;
            }
            else if (cameraRect.x == 0.7f && cameraRect.y == 0 && cameraRect.width == 0.3f && cameraRect.height == 0.3f)
            {
                res = 3;
            }
        }
        return res;
    }
}
