using System.IO;
using UnityEngine;

namespace MFramework
{
    /// <summary>
    /// 标题：PC平台工具
    /// 作者：毛俊峰
    /// 时间：2023-04-18
    /// 功能：PC平台打开指定文件夹
    /// </summary>
    public class WindowsTool
    {
        /// <summary>
        /// 打开Windows指定文件夹
        /// </summary>
        /// <param name="path"></param>
        public static void OpenDirectory(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            path = path.Replace("/", "\\");
            if (!Directory.Exists(path))
            {
                Debug.LogError("No Directory: " + path);
                return;
            }
            System.Diagnostics.Process.Start("explorer.exe", path);
        }
    } 
}
