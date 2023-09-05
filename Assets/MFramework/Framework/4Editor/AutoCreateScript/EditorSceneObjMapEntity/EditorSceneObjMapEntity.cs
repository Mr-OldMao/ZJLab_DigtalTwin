#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

namespace MFramework.Editor
{
    /// <summary>
    /// 标题：Scene GameObject => EntityClass 场景游戏对象转实体类 
    /// 目的：解决手动获取场景对象实例需要繁琐且重复的操作
    /// 功能：脚本自动化生成工具，根据场景中某游戏对象下的所有子对象，创建实体并与之对应，自动映射与关联
    /// 调用：选中菜单MFramework/脚本自动化工具，拖入根节点，设置脚本路径，点击生成，可自动获取根节点其下字段、属性、映射并生成脚本到指定路径 
    /// 规则：场景游戏对象会被直接引用成为字段名，所以命名要规范，并且不要使用创建场景对象时引擎默认的名称会被屏蔽
    /// 作者：毛俊峰
    /// 时间：2022.08.01
    /// 版本：1.0
    /// </summary>
    public class EditorSceneObjMapEntity : ScriptableWizard
    {
        //[MenuItem("MFramework/脚本自动化工具/1.场景游戏对象映射实体类", false, 1)]
        public static void CreateWizard()
        {
            DisplayWizard<EditorSceneObjMapEntity>("一键生成游戏对象映射实体类", "生成且关闭", "生成不关闭");
        }
        [Header("根节点")]
        public GameObject root;
        [Header("脚本类名 格式：Xxx")]
        public string scriptName;
        [Header("基类类名")]
        public string baseClassName;
        //[Header("脚本路径,Assets目录下子路径 格式：/xxx/xx/xxx/")]
        private string scriptPath;

        [Header("是否创建根节点旗下未激活的游戏对象")]
        public bool isCreateHideObj;
        [Header("是否覆盖同路径下的同类名脚本")]
        public bool isOverrideScript;

        private void Reset()
        {
            root = null;
            scriptName = string.Empty;
            scriptPath = EditorPrefs.GetString("scriptPath", Application.dataPath);
            baseClassName = EditorPrefs.GetString("baseClassName", "UIFormBase");
            isCreateHideObj = EditorPrefs.GetBool("isCreateHideObj", true);
            isOverrideScript = EditorPrefs.GetBool("isOverrideScript", false);
        }

        /// <summary>
        /// 生成且关闭 btn2
        /// </summary>
        private void OnWizardCreate()
        {
            CreateScrits();
        }

        /// <summary>
        /// 生成不关闭 btn1
        /// </summary>
        private void OnWizardOtherButton()
        {
            CreateScrits();
        }

        private void CreateScrits()
        {
            AutoCreateScriptStructInfo autoCreateScriptStructInfo = new AutoCreateScriptStructInfo
            {
                root = root,
                scriptName = scriptName,
                scriptPath = scriptPath,
                isCreateHideObj = isCreateHideObj,
                baseClassName = baseClassName,
                isOverrideScript = isOverrideScript
            };
            AutoCreateScriptImpl.BuildUIScript(autoCreateScriptStructInfo, (b) =>
            {
                if (b)
                {
                    EditorPrefs.SetString("scriptPath", scriptPath);
                    EditorPrefs.SetString("baseClassName", baseClassName);
                    EditorPrefs.SetBool("isCreateHideObj", isCreateHideObj);
                    EditorPrefs.SetBool("isOverrideScript", isOverrideScript);
                }
            });
        }

        /// <summary>
        /// 当前字段值修改时调用
        /// </summary>
        private void OnWizardUpdate()
        {
            helpString = null;
            errorString = null;

            if (Selection.activeGameObject != null)
            {
                if (root != Selection.activeGameObject)
                {
                    root = Selection.activeGameObject;
                    scriptName = Selection.activeGameObject.name;
                    scriptPath = EditorPrefs.GetString("scriptPath", Application.dataPath);
                }
                helpString = "AutoCreateScriptTargetPath：\n" + Application.dataPath + scriptPath + scriptName + ".cs";
            }
            else
            {
                if (!root)
                {
                    //给出错误提示
                    errorString = "请选择根节点作为Root";
                }
                else if (string.IsNullOrEmpty(scriptName))
                {
                    errorString = "请填写脚本类名 ScriptName";
                }
            }
        }


        /// <summary>
        /// 当鼠标选中的物体发生改变时调用此方法
        /// </summary>
        private void OnSelectionChange()
        {
            OnWizardUpdate();
        }


