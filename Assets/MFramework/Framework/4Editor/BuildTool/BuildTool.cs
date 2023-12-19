#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;
/// <summary>
/// 标题：自动打包工具
/// 作者：毛俊峰
/// 时间：2023-04-18
/// 功能：根据参数配置一键打包当前平台(Android,PC)的项目,
/// 编辑器菜单：一键打包 快捷键Shift+B  MFreamwork/BuildTool/BuildTargetPlatformBundle  
///             参数配置 快捷键Shift+S  MFreamwork/BuildTool/BuildConfigSetting
/// </summary>
public class BuildTool : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public const string EditorMenuRootPath = "MFramework/BuildTool";

    public int callbackOrder => 0;
    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log("Building Start...  StartTime:" + report.summary.buildStartedAt);
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        Debug.Log("Building End" +
            "\nOutPutPath：" + report.summary.outputPath + "，" +
            "\nSize:" + string.Format("{0:N1}", report.summary.totalSize / 1024f / 1024f) + "M，" +
            "\nEndTime:" + report.summary.buildEndedAt
            );
    }


    [MenuItem((EditorMenuRootPath + "/BuildTargetPlatformBundle #B"))]
    public static void EditorBuildCurrentPlatform()
    {
        Build(PlayerPrefs.GetString(PlayerPrefsKeyGroup.Build_OutPutPath_Editor, BuildConfigSetting.BuildRootDefaultPath));
    }
    /// <summary>
    /// 当前打包的导出的全路径
    /// </summary>
    static string curBuildOutPutAllPath;
    /// <summary>
    /// 产品名称
    /// </summary>
    static string appName;
    /// <summary>
    /// Windows全路径 打包完成自动打开文件夹
    /// </summary>
    static string buildEndAutoOpenFolderPath;


    /// <summary>
    /// 自动根据当前平台打包打包
    /// </summary>
    /// <param name="outPutFolderPath">导出包体的根目录</param>
    private static void Build(string outPutFolderPath)
    {
        if (string.IsNullOrEmpty(outPutFolderPath))
        {
            Debug.LogError("打包失败！打包路径为空，请使用编辑器工具配置打包路径 " + EditorMenuRootPath + "/BuildConfigSetting");
            return;
        }
        string curTime = string.Format("{0:yyyy_MM_dd_HH_mm_ss}", DateTime.Now);
        appName = PlayerSettings.productName;
        //test start
        string productName = "";
        if (GameLaunch.GetInstance.scene == GameLaunch.Scenes.MainScene1)
        {
            productName = "之江仿真程序_";
        }
        else
        {
            productName = "之江数字孪生程序_"; 
        }
#if UNITY_WEBGL
        productName += "Webgl_";
        outPutFolderPath = "D:/UnityProject/JHGit/ZJLab_DigtalTwin/Assets/../_Build/_Web";
#elif UNITY_STANDALONE_LINUX
        productName += "PCLinux_";
        outPutFolderPath = "D:/UnityProject/JHGit/ZJLab_DigtalTwin/Assets/../_Build/_Linux";

#elif UNITY_STANDALONE_WIN
        productName += "PCWin_";
        outPutFolderPath = "D:/UnityProject/JHGit/ZJLab_DigtalTwin/Assets/../_Build/_Win";
#endif
        productName += curTime;
        PlayerSettings.productName = productName;
        appName = PlayerSettings.productName;
        //test end

        //curBuildOutPutAllPath = outPutFolderPath + "/" + appName + "_v" + PlayerSettings.bundleVersion + "_" + curTime;
        curBuildOutPutAllPath = outPutFolderPath + "/" + appName;
        buildEndAutoOpenFolderPath = string.Empty;

        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            //代码配置秘钥
            //PlayerSettings.Android.keystorePass = "sikiocean";
            //PlayerSettings.Android.keyaliasPass = "sikiocean";
            //PlayerSettings.Android.keyaliasName = "android.keystore";
            //PlayerSettings.Android.keystoreName = Application.dataPath.Replace("/Assets", "") + "/realfram.keystore";
            curBuildOutPutAllPath += ".apk";
            buildEndAutoOpenFolderPath = outPutFolderPath;
        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
        {

        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL)
        {
            buildEndAutoOpenFolderPath = curBuildOutPutAllPath;
        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows || EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
        {
            buildEndAutoOpenFolderPath = curBuildOutPutAllPath;
            curBuildOutPutAllPath += "/" + appName + ".exe";
        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneLinux64)
        {
            buildEndAutoOpenFolderPath = curBuildOutPutAllPath;
            curBuildOutPutAllPath += "/" + appName;
        }
        Debug.Log("curBuildOutPutAllPath :  " + curBuildOutPutAllPath);

        BuildPipeline.BuildPlayer(FindEnableEditorrScenes(), curBuildOutPutAllPath, EditorUserBuildSettings.activeBuildTarget, BuildOptions.None);


        OpenDirectory(buildEndAutoOpenFolderPath);
    }

    #region 自动化打包基于jenkins或windows command批处理命令

    /// <summary>
    /// 打包PC端
    /// 自动化打包基于jenkins或windows command批处理命令
    /// </summary>
    /// .bat命令
    //:: 此处执行Unity打包
    //echo 正在打包中请稍后...
    //"D:\UnityEngine\2021.3.20f1c1\Editor\Unity.exe" ^
    //-quit ^
    //-batchmode ^
    //-projectPath "D:\LearnPath\AutoBuildByJenkins\LearnJenkins" ^
    //-executeMethod BuildTool.BuildByCMD ^
    //-logFile "D:\AutoBuildlog.txt" ^
    //echo 打包完成
    //exit
    public static void BuildByCMD()
    {
        Debug.Log("Building......");
        SetConfigSetting();
        Build(PlayerPrefs.GetString(PlayerPrefsKeyGroup.Build_OutPutPath_CMD, BuildConfigSetting.BuildRootDefaultPath));
    }
    #endregion
    /// <summary>
    /// 设置打包参数
    /// 自动化打包基于jenkins或windows command批处理命令
    /// </summary>
    public static void SetConfigSetting()
    {
        // 解析命令行参数
        string[] args = System.Environment.GetCommandLineArgs();
        Debug.Log("------------------------------------------------------------解析CMD参数------------------------------------------------------------");
        foreach (var s in args)
        {
            Debug.Log("参数遍历 arg:" + s);
            if (s.Contains("--productName:"))
            {
                Debug.Log("--productName:" + s);
                string productName = s.Split(':')[1];
                // 设置app名字
                PlayerSettings.productName = productName;
            }
            if (s.Contains("--version:"))
            {
                Debug.Log("--version:" + s);
                string version = s.Split(':')[1];
                // 设置版本号
                PlayerSettings.bundleVersion = version;
                foreach (var item in s.Split(':'))
                {
                    Debug.Log("BBB " + item);
                }
            }
            if (s.Contains("--outPutPath:"))
            {
                Debug.Log("--outPutPath:" + s);
                string buildPath = s.Split(':')[1] + ":" + s.Split(':')[2];
                PlayerPrefs.SetString(PlayerPrefsKeyGroup.Build_OutPutPath_CMD, buildPath);
            }
        }
    }

    #region command批处理命令

    #endregion

    #region Other
    /// <summary>
    /// 获得在BuildSettings已经添加的场景
    /// </summary>
    /// <returns></returns>
    private static string[] FindEnableEditorrScenes()
    {
        List<string> editorScenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            editorScenes.Add(scene.path);

            Debug.Log(scene.path);
        }
        return editorScenes.ToArray();
    }
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

    #endregion
}
#endif