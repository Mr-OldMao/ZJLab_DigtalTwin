#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 标题：Unity编辑器控制台 双击准确定位日志打印脚本位置
/// 功能：监听编辑器状态下，双击控制台定位日志打印的位置
/// 作者：毛俊峰
/// 时间：2022.11.11
/// 版本：1.0
/// </summary>
public class LogEditor
{
    private static LogEditor m_Instance;
    public static LogEditor GetInstacne()
    {
        if (m_Instance == null)
        {
            m_Instance = new LogEditor();
        }
        return m_Instance;
    }

    //封装日志系统类地址  //eg:"Assets/Scripts/MFramework/Utility/Log/Debugger.cs"
    private static string DeBuggerScriptPath = "";
    private int m_DebugerFileInstanceId;
    private Type m_ConsoleWindowType = null;
    private FieldInfo m_ActiveTextInfo;
    private FieldInfo m_ConsoleWindowFileInfo;

    private LogEditor()
    {
        foreach (var guid in UnityEditor.AssetDatabase.FindAssets("t:Script Debugger", new string[] { "Assets/" }))
        {
            string targetScriptPath = AssetDatabase.GUIDToAssetPath(guid);
            string[] targetScriptPathSplitArr = targetScriptPath.Split('/');
            if (targetScriptPathSplitArr[targetScriptPathSplitArr.Length - 1] == "Debugger.cs")
            {
                DeBuggerScriptPath = targetScriptPath;
                break;
            }
        }
        UnityEngine.Object debuggerFile = AssetDatabase.LoadAssetAtPath(DeBuggerScriptPath, typeof(UnityEngine.Object));
        m_DebugerFileInstanceId = debuggerFile.GetInstanceID();
        m_ConsoleWindowType = Type.GetType("UnityEditor.ConsoleWindow,UnityEditor");
        m_ActiveTextInfo = m_ConsoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
        m_ConsoleWindowFileInfo = m_ConsoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
    }

    [UnityEditor.Callbacks.OnOpenAssetAttribute(-1)]
    private static bool OnOpenAsset(int instanceID, int line)
    {
        if (instanceID == LogEditor.GetInstacne().m_DebugerFileInstanceId)
        {
            return LogEditor.GetInstacne().FindCode();
        }
        return false;
    }

    public bool FindCode()
    {
        var windowInstance = m_ConsoleWindowFileInfo.GetValue(null);
        var activeText = m_ActiveTextInfo.GetValue(windowInstance);
        string[] contentStrings = activeText.ToString().Split('\n');
        List<string> filePath = new List<string>();
        for (int index = 0; index < contentStrings.Length; index++)
        {
            if (contentStrings[index].Contains("(at Assets/"))
            {
                filePath.Add(contentStrings[index]);
            }
        }
        bool success = PingAndOpen(filePath[2]);
        return success;
    }

    public bool PingAndOpen(string fileContext)
    {
        string regexRule = @"at ([\w\W]*):(\d+)\)";
        Match match = Regex.Match(fileContext, regexRule);
        if (match.Groups.Count > 1)
        {
            string path = match.Groups[1].Value;
            string line = match.Groups[2].Value;
            UnityEngine.Object codeObject = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
            if (codeObject == null)
            {
                return false;
            }
            EditorGUIUtility.PingObject(codeObject);
            AssetDatabase.OpenAsset(codeObject, int.Parse(line));
            return true;
        }
        return false;
    }
}

#endif