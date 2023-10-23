#if UNITY_EDITOR
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace MFramework.Editor
{
    /// <summary>
    /// 标题：Json =》 Object，Json反序列化实体类自动生成(基于Newtonsoft.Json) 脚本自动化生成工具
    /// 目的：为解决根据Json数据进行反序列化，需要手动创建对应的对象实体进行数据缓存的问题
    /// 功能：输入Json格式的字符串，指定实体类的路径，自动生成Json反序列化所需的实体类
    /// 调用：Unity菜单栏  MFramework/脚本自动化工具/2.Json自动映射实体类
    /// 作者：毛俊峰
    /// 时间：2022.08.05
    /// 版本：1.0
    /// </summary>
    public class EditorJsonMapEntity : ScriptableWizard
    {
        public static JsonMapClassStruct jsonMapClassStruct;
        //[MenuItem("MFramework/脚本自动化工具/2.Json反序列化实体类自动生成工具", false, 2)]
        public static void CreateWizard()
        {
            DisplayWizard<EditorJsonMapEntity>("Json反序列化实体类自动生成工具", "生成", "重置");
        }
        [Header("Json格式字符串")]
        public string jsonStr;
        [Header("脚本类名 格式：Xxx")]
        public string scriptName;
        [Header("脚本路径 格式：x:/xxx/xx/xxx/")]
        public string scriptPath;
        [Header("是否需要文本注释")]
        public bool isSummaryAnnotation;
        [Header("是否覆盖同路径下的同类名脚本")]
        public bool isOverrideScript;
        /// <summary>
        /// 脚本自动化结构体
        /// </summary>
        public struct JsonMapClassStruct
        {
            /// <summary>
            /// Json格式字符串
            /// </summary>
            public string jsonStr;
            /// <summary>
            /// 脚本类名
            /// </summary>
            public string scriptName;
            /// <summary>
            /// 脚本路径
            /// </summary>
            public string scriptPath;
            /// <summary>
            /// 是否需要文本注释
            /// </summary>
            public bool isSummaryAnnotation;
            /// <summary>
            /// 是否覆盖同路径下的同类名脚本
            /// </summary>
            public bool isOverrideScript;
        }
        private void OnEnable()
        {
            Reset();
        }
        private void OnWizardCreate()
        {
            jsonMapClassStruct = new JsonMapClassStruct
            {
                jsonStr = jsonStr,
                scriptName = scriptName,
                scriptPath = scriptPath,
                isSummaryAnnotation = isSummaryAnnotation,
                isOverrideScript = isOverrideScript
            };
            AutoCreateScriptImpl.BuildScript();

            //try
            //{
            //    AutoCreateScriptImpl.BuildScript();
            //}
            //catch (System.Exception)
            //{
            //    Debug.Log("脚本自动化生成工具运行失败! Json数据 格式错误 请检查 jsonStr：" + jsonMapClassStruct.jsonStr);
            //}
        }
        private void OnWizardOtherButton()
        {
            Reset();
        }
        private void OnWizardUpdate()
        {
            helpString = null;
            errorString = null;
            if (string.IsNullOrEmpty(jsonStr))
            {
                //给出错误提示
                errorString = "请输入Json";
            }
            else
            {
                if (string.IsNullOrEmpty(scriptName))
                {
                    errorString = "请填写脚本类名";
                }
                else if (string.IsNullOrEmpty(scriptPath))
                {
                    errorString = "请填写脚本路径";
                }
                else
                {
                    helpString = "点击“生成”尝试创建Json实体类";
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

        private void Reset()
        {
            jsonStr = string.Empty;
            scriptName = string.Empty;
            scriptPath = EditorMenu.autoCreateScriptDefaultPath + "JsonMapEntity/";
            isSummaryAnnotation = false;
            isOverrideScript = false;
        }

        class AutoCreateScriptImpl
        {
            /// <summary>
            /// 映射内容  key-自定义类名  value-类结构
            /// </summary>
            private static Dictionary<string, ClassInfo> dicClassContent = new Dictionary<string, ClassInfo>();

            private class ClassInfo
            {
                public string classHead;
                public string classBody;
                public string classEnd;
            }

            public static void BuildScript()
            {
                JsonMapClassStruct jsonMapClassStruct = EditorJsonMapEntity.jsonMapClassStruct;
                DeserializeObject(jsonMapClassStruct.jsonStr, jsonMapClassStruct.scriptName);
                string scriptContent = "";
                foreach (var item in dicClassContent)
                {
                    string subContent = item.Value.classHead + item.Value.classBody + item.Value.classEnd;
                    scriptContent += subContent;
                    //Debug.Log("k: " + item.Key + "，v：" + subContent);
                }
                scriptContent = AutoCreateScripttestTemplate.classtestTemplateHead(jsonMapClassStruct.scriptName, scriptContent);
                WriteFile(jsonMapClassStruct, scriptContent);
            }

            //private static int testTemp = 1;
            /// <summary>
            /// 反序列化 json => Obj (核心方法)
            /// </summary>
            /// <param name="jsonStr"></param>
            /// <param name="curClassName">当前json包含的字段的类名</param>
            private static void DeserializeObject(string jsonStr, string curClassName)
            {
                Debug.Log(jsonStr + ",curClassName:" + curClassName);
                JObject jObject = null;
                try
                {
                    jObject = (JObject)JsonConvert.DeserializeObject(jsonStr);//将json字符串转化为JObject
                }
                catch (System.Exception e)
                {
                    Debug.LogError("子json转换实体类失败 e:" + e);
                    return;
                }

                foreach (var obj in jObject)
                {
                    //Debug.Log("testTemp:" + testTemp + ",item.Key:" + obj.Key + " ,item.Value.Type:" + obj.Value.Type + ",item.Value : " + obj.Value);
                    if (obj.Value.Type == JTokenType.Object)
                    {
                        string fieldName = obj.Key;
                        string fieldType = curClassName + "_" + obj.Key;
                        string nextJson = obj.Value.ToString();
                        string nextClassName = fieldType;
                        //testTemp++;
                        //Debug.Log("testTemp:" + testTemp + "，fieldName：" + fieldName + "，fieldContent：" + obj.Value + "，type：" + obj.Value.Type + "，fieldType：" + fieldType);
                        SpliceStr(curClassName, fieldName, fieldType);
                        DeserializeObject(nextJson, nextClassName);
                    }
                    else if (obj.Value.Type == JTokenType.Array)
                    {
                        //testTemp++;
                        //Debug.Log("testTemp:" + testTemp + ",type:" + obj.Value.Type + ",k:" + obj.Key + ",v:" + obj.Value + ",subArrJson：" + obj.Value.ToString());
                        //遍历数组
                        foreach (var arrItem in obj.Value)
                        {
                            string fieldName = obj.Key;
                            //Debug.Log("testTemp:" + testTemp + "，ArrElemType:" + arrItem.Type + "，ArrElemValue" + arrItem);
                            if (arrItem.Type == JTokenType.Object || arrItem.Type == JTokenType.Array)
                            {
                                string fieldType = curClassName + "_" + obj.Key + "[]";
                                string nextJson = arrItem.ToString();
                                string nextClassName = curClassName + "_" + obj.Key;
                                //testTemp++;
                                //Debug.Log("testTemp:" + testTemp + "，fieldName：" + fieldName + "，fieldContent：" + obj.Value + "，type：" + obj.Value.Type + "，fieldType：" + fieldType);
                                SpliceStr(curClassName, fieldName, fieldType);
                                DeserializeObject(nextJson, nextClassName);
                            }
                            else
                            {
                                string fieldType = GetFieldTypeByJTokenType(fieldName, arrItem.Type) + "[]";
                                //Debug.Log("testTemp:" + testTemp + "，fieldName：" + fieldName + "，fieldContent：" + obj.Value + "，type：" + obj.Value.Type + "，fieldType：" + fieldType);
                                SpliceStr(curClassName, fieldName, fieldType);
                            }
                            break;//仅遍历一次
                        }
                    }
                    else
                    {
                        string fieldName = obj.Key;
                        string fieldType = GetFieldTypeByJTokenType(fieldName, obj.Value.Type);
                        //Debug.Log("testTemp:" + testTemp +  "，fieldName：" + fieldName + "，fieldContent：" + obj.Value+"，type：" + obj.Value.Type + "，fieldType："+ fieldType);
                        SpliceStr(curClassName, fieldName, fieldType);
                    }

                }
            }
            /// <summary>
            /// 拼接字符串
            /// </summary>
            /// <param name="subClassName">类名</param>
            /// <param name="fieldName">字段名</param>
            /// <param name="fieldType">字段类型</param>
            private static void SpliceStr(string subClassName, string fieldName, string fieldType)
            {
                if (!dicClassContent.ContainsKey(subClassName))
                {
                    ClassInfo classInfo = new ClassInfo();
                    if (subClassName == jsonMapClassStruct.scriptName)
                    {
                        classInfo.classHead = "";
                        classInfo.classEnd = "";
                    }
                    else
                    {
                        //类名首字母大写
                        string changeClassName = subClassName;
                        classInfo.classHead = "\n\tpublic class " + changeClassName + "\n\t{";
                        classInfo.classEnd = "\n\t}";
                    }
                    classInfo.classBody = "";
                    dicClassContent.Add(subClassName, classInfo);
                }
                //字段缩进格式   class缩进一次 subClass缩进两次
                string format = subClassName == jsonMapClassStruct.scriptName ? "\n\t" : "\n\t\t";
                dicClassContent[subClassName].classBody += AutoCreateScripttestTemplate.FieldtestTemplate(fieldName, fieldType, format);
            }

            /// <summary>
            /// 获取字段类型 根据JTokenType映射c#数据类型
            /// </summary>
            /// <param name="jTokenType"></param>
            /// <returns></returns>
            private static string GetFieldTypeByJTokenType(string fieldName, JTokenType jTokenType)
            {
                string fieldTypeStr = "";
                switch (jTokenType)
                {
                    case JTokenType.Integer:
                        fieldTypeStr = "int";
                        break;
                    case JTokenType.Float:
                        fieldTypeStr = "float";
                        break;
                    case JTokenType.String:
                        fieldTypeStr = "string";
                        break;
                    case JTokenType.Boolean:
                        fieldTypeStr = "bool";
                        break;
                    case JTokenType.Bytes:
                        fieldTypeStr = "byte";
                        break;
                    case JTokenType.Null:
                        fieldTypeStr = "object";
                        Debug.LogError("fieldName：" + fieldName + "，jTokenType：" + jTokenType + "，未知类型自动转为 object");
                        break;
                    case JTokenType.None:
                    case JTokenType.Object:
                    case JTokenType.Array:
                    case JTokenType.Constructor:
                    case JTokenType.Property:
                    case JTokenType.Comment:
                    case JTokenType.Undefined:
                    case JTokenType.Date:
                    case JTokenType.Raw:
                    case JTokenType.Guid:
                    case JTokenType.Uri:
                    case JTokenType.TimeSpan:
                        Debug.LogError("反序列化 不支持当前类型请检查  jTokenType：" + jTokenType);
                        break;
                    default:
                        break;
                }
                return fieldTypeStr;
            }

            /// <summary>
            /// 写入本地
            /// </summary>
            /// <param name="jsonMapClassStruct"></param>
            /// <param name="scriptContent"></param>
            private static void WriteFile(JsonMapClassStruct jsonMapClassStruct, string scriptContent)
            {
                string scriptAllPath = jsonMapClassStruct.scriptPath + jsonMapClassStruct.scriptName + ".cs";
                //写入本地
                //判定文件夹
                if (!Directory.Exists(jsonMapClassStruct.scriptPath))
                {
                    Directory.CreateDirectory(jsonMapClassStruct.scriptPath);
                }
                if (!Directory.Exists(jsonMapClassStruct.scriptPath))
                {
                    Debug.LogError("脚本自动化失败 脚本路径不合法 scriptPath：" + jsonMapClassStruct.scriptPath);
                    return;
                }
                //脚本已存在
                if (File.Exists(scriptAllPath))
                {
                    //不允许覆盖
                    if (!jsonMapClassStruct.isOverrideScript)
                    {
                        Debug.LogError("脚本自动化失败 脚本已存在且不允许覆盖 scriptAllPath：" + scriptAllPath + "，isOverrideScript：" + jsonMapClassStruct.isOverrideScript);
                        return;
                    }
                }
                FileStream file = new FileStream(scriptAllPath, FileMode.Create); //FileMode.Create 新建新的文件 如果文件存在，则直接覆盖    FileMode.CreateNew 新建新的文件 如果文件存在，则产生异常
                StreamWriter fileW = new StreamWriter(file, System.Text.Encoding.UTF8);
                fileW.Write(scriptContent);
                fileW.Flush();
                fileW.Close();
                file.Close();
                Debug.Log("脚本自动化成功 scriptName：" + jsonMapClassStruct.scriptName + "，scriptAllPath：" + scriptAllPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }



        /// <summary>
        /// 自动生成代码模板
        /// </summary>
        public class AutoCreateScripttestTemplate
        {
            /// <summary>
            /// 头节点模板
            /// </summary>
            /// <param name="className"></param>
            /// <param name="content"></param>
            /// <returns></returns>
            public static string classtestTemplateHead(string className, string content)
            {
                string res =
@"/// <summary>
/// Json映射的实体类 以下代码自动生成
/// 接口地址：
/// 创建时间：" + System.DateTime.Now + @"
/// </summary>
public class " + className + "\n{  " + content + "\n}";
                return res;
            }

            /// <summary>
            /// 字段模板
            /// </summary>
            /// <param name="fieldName">字段名称</param>
            /// <param name="fieldType">字段类型</param>
            /// <param name="format">字段格式</param>
            /// <returns></returns>
            public static string FieldtestTemplate(string fieldName, string fieldType, string format)
            {
                string res = string.Empty;
                if (jsonMapClassStruct.isSummaryAnnotation)
                {
                    res += format + "/// <summary>";
                    res += format + "/// ";
                    res += format + "/// </summary>";
                }
                res += format + "public " + fieldType + " " + fieldName + ";";
                return res;
            }
        }
    }


}
#endif