using MFramework;
using System.Collections.Generic;
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
    }


    public void ClickCameraFree()
    {
        m_CameraFree?.gameObject.SetActive(!m_CameraFree.gameObject.activeSelf);
    }
}
