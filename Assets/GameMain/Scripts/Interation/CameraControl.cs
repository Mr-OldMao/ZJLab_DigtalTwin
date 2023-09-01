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
    private Dictionary<CameraType,Camera> m_CameraArr;
    public CameraType CurMainCamera
    {
        get;
        private set;
    } = CameraType.Top;

    public enum CameraType
    {
        Top = 0,
        First,
        Three
    }

    void Start()
    {
        
    }

    public void Init()
    {
        m_CameraArr = new Dictionary<CameraType, Camera>
        {
            { CameraType.Top, GameObject.Find("CameraTop")?.GetComponent<Camera>() },
            { CameraType.First, GameObject.Find("CameraFirst")?.GetComponent<Camera>() },
            { CameraType.Three, GameObject.Find("CameraThree")?.GetComponent<Camera>() }
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
}