        protected override bool DrawWizardGUI()
        {
            base.DrawWizardGUI();
            //水平布局
            GUILayout.BeginVertical(new[] { GUILayout.Width(10), GUILayout.Height(100) });
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(new[] { GUILayout.Height(10) });
            {
                GUILayout.Label("脚本路径,格式：X:/x/x/x/", GUILayout.Width(147f));
                scriptPath = GUILayout.TextField(scriptPath);
                if (GUILayout.Button("浏览", GUILayout.Width(50f)))
                {
                    string selectPath = EditorUtility.OpenFolderPanel("脚本自动化窗口", Application.dataPath, "请选择文件夹");
                    if (!string.IsNullOrEmpty(selectPath))
                    {
                        scriptPath = selectPath + "/";
                    }
                }
            }
            GUILayout.EndHorizontal();
            return default;
        }

        /// <summary>
        /// 脚本自动化生成具体实现
        /// </summary>
        public class AutoCreateScriptImpl
        {
            //拼接字段
            static string fieldContent;
            //拼接属性
            static string propertyContent;
            //拼接字段映射
            static string fieldMapContent;
            //拼接UI事件
            static string uiEventContent;

            private static void ResetContent()
            {
                fieldContent = string.Empty;
                propertyContent = string.Empty;
                fieldMapContent = string.Empty;
                propertyContent += "public override UILayerType GetUIFormLayer { get => UILayerType.Common; protected set => _ = UILayerType.Common; }";
                propertyContent += "\n\tpublic override string AssetPath { get => AssetPathRootDir + \"/xxx.prefab\"; protected set => _ = AssetPathRootDir + \"/xxx.prefab\"; }\n\t";
                fieldMapContent += "protected override void InitMapField()\n\t{\n";
            }

