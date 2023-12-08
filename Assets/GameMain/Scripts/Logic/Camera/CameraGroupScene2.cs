using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFramework;
/// <summary>
/// 标题：scene2相机组管理
/// 功能：
/// 作者：毛俊峰
/// 时间：2023.12.08
/// </summary>
public class CameraGroupScene2 : SingletonByMono<CameraGroupScene2>
{
    private Camera m_MainCamera;
    private Camera m_CameraTop;
    private Camera m_CameraFixed;

    void Awake()
    {
        m_MainCamera = GameObject.Find("CameraGroup/MainCamera")?.GetComponent<Camera>();
        m_CameraTop = GameObject.Find("CameraGroup/CameraTop")?.GetComponent<Camera>();
        m_CameraFixed = GameObject.Find("CameraGroup/CameraFixed")?.GetComponent<Camera>();

    }

    public void ShowCamera(CameraType cameraType)
    {
        m_MainCamera?.gameObject.SetActive(cameraType == CameraType.Main);
        m_CameraTop?.gameObject.SetActive(cameraType == CameraType.Top);
        m_CameraFixed?.gameObject.SetActive(cameraType == CameraType.Fixed);

    }

    public enum CameraType
    {
        Main,
        Top,
        Fixed
    }
}
