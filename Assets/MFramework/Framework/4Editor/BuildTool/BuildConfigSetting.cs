#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// 标题：自动化打包参数配置设置窗体 配合一键打包使用
/// 作者：毛俊峰
/// 时间：2023-04-18
/// 功能：配置一键打包的设置参数， 配合一键打包使用
/// 编辑器菜单：一键打包 快捷键Shift+B  MFreamwork/BuildTool/BuildTargetPlatformBundle  
///             参数配置 快捷键Shift+S  MFreamwork/BuildTool/BuildConfigSetting
/// </summary>
public class BuildConfigSetting : ScriptableWizard
{
    #region EditorExtend
    [Tooltip("产品名称")]
    public string productName;
    [Tooltip("公司名称")]
    public string companyName;
    [Tooltip("版本号(x.x.x)")]
    public string version;
    //当前平台标识
    [Tooltip("当前平台标识(x.x.x)")]
    public string bundleIdentifier;
    [Tooltip("是否打包调试版本")]
    public bool isDevelopmentBuild = false;
    private string buildOutPutPath;

    //默认打包路径
    public static string BuildRootDefaultPath
    {
        get
        {
            string defaultPath = Application.dataPath + "/../_Build";
            if (!Directory.Exists(defaultPath))
            {
                Directory.CreateDirectory(defaultPath);
            }
            return defaultPath;
        }
    }

    [MenuItem(BuildTool.EditorMenuRootPath + "/BuildConfigSetting #S")]
    public static void EditorBuildConfigSetting()
    {
        DisplayWizard<BuildConfigSetting>("BuildConfigSetting", "保存并退出", "保存不退出");
    }
    private void OnWizardCreate()
    {
        //Debug.Log("OnWizardCreate");
        SaveConfigSetting();
    }

    private void OnWizardOtherButton()
    {
        //Debug.Log("OnWizardOtherButton");
        SaveConfigSetting();
    }

    private void Reset()
    {
        //Debug.Log("Reset");
        productName = PlayerSettings.productName;
        companyName = PlayerSettings.companyName;
        version = PlayerSettings.bundleVersion;
        bundleIdentifier = PlayerSettings.applicationIdentifier;
        isDevelopmentBuild = EditorUserBuildSettings.development;
        buildOutPutPath = PlayerPrefs.GetString(PlayerPrefsKeyGroup.Build_OutPutPath_Editor, BuildRootDefaultPath);
    }

    protected override bool DrawWizardGUI()
    {
        base.DrawWizardGUI();
        //水平布局
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Unity打包路径", GUILayout.Width(147f));
            buildOutPutPath = GUILayout.TextField(buildOutPutPath);
            if (GUILayout.Button("浏览", GUILayout.Width(50f)))
            {
                string selectPath = EditorUtility.OpenFolderPanel("Unity打包路径窗口", Application.dataPath, "请选择文件夹");
                if (!string.IsNullOrEmpty(selectPath))
                {
                    buildOutPutPath = selectPath;
                }
            }
        }
        GUILayout.EndHorizontal();
        return default;
    }

    private void SaveConfigSetting()
    {
        if (!string.IsNullOrEmpty(productName))
        {
            PlayerSettings.productName = productName;
        }
        else
        {
            Debug.LogError("部分配置保存失败！请检查 productName：" + productName);
        }

        if (!string.IsNullOrEmpty(companyName))
        {
            PlayerSettings.companyName = companyName;
        }
        else
        {
            Debug.LogError("部分配置保存失败！请检查 companyName：" + companyName);
        }

        if (!string.IsNullOrEmpty(version))
        {
            PlayerSettings.bundleVersion = version;

        }
        else
        {
            Debug.LogError("部分配置保存失败！请检查 version：" + version);
        }

        if (!string.IsNullOrEmpty(bundleIdentifier))
        {
            PlayerSettings.applicationIdentifier = bundleIdentifier;

        }
        else
        {
            Debug.LogError("部分配置保存失败！请检查 bundleIdentifier：" + bundleIdentifier);
        }

        if (!string.IsNullOrEmpty(buildOutPutPath))
        {
            PlayerPrefs.SetString(PlayerPrefsKeyGroup.Build_OutPutPath_Editor, buildOutPutPath);
        }
        else
        {
            Debug.LogError("部分配置保存失败！请检查 buildBundlePath：" + buildOutPutPath);
        }

        PlayerPrefs.SetInt(PlayerPrefsKeyGroup.Is_Development_Build, isDevelopmentBuild ? 1 : 0);

        EditorUserBuildSettings.development = isDevelopmentBuild;
        if (isDevelopmentBuild)
        {
            EditorUserBuildSettings.allowDebugging = true;
        }
        
        Debug.Log($"保存完成 " +
            $"\nproductName = { PlayerSettings.productName}," +
            $"\ncompanyName = { PlayerSettings.companyName}," +
            $"\nversion = {PlayerSettings.bundleVersion }," +
            $"\nbundleIdentifier = { PlayerSettings.applicationIdentifier}," +
            $"\nisDevelopmentBuild = {PlayerPrefs.GetInt(PlayerPrefsKeyGroup.Is_Development_Build) == 1}," +
            $"\nbuildOutPutPath = {PlayerPrefs.GetString(PlayerPrefsKeyGroup.Build_OutPutPath_Editor)}");
    }
    #endregion
     
}

/// <summary>
/// 数据持久化基于PlayerPrefs的key Group
/// </summary>
public partial class PlayerPrefsKeyGroup
{
    /// <summary>
    /// 是否调试模式打包 0-F 1-T
    /// </summary>
    public const string Is_Development_Build = "Is_Development_Build";
    /// <summary>
    /// 打包路径 Editor环境
    /// </summary>
    public const string Build_OutPutPath_Editor = "Build_OutPutPath_Editor";

    /// <summary>
    /// 打包路径 jenkins/command环境
    /// </summary>
    public const string Build_OutPutPath_CMD = "Build_OutPutPath_CMD";
}

#endif