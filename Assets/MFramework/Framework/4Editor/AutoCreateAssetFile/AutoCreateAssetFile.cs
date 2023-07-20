#if UNITY_EDITOR
#pragma warning disable IDE0051 // 删除未使用的私有成员
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using Object = UnityEngine.Object;
using System.Linq;
using System.Collections.Generic;

namespace MFramework
{
    /// <summary>
    /// 标题：编辑器工具，自动生成脚本对应的.asset文件
    /// 功能：右键脚本（脚本需要继承ScriptableObject）自动生成.asset文件 
    /// 作者：毛俊峰
    /// 时间：2023.05.19
    /// </summary>
    static class AutoCreateAssetFile
    {
        static List<ScriptableObject> list = new List<ScriptableObject>();
        [MenuItem("Assets/Create/创建.asset资源文件", priority = 1)]
        public static void Create()
        {
            list.Clear();
            foreach (var item in Selection.objects)
            {
                list.Add(CreateAsset(item));
            }
            Selection.objects = list.ToArray();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
        }

        //[MenuItem("Assets/Create/ScriptableObject Asset", true)]
        public static bool Validate()
        {
            Func<Object, bool> predicate = (obj) =>
            {
                if (obj is MonoScript)
                {
                    var ms = obj as MonoScript;
                    var type = ms.GetClass();
                    if (null != type)
                    {
                        var valid = type.IsSubclassOf(typeof(ScriptableObject));
                        Debug.LogError(valid + $",创建失败：选择了错误的类型 → {type}");
                        return !valid;
                    }
                    Debug.LogError($"创建失败: 请避免选择静态类型和没写继承关系的patial类型 → {ms.name}");
                }
                return true;
            };
            return Selection.objects.Length > 0 && !Selection.objects.Any(predicate);
        }
        static ScriptableObject CreateAsset(Object ms)
        {
            var path = AssetDatabase.GetAssetPath(ms);
            path = path.Substring(0, path.LastIndexOf("/"));
            path = Path.Combine(path, "AssetData");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            ScriptableObject asset = null;
            try
            {
                Type type = (ms as MonoScript).GetClass();
                string asseetPath = $"{path}/ {type.Name}.asset";
                path = AssetDatabase.GenerateUniqueAssetPath(asseetPath);
                asset = ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                if (asset != null)
                {
                    Debug.Log("Create .asset Complete , asseetPath：" + asseetPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Create .asset Fail , curSelected：" + ms);
            }

            return asset;
        }
    }
} 
#endif