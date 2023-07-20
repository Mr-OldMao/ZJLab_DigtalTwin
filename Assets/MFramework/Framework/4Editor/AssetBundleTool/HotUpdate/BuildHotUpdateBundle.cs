#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MFramework
{
    /// <summary>
    /// 打包热更包
    /// </summary>
    public class BuildHotUpdateBundle : EditorWindow
    {
        public static void BuildHotUpdateBundleMethod()
        {
            BuildHotUpdateBundle window = (BuildHotUpdateBundle)EditorWindow.GetWindow(typeof(BuildHotUpdateBundle), false, "热更包界面", true);
            window.Show();
        }

        /// <summary>
        /// 历史版本ab包MD5码
        /// </summary>
        string oldMD5FilePath = "";
        /// <summary>
        /// ab包位置
        /// </summary>
        string assetsBundleDir = "";
        /// <summary>
        /// 热更包输出位置
        /// </summary>
        string exportBundleDir = "";

        private void OnEnable()
        {
            oldMD5FilePath = PlayerPrefs.GetString("md5Path", "");
            assetsBundleDir = PlayerPrefs.GetString("assetsBundleDir", Application.streamingAssetsPath);
            exportBundleDir = PlayerPrefs.GetString("exportBundleDir", HotUpdateSetting.folderFullPath_HoutUpdateBundle);
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();


            //水平布局
            GUILayout.BeginHorizontal();

            oldMD5FilePath = EditorGUILayout.TextField("历史版本ABMD5文件路径：", oldMD5FilePath, GUILayout.Width(600), GUILayout.Height(20));
            if (GUILayout.Button("浏览", GUILayout.Width(50f)))
            {
                string selectPath = EditorUtility.OpenFilePanel("ABMD5路径窗口", Application.dataPath + "/..", "");
                if (!string.IsNullOrEmpty(selectPath))
                {
                    oldMD5FilePath = selectPath;
                    PlayerPrefs.SetString("md5Path", oldMD5FilePath);
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            assetsBundleDir = EditorGUILayout.TextField("AssetsBundle文件夹路径：", assetsBundleDir, GUILayout.Width(600), GUILayout.Height(20));
            if (GUILayout.Button("浏览", GUILayout.Width(50f)))
            {
                string selectPath = EditorUtility.OpenFolderPanel("AssetsBundle文件夹路径窗口", Application.dataPath + "/..", "");
                if (!string.IsNullOrEmpty(selectPath))
                {
                    assetsBundleDir = selectPath;
                    PlayerPrefs.SetString("assetsBundleDir", assetsBundleDir);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            exportBundleDir = EditorGUILayout.TextField("热更AB包输出文件夹路径：", exportBundleDir, GUILayout.Width(600), GUILayout.Height(20));
            if (GUILayout.Button("浏览", GUILayout.Width(50f)))
            {
                string selectPath = EditorUtility.OpenFolderPanel("热更AB包输出文件夹路径窗口", Application.dataPath + "/..", "");
                if (!string.IsNullOrEmpty(selectPath))
                {
                    exportBundleDir = selectPath;
                    PlayerPrefs.SetString("exportBundleDir", exportBundleDir);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("开始打热更包", GUILayout.Width(80), GUILayout.Height(40)))
            {
                BuildHotUpdatePackage();
            }

            if (GUILayout.Button("保存配置", GUILayout.Width(80), GUILayout.Height(40)))
            {
                SaveConfig();
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        /// <summary>
        /// 生成热更包
        /// </summary>
        private void BuildHotUpdatePackage()
        {
            //读取历史ab包md5码
            ABMainData abMainData = HotUpdateTool.ReadMD5(oldMD5FilePath);
            //历史与当前的ab包 md5码比对获取热更列表
            List<ABInfo> compraeList = HotUpdateTool.MD5Comprae(abMainData.abInfoList, assetsBundleDir);
            //获取历史版本版本号
            string oldVersion = abMainData.version;
            //根据热更列表 拷贝差异ab文件到目标位置
            string exportBundleFullPath = exportBundleDir + "/newVersion_" + PlayerSettings.bundleVersion + "_oldVersion_" + oldVersion + "_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
            //拷贝差异ab包，即：热更包
            CopyABFile(compraeList, exportBundleFullPath);
            //打开热更包文件夹
            WindowsTool.OpenDirectory(exportBundleDir);
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        private void SaveConfig()
        {
            PlayerPrefs.SetString("md5Path", oldMD5FilePath);
            PlayerPrefs.SetString("assetsBundleDir", assetsBundleDir);
            PlayerPrefs.SetString("exportBundleDir", exportBundleDir);
            Debug.Log("已保存热更配置");
        }

        /// <summary>
        /// 拷贝热更包文件
        /// </summary>
        /// <param name="aBInfos"></param>
        private void CopyABFile(List<ABInfo> aBInfos, string targetCopyDir)
        {
            for (int i = 0; i < aBInfos?.Count; i++)
            {
                //xxx.bundle
                FindFile(aBInfos[i].Name, assetsBundleDir, (p) =>
                {
                    if (!Directory.Exists(targetCopyDir))
                    {
                        Directory.CreateDirectory(targetCopyDir);
                    }
                    string targetFullPath = targetCopyDir + "/" + p.Name;
                    Debug.Log("Copy File :"+targetFullPath);
                    File.Copy(p.FullName, targetFullPath);
                });
            }
        }


        /// <summary>
        /// 递归查找ab文件夹下的ab文件
        /// </summary>
        /// <param name="targetfileName"></param>
        /// <param name="folderFullPath"></param>
        /// <param name="callback"></param>
        private void FindFile(string targetfileName, string folderFullPath, Action<FileInfo> callback)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(folderFullPath);
            for (int i = 0; i < directoryInfo.GetFiles().Length; i++)
            {
                if (directoryInfo.GetFiles()[i].Name.EndsWith(".meta"))
                {
                    continue;
                }
                if (directoryInfo.GetFiles()[i].Name == targetfileName)
                {
                    //Debug.Log("差异包：targetfileName:" + targetfileName);
                    callback?.Invoke(directoryInfo.GetFiles()[i]);
                    return;
                }
            }

            for (int i = 0; i < directoryInfo.GetDirectories().Length; i++)
            {
                FindFile(targetfileName, directoryInfo.GetDirectories()[i].FullName, callback);
            }
        }
    }
}

#endif