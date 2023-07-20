using UnityEngine;

namespace MFramework
{
    /// <summary>
    /// 标题：测试PC平台，非编辑器下控制Windows窗体交互，对硬盘文件夹选择读取操作
    /// 作者：毛俊峰
    /// 时间：2023.05.19
    /// </summary>
    public class TestFolderBrowserHelper : MonoBehaviour
    {
        void Start()
        {
            // 打开文件目录
            FolderBrowserHelper.OpenFolder(@"E:\temp");
            // 选择JPG/PNG图片文件并返回选择文件路径
            FolderBrowserHelper.SelectFile(_ => Debug.Log(_), FolderBrowserHelper.IMAGEFILTER);
            // 选择文件目录并返回选择文件夹路径
            Debug.Log(FolderBrowserHelper.GetPathFromWindowsExplorer());

        }
    } 
}
