using System;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif
using UnityEngine;
/// <summary>
/// PC平台程序窗体布局
/// Unity发布PC 设置窗口 无边框（显示win任务栏），或全屏无边框
/// https://blog.csdn.net/o_ojjj/article/details/128640619
/// </summary>
public class LayoutWindow : MonoBehaviour
#if UNITY_EDITOR
    , IPreprocessBuildWithReport
#endif
{
    //是否开启强制布局程序
    private bool m_IsUseLayout = true;

    [Header("目标分辨率")]
    public Vector2 targetScreen;

    [Header("是否为全屏无边框")]
    public bool isFull = true;//T-程序窗体底边无视操作系统任务栏 F-程序窗体底边在操作系统任务栏上方

    //使用查找任务栏
    [DllImport("user32.dll")]
    static extern IntPtr FindWindow(string strClassName, int nptWindowName);
    //当前窗口
    [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
    //获取窗口位置以及大小
    [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left; //最左坐标
        public int Top; //最上坐标
        public int Right; //最右坐标
        public int Bottom; //最下坐标
    }
    //设置窗口位置，尺寸
    [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    //设置无windows自带边框
    [DllImport("user32.dll")] static extern IntPtr SetWindowLong(IntPtr hwnd, int _nIndex, int dwNewLong);

    const uint SWP_SHOWWINDOW = 0x0040;
    const int GWL_STYLE = -16;
    const int WS_BORDER = 1;

    Resolution[] resolutions;//分辨率
    private Rect screenPosition;//最终的屏幕的位置和长宽

    public int callbackOrder => 0;

#if UNITY_EDITOR
    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log("build before "+ m_IsUseLayout);
        if (m_IsUseLayout)
        {
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        }
    }
#endif



    void Awake()
    {
#if UNITY_EDITOR
        return;
#endif

        if (!m_IsUseLayout)
        {
            return;
        }

        if (targetScreen == null)
        {
            Debug.LogError("targetScreen is null");
            return;
        }
        else if (targetScreen.x <= 0 || targetScreen.y <= 0)
        {
            Debug.LogError("check targetScreen:" + targetScreen);
            return;
        }

        // Cursor.visible = false; // 鼠标隐藏
        //获取当前屏幕分辩率  
        //resolutions = Screen.resolutions;

        resolutions = new Resolution[] { new Resolution { width = (int)targetScreen.x, height = (int)targetScreen.y } };
        foreach (var item in resolutions)
        {
            Debug.Log("width:" + item.width + ",height:" + item.height);
        }


        if (isFull)
        {
            //设置全屏无边框
            Setposition();
        }
        else
        {
            //除任务栏外最大化窗口
            witnOutBorder();
        }
    }

    /// <summary>
    /// 获取当前窗口尺寸
    /// </summary>
    /// <returns></returns>
    public Rect GetWindowInfo()
    {
        RECT rect = new RECT();
        Rect targetRect = new Rect();
        GetWindowRect(GetForegroundWindow(), ref rect);
        targetRect.width = Mathf.Abs(rect.Right - rect.Left);
        targetRect.height = Mathf.Abs(rect.Top - rect.Bottom);

        //锚点在左上角
        targetRect.x = rect.Left;
        targetRect.y = rect.Top;
        return targetRect;
    }
    /// <summary>
    /// 获取任务栏高度
    /// </summary>
    /// <returns>任务栏高度</returns>
    private int GetTaskBarHeight()
    {
        int taskbarHeight = 10;
        IntPtr hWnd = FindWindow("Shell_TrayWnd", 0);       //找到任务栏窗口
        RECT rect = new RECT();
        GetWindowRect(hWnd, ref rect);                      //获取任务栏的窗口位置及大小
        taskbarHeight = (int)(rect.Bottom - rect.Top);      //得到任务栏的高度
        return taskbarHeight;
    }
    /// <summary>
    /// 除任务栏外最大化窗口（无边框）
    /// </summary>
    private void witnOutBorder()
    {
        //新的屏幕宽度
        screenPosition.width = resolutions[resolutions.Length - 1].width;
        //新的屏幕高度=当前屏幕分辨率的高度-状态栏的高度
        int currMaxScreenHeight = Screen.currentResolution.height - GetTaskBarHeight();
        screenPosition.height = currMaxScreenHeight;
        //新的分辨率(exe文件新的宽高)  这个是Unity里的设置屏幕大小，
        Screen.SetResolution((int)screenPosition.width, (int)screenPosition.height, false);

        //screenPosition.x = (int)((Screen.currentResolution.width - screenPosition.width) / 2);//宽度居中
        //screenPosition.y = (int)((Screen.currentResolution.height - screenPosition.height) / 2);//高度居中

        //设置无框
        SetWindowLong(GetForegroundWindow(), GWL_STYLE, WS_BORDER);
        //exe居左上显示；
        bool result = SetWindowPos(GetForegroundWindow(), 0, 0, 0, (int)screenPosition.width, (int)screenPosition.height, SWP_SHOWWINDOW);
        //exe居中显示；
        // bool result = SetWindowPos(GetForegroundWindow(), 0, (int)screenPosition.x, (int)screenPosition.y,  (int)screenPosition.width, (int)screenPosition.height, SWP_SHOWWINDOW);
    }

    /// <summary>
    /// 设置全屏无边框
    /// </summary>
    private void Setposition()
    {
        SetWindowLong(GetForegroundWindow(), GWL_STYLE, WS_BORDER);      //无边框
        bool result = SetWindowPos(GetForegroundWindow(), 0, 0, 0, resolutions[resolutions.Length - 1].width, resolutions[resolutions.Length - 1].height, SWP_SHOWWINDOW);
    }


}