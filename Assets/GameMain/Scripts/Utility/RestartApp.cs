using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
/// <summary>
/// 标题：win PC端程序重启功能
/// 功能：
/// 作者：毛俊峰
/// 时间：2023.
/// </summary>
public class RestartApp : MonoBehaviour
{
    private string targetAppName = "Simulation_2023_12_12_15_39_49.exe";

    private string targetAppPath = string.Empty;

    private const uint sleepTime = 1000;//休眠时间毫秒

    void ReStart()
    {
        //延迟5秒启动

        string[] cmdOrder = new string[]
         {
            "@echo off",
            "echo wscript.sleep "+sleepTime.ToString()+" > sleep.vbs",
            "start /wait sleep.vbs",
            "start \"\" {0}",
            "del /f /s /q sleep.vbs",
            "exit"
         };

        string path = Application.dataPath + "/../";


        List<string> prefabs = new List<string>();
        prefabs = new List<string>(Directory.GetFiles(Application.dataPath + "/../", "*.exe", SearchOption.AllDirectories));
        foreach (string keyx in prefabs)
        {
            string[] strArr = keyx.Split(new char[] { '/', '\\' });
            Debug.LogError("path:" + keyx + ",exeName：" + strArr[strArr.Length - 1]);
            if (keyx.Contains(targetAppName))
            {
                targetAppPath = keyx;
                Debug.Log("is exist ");
                string _path = Application.dataPath;
                _path = _path.Remove(_path.LastIndexOf("/")) + "/";
                Debug.LogError(_path);
                string _name = Path.GetFileName(keyx);
                cmdOrder[3] = string.Format(cmdOrder[3], targetAppPath);
               // Application.OpenURL(path);
            }
        }

        string batPath = Application.dataPath + "/../restart.bat";
        if (File.Exists(batPath))
        {
            File.Delete(batPath);
        }
        using (FileStream fileStream = File.OpenWrite(batPath))
        {
            using (StreamWriter writer = new StreamWriter(fileStream, System.Text.Encoding.GetEncoding("UTF-8")))
            {
                foreach (string s in cmdOrder)
                {
                    writer.WriteLine(s);
                }
                writer.Close();
            }
        }
        Application.Quit();
        Application.OpenURL(batPath);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ReStart();
        }
    }
}
