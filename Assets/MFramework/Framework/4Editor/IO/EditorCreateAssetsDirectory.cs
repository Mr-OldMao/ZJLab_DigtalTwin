#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
namespace MFramework
{
    /// <summary>
    /// 标题：编辑器工具类 自动创建Unity工程目录
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2022.12.2
    /// </summary>
    public class EditorCreateAssetsDirectory
    {
        public static string rootNode = Application.dataPath + "/GameMain/";
        /// <summary>
        /// 一级目录结构
        /// </summary>
        public static List<string> m_ListDireStruct1 = new List<string>()
        {   
            /*
            存放进ab包的资源
            */
            "AB",   
            /*
            存放非ab包的资源
            */
            "Audios",
            "Configs",//项目配置文件
            "DataTables",//txt格式数据表，由Excel表导出
            "Prefab",   //不进ab包的实体预设体
            "Fonts",
            "Materials",
            "Meshes",
            "Scenes",
            "Scripts",
            "Textures",
            "UI",
            "Videos",
            "Shader",
            "RenderTextures"
        };

        /// <summary>
        /// 二级目录结构
        /// </summary>
        public static List<string> m_ListDireStruct2 = new List<string>()
        {
            "AB/Prefab",
            "Audios/BGM",
            "Audios/Sounds",

            "Prefab/Entity",   //不进ab包的gameobject预制体
            "Prefab/UI",   //不进ab包的ui预制体

            "Scripts/Base",
            "Scripts/DataCache", //缓存数据，可读写
            "Scripts/DataConstantReadOnly", //存放项目常量（只读）
            "Scripts/DataTable", //UGF自动映射(GameMain/DataTables目录下表的)数据结构 
            "Scripts/Debugger",
            "Scripts/Editor",
            "Scripts/Entity",
            "Scripts/Event",
            "Scripts/Logic",
            "Scripts/Network",
            "Scripts/Procedure",
            "Scripts/Scene",
            "Scripts/UI",
            "Scripts/Utility",

            "UI/UIForms",//UI窗体预设
            "UI/UISprites",//UI精灵图片
        };

        /// <summary>
        /// 三级目录结构
        /// </summary>
        public static List<string> m_ListDireStruct3 = new List<string>()
        {
            "AB/Prefab/Entity", //进ab包 gameobject预制体
            "AB/Prefab/UI", //进ab包 ui预制体
            
            "Audios/Sounds/BtnSound",

            "Scripts/UI/Base", //存放UI的接口、抽象基类、UI工具

            "Scripts/Procedure/Base", //存放流程类的接口、抽象基类
            "Scripts/Procedure/0MainProcedureBefore",//进入主流程之前流程，eg：游戏入口、热更版本资源检查、资源预加载等
            "Scripts/Procedure/1MainProcedure",//主流程业务
            "Scripts/Procedure/2MainProcedureAfter",//主流程结束


        };

        public static void AutoCreateAssetsDirectory()
        {
            foreach (string direStruct in m_ListDireStruct1)
            {
                CreateDirectory(direStruct);
            }
            foreach (string direStruct in m_ListDireStruct2)
            {
                CreateDirectory(direStruct);
            }
            foreach (string direStruct in m_ListDireStruct3)
            {
                CreateDirectory(direStruct);
            }
            AssetDatabase.Refresh();
        }

        private static void CreateDirectory(string targetDirectory)
        {
            if (!string.IsNullOrEmpty(targetDirectory))
            {
                string targetDire = rootNode + targetDirectory;
                if (!Directory.Exists(targetDire))
                {
                    Directory.CreateDirectory(targetDire);
                    Debug.Log($"Create Directory Succ, targetDire：{targetDire}");
                }
                else
                {
                    Debug.Log($"Create Directory Fail, Existed targetDire：{targetDire}");
                }
            }
        }
    }
}

#endif