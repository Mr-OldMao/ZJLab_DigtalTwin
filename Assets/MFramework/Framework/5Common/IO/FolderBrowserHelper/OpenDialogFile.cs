using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;


/// <summary>
/// 标题：PC平台，非编辑器下控制Windows窗体交互，对硬盘文件夹选择读取操作
/// 时间：2023.04.20
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class OpenDialogFile
{
    //public void TestMethod()
    //{
    //    // 打开文件目录
    //    FolderBrowserHelper.OpenFolder(@"E:\temp");
    //    // 选择JPG/PNG图片文件并返回选择文件路径
    //    FolderBrowserHelper.SelectFile(_ => Debug.Log(_), FolderBrowserHelper.IMAGEFILTER);
    //    // 选择文件目录并返回选择文件夹路径
    //    Debug.Log(FolderBrowserHelper.GetPathFromWindowsExplorer());
    //}


    public int structSize = 0;
    public IntPtr dlgOwner = IntPtr.Zero;
    public IntPtr instance = IntPtr.Zero;
    public String filter = null;
    public String customFilter = null;
    public int maxCustFilter = 0;
    public int filterIndex = 0;
    public String file = null;
    public int maxFile = 0;
    public String fileTitle = null;
    public int maxFileTitle = 0;
    public String initialDir = null;
    public String title = null;
    public int flags = 0;
    public short fileOffset = 0;
    public short fileExtension = 0;
    public String defExt = null;
    public IntPtr custData = IntPtr.Zero;
    public IntPtr hook = IntPtr.Zero;
    public String templateName = null;
    public IntPtr reservedPtr = IntPtr.Zero;
    public int reservedInt = 0;
    public int flagsEx = 0;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class OpenDialogDir
{
    public IntPtr hwndOwner = IntPtr.Zero;
    public IntPtr pidlRoot = IntPtr.Zero;
    public String pszDisplayName = null;
    public String lpszTitle = null;
    public UInt32 ulFlags = 0;
    public IntPtr lpfn = IntPtr.Zero;
    public IntPtr lParam = IntPtr.Zero;
    public int iImage = 0;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class OpenFileName
{
    public int structSize = 0;
    public IntPtr dlgOwner = IntPtr.Zero;
    public IntPtr instance = IntPtr.Zero;
    public String filter = null;
    public String customFilter = null;
    public int maxCustFilter = 0;
    public int filterIndex = 0;
    public String file = null;
    public int maxFile = 0;
    public String fileTitle = null;
    public int maxFileTitle = 0;
    public String initialDir = null;
    public String title = null;
    public int flags = 0;
    public short fileOffset = 0;
    public short fileExtension = 0;
    public String defExt = null;
    public IntPtr custData = IntPtr.Zero;
    public IntPtr hook = IntPtr.Zero;
    public String templateName = null;
    public IntPtr reservedPtr = IntPtr.Zero;
    public int reservedInt = 0;
    public int flagsEx = 0;
}

public static class FolderBrowserHelper
{

    #region Window

    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    private static extern bool GetSaveFileName([In, Out] OpenFileName ofn);

    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName([In, Out] OpenDialogFile ofn);

    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    private static extern bool GetSaveFileName([In, Out] OpenDialogFile ofn);

    [DllImport("shell32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    private static extern IntPtr SHBrowseForFolder([In, Out] OpenDialogDir ofn);

    [DllImport("shell32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    private static extern bool SHGetPathFromIDList([In] IntPtr pidl, [In, Out] char[] fileName);

    #endregion

    public const string IMAGEFILTER = "图片文件(*.jpg;*.png)\0*.jpg;*.png";
    public const string ALLFILTER = "所有文件(*.*)\0*.*";

    /// <summary>
    /// 选择文件
    /// </summary>
    /// <param name="callback">返回选择文件夹的路径</param>
    /// <param name="filter">文件类型筛选器</param>
    public static void SelectFile(Action<string> callback, string filter = ALLFILTER)
    {
        try
        {
            OpenFileName openFileName = new OpenFileName();
            openFileName.structSize = Marshal.SizeOf(openFileName);
            // openFileName.initialDir = "初始目录"

            openFileName.filter = filter;
            openFileName.file = new string(new char[256]);
            openFileName.maxFile = openFileName.file.Length;
            openFileName.fileTitle = new string(new char[64]);
            openFileName.maxFileTitle = openFileName.fileTitle.Length;
            openFileName.title = "选择文件";
            openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;
            if (GetSaveFileName(openFileName))
            {
                string filepath = openFileName.file; //选择的文件路径;  
                if (File.Exists(filepath))
                {
                    if (callback != null)
                        callback(filepath);
                    return;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        if (callback != null)
            callback(string.Empty);
    }

    /// <summary>
    /// 调用WindowsExploer 并返回所选文件夹路径
    /// </summary>
    /// <param name="dialogtitle">打开对话框的标题</param>
    /// <returns>所选文件夹路径</returns>
    public static string GetPathFromWindowsExplorer(string dialogtitle = "请选择下载路径")
    {
        try
        {
            OpenDialogDir ofn2 = new OpenDialogDir();
            ofn2.pszDisplayName = new string(new char[2048]);
            ; // 存放目录路径缓冲区  
            ofn2.lpszTitle = dialogtitle; // 标题  
            ofn2.ulFlags = 0x00000040; // 新的样式,带编辑框  
            IntPtr pidlPtr = SHBrowseForFolder(ofn2);

            char[] charArray = new char[2048];

            for (int i = 0; i < 2048; i++)
            {
                charArray[i] = '\0';
            }

            SHGetPathFromIDList(pidlPtr, charArray);
            string res = new string(charArray);
            res = res.Substring(0, res.IndexOf('\0'));
            return res;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        return string.Empty;
    }

    /// <summary>
    /// 打开目录
    /// </summary>
    /// <param name="path">将要打开的文件目录</param>
    public static void OpenFolder(string path)
    {
        System.Diagnostics.Process.Start("explorer.exe", path);
    }
}