            [MenuItem("生成/根据字符串生成枚举类型")]
            public static void BuildUIScript(AutoCreateScriptStructInfo autoCreateScriptStructInfo, Action<bool> callback)
            {
                ResetContent();
                string scriptPath = autoCreateScriptStructInfo.scriptPath;
                string scriptName = autoCreateScriptStructInfo.scriptName;
                if (!autoCreateScriptStructInfo.root)
                {
                    Debug.LogError("脚本自动化失败 sceneGameObject is null");
                    callback?.Invoke(false);
                    return;
                }
                if (string.IsNullOrEmpty(scriptName))//todo  后期通过正则表达式验证类名
                {
                    Debug.LogError("脚本自动化失败 scriptName is null");
                    callback?.Invoke(false);
                    return;
                }
                if (string.IsNullOrEmpty(scriptPath))
                {
                    Debug.LogError("脚本自动化失败 scriptPath is null");
                    callback?.Invoke(false);
                    return;
                }
                string scriptAllPath = scriptPath + scriptName + ".cs";

                AutoCreateScriptTemplate.rootTrans = autoCreateScriptStructInfo.root.transform;
                UnityComponent unityComponent = new UnityComponent();
                unityComponent.SaveAllUIComponentContainerOnly(autoCreateScriptStructInfo.root.transform, autoCreateScriptStructInfo.isCreateHideObj);
                bool useTMProNamespace = false;
                //场景对象实例集合
                List<object> listContainer = unityComponent.GetListComponentContainer();
                foreach (var dic in listContainer)
                {
                    if (dic.GetType() == typeof(Dictionary<string, Button>))
                    {
                        Dictionary<string, Button> dicText = dic as Dictionary<string, Button>;
                        foreach (var item in dicText)
                        {
                            //string type = item.Value.GetType().ToString();
                            SpliceStr(item.Key, "Button");
                        }
                        continue;
                    }
                    if (dic.GetType() == typeof(Dictionary<string, Toggle>))
                    {
                        Dictionary<string, Toggle> dicChild = dic as Dictionary<string, Toggle>;
                        foreach (var item in dicChild)
                        {
                            SpliceStr(item.Key, "Toggle");
                        }
                        continue;
                    }
                    if (dic.GetType() == typeof(Dictionary<string, Text>))
                    {
                        Dictionary<string, Text> dicChild = dic as Dictionary<string, Text>;
                        foreach (var item in dicChild)
                        {
                            SpliceStr(item.Key, "Text");
                        }
                        continue;
                    }
                    if (dic.GetType() == typeof(Dictionary<string, Image>))
                    {
                        Dictionary<string, Image> dicChild = dic as Dictionary<string, Image>;
                        foreach (var item in dicChild)
                        {
                            SpliceStr(item.Key, "Image");
                        }
                        continue;
                    }
                    if (dic.GetType() == typeof(Dictionary<string, Slider>))
                    {
                        Dictionary<string, Slider> dicChild = dic as Dictionary<string, Slider>;
                        foreach (var item in dicChild)
                        {
                            SpliceStr(item.Key, "Slider");
                        }
                        continue;
                    }
                    if (dic.GetType() == typeof(Dictionary<string, ScrollRect>))
                    {
                        Dictionary<string, ScrollRect> dicChild = dic as Dictionary<string, ScrollRect>;
                        foreach (var item in dicChild)
                        {
                            SpliceStr(item.Key, "ScrollRect");
                        }
                        continue;
                    }
                    if (dic.GetType() == typeof(Dictionary<string, Scrollbar>))
                    {
                        Dictionary<string, Scrollbar> dicChild = dic as Dictionary<string, Scrollbar>;
                        foreach (var item in dicChild)
                        {
                            SpliceStr(item.Key, "Scrollbar");
                        }
                        continue;
                    }
                    if (dic.GetType() == typeof(Dictionary<string, InputField>))
                    {
                        Dictionary<string, InputField> dicChild = dic as Dictionary<string, InputField>;
                        foreach (var item in dicChild)
                        {
                            SpliceStr(item.Key, "InputField");
                        }
                        continue;
                    }
                    if (dic.GetType() == typeof(Dictionary<string, TMP_InputField>))
                    {
                        Dictionary<string, TMP_InputField> dicChild = dic as Dictionary<string, TMP_InputField>;
                        foreach (var item in dicChild)
                        {
                            useTMProNamespace = true;
                            SpliceStr(item.Key, "TMP_InputField");
                        }
                        continue;
                    }
                    if (dic.GetType() == typeof(Dictionary<string, TextMeshProUGUI>))
                    {
                        Dictionary<string, TextMeshProUGUI> dicChild = dic as Dictionary<string, TextMeshProUGUI>;
                        foreach (var item in dicChild)
                        {
                            useTMProNamespace = true;
                            SpliceStr(item.Key, "TextMeshProUGUI");
                        }
                        continue;
                    }
                    if (dic.GetType() == typeof(Dictionary<string, Transform>))
                    {
                        Dictionary<string, Transform> dicChild = dic as Dictionary<string, Transform>;
                        foreach (var item in dicChild)
                        {
                            SpliceStr(item.Key, "Transform");
                        }
                        continue;
                    }
                }
                fieldMapContent += "}\n\t";
                fieldMapContent += "\n\t";
                fieldMapContent += "protected override void RegisterUIEvnet()\n\t{\n " + uiEventContent + "\t}\n\t";

                string classInfo = AutoCreateScriptTemplate.classTemplate;
                classInfo = classInfo.Replace("{命名空间}", useTMProNamespace ? "using TMPro;\n" : string.Empty);
                classInfo = classInfo.Replace("{头注释}", "标题：UI窗体视图层(当前代码都是通过脚本自动生成的)\n/// 功能：\n/// 作者：毛俊峰\n/// 时间：" + System.DateTime.Now.ToString("yyyy.MM.dd"));
                classInfo = classInfo.Replace("{类名}", scriptName);
                classInfo = classInfo.Replace("{: 基类}", string.IsNullOrEmpty(autoCreateScriptStructInfo.baseClassName) ? "" : ": " + autoCreateScriptStructInfo.baseClassName);
                classInfo = classInfo.Replace("{字段}", fieldContent);
                classInfo = classInfo.Replace("{属性}", propertyContent);
                classInfo = classInfo.Replace("{初始化}", autoCreateScriptStructInfo.baseClassName == "UIFormBase" ? AutoCreateScriptTemplate.awakeOverMethodTemplate : AutoCreateScriptTemplate.awakeMethodTemplate);
                classInfo = classInfo.Replace("{方法}", fieldMapContent);

                //写入本地
                //判定文件夹
                if (!Directory.Exists(scriptPath))
                {
                    Directory.CreateDirectory(scriptPath);
                }
                if (!Directory.Exists(scriptPath))
                {
                    Debug.LogError("脚本自动化失败 脚本路径不合法 scriptPath：" + scriptPath);
                    return;
                }

                //脚本已存在
                if (File.Exists(scriptAllPath))
                {
                    //不允许覆盖
                    if (!autoCreateScriptStructInfo.isOverrideScript)
                    {
                        Debug.LogError("脚本自动化失败 脚本已存在且不允许覆盖 scriptAllPath：" + scriptAllPath + "，isOverrideScript：" + autoCreateScriptStructInfo.isOverrideScript);
                        return;
                    }
                }
                FileStream file = new FileStream(scriptAllPath, FileMode.Create); //FileMode.Create 新建新的文件 如果文件存在，则直接覆盖    FileMode.CreateNew 新建新的文件 如果文件存在，则产生异常
                StreamWriter fileW = new StreamWriter(file, System.Text.Encoding.UTF8);
                fileW.Write(classInfo);
                fileW.Flush();
                fileW.Close();
                file.Close();
                Debug.Log("脚本自动化成功 scriptName：" + scriptName + "，scriptAllPath：" + scriptAllPath);
                Application.OpenURL(scriptAllPath);
                callback?.Invoke(true);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            /// <summary>
            /// 拼接字符串
            /// </summary>
            /// <param name="fieldName"></param>
            /// <param name="fieldType"></param>
            private static void SpliceStr(string fieldName, string fieldType)
            {
                string propertyName = fieldName.Substring(0, 1).ToUpper() + fieldName.Substring(1);
                fieldContent += AutoCreateScriptTemplate.FieldTemplate(fieldName, fieldType);
                propertyContent += AutoCreateScriptTemplate.PropertyTemplate(fieldType, propertyName, fieldName);
                fieldMapContent += AutoCreateScriptTemplate.FieldMapTemplate(fieldName, fieldType);
                uiEventContent += AutoCreateScriptTemplate.RegisterUIEvent(fieldName, fieldType);
            }
        }

        /// <summary>
        /// 自动生成代码模板
        /// </summary>
        public class AutoCreateScriptTemplate
        {
            [SerializeField]
            public static Transform rootTrans;
            /// <summary>
            /// 头节点模板
            /// </summary>
            public static string classTemplate =
    @"using UnityEngine;
using UnityEngine.UI;
using MFramework;
{命名空间}
/// <summary>
/// {头注释}
/// </summary>
public class {类名} {: 基类}
{
    {字段}
    
    {属性}

    {初始化}
    
    {方法}
}
";
            public static string awakeOverMethodTemplate =
  @"protected override void Awake()
    {
        base.Awake();
        InitMapField();
    }";
            public static string awakeMethodTemplate =
  @"private void Awake()
    {
        InitMapField();
    }";


            /// <summary>
            /// 字段模板
            /// </summary>
            /// <param name="fieldName"></param>
            /// <param name="fieldType"></param>
            /// <returns></returns>
            public static string FieldTemplate(string fieldName, string fieldType)
            {
                return "[SerializeField]\n\tprivate " + fieldType + " " + fieldName + ";\n\t";
            }

            /// <summary>
            /// 属性模板
            /// </summary>
            /// <param name="returnStr"></param>
            /// <param name="PropertyStr"></param>
            /// <param name="fieldStr"></param>
            /// <returns></returns>
            public static string PropertyTemplate(string returnStr, string PropertyStr, string fieldStr)
            {
                return "public " + returnStr + " " + PropertyStr + " { get => " + fieldStr + "; set => " + fieldStr + " = value; }\n\t";
            }

            /// <summary>
            /// 字段映射模板
            /// </summary>
            /// <param name="returnStr"></param>
            /// <param name="PropertyStr"></param>
            /// <param name="fieldStr"></param>
            /// <returns></returns>
            public static string FieldMapTemplate(string fieldName, string fieldType)
            {
                return "\t\t" + @fieldName + @" = transform.Find<" + fieldType + ">(\"" + fieldName + "\");\n";
            }

            public static string RegisterUIEvent(string fieldName, string fieldType)
            {
                string res = string.Empty;
                switch (fieldType)
                {
                    case "Button":
                        res = "\t\t" + fieldName + ".onClick.AddListenerCustom(() =>\r\n\t\t{\r\n\t\t\t" +
                            "Debug.Log(\"button click " + fieldName + "\");" +
                            "\r\n\t\t});\n";
                        break;
                    case "Toggle":
                        res = "\t\t" + fieldName + ".onValueChanged.AddListenerCustom((ison) =>\r\n\t\t{\r\n\t\t\t" +
                            "Debug.Log(\"toggle change " + fieldName + "，Ison：\"+ ison);" +
                            "\r\n\t\t});\n";
                        break;
                    default:
                        break;
                }
                return res;
            }
        }
    }

    /// <summary>
    /// 脚本自动化结构体
    /// </summary>
    public struct AutoCreateScriptStructInfo
    {
        /// <summary>
        /// 场景目标父对象
        /// </summary>
        public GameObject root;
        /// <summary>
        /// 脚本类名
        /// </summary>
        public string scriptName;
        /// <summary>
        /// 基类类名
        /// </summary>
        public string baseClassName;
        /// <summary>
        /// 脚本路径
        /// </summary>
        public string scriptPath;
        /// <summary>
        /// 是否创建未激活的游戏对象字段
        /// </summary>
        public bool isCreateHideObj;
        /// <summary>
        /// 是否覆盖同路径下的同类名脚本
        /// </summary>
        public bool isOverrideScript;
    }
}

#endif