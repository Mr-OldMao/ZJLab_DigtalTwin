using UnityEngine;

namespace MFramework
{
    /// <summary>
    /// 标题：测试基于Litjson的Json序列化和反序列化
    /// 参考：litjson源码参考 https://github.com/XINCGer/LitJson4Unity/tree/master
    /// 作者：毛俊峰
    /// 时间：2023.05.19
    /// </summary>
    public class TestLitJson : MonoBehaviour
    {
        void Start()
        {

            //反序列化 .Asset文件（实体类）转json信息
            TestScriptableObj asset = GetAssetFile();
            if (asset != null)
            {
                string jsonStr1 = JsonTool.GetInstance.ObjectToJsonStringByLitJson(asset);
                Debug.Log("反序列化 .Asset文件（实体类）转json信息 jsonStr：" + jsonStr1);
            }

            //json信息转实体类
            string jsonStr2 = GetJson();
            if (!string.IsNullOrEmpty(jsonStr2))
            {
                //json字符串转 实体类
                TestScriptableObj obj = JsonTool.GetInstance.JsonToObjectByLitJson<TestScriptableObj>(jsonStr2);
                Debug.Log("json字符串转 实体类 " + obj);
#if UNITY_EDITOR
                //生成实体类对应.Assets文件
                UnityEditor.AssetDatabase.CreateAsset(obj, "Assets/_TestScriptableObj.asset");
                UnityEditor.AssetDatabase.Refresh();
                Debug.Log("写入实体类对应.Assets文件 path:Assets/_TestScriptableObj.asset"); 
#endif
            }
        }


        private TestScriptableObj GetAssetFile()
        {
            TestScriptableObj asset = null;

#if UNITY_EDITOR
            string assetPath = "Assets/MFramework/Example/ExampleScripts/Json/TestScriptableObj.asset";
            if (System.IO.File.Exists(assetPath))
            {
                asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TestScriptableObj>(assetPath);
            }
            else
            {
                Debug.LogError("asset is null,assetPath:" + assetPath);
            }
#endif
            return asset;
        }


        private string GetJson()
        {
            string jsonStr = string.Empty;
#if UNITY_EDITOR
            string jsonFilePath = "Assets/MFramework/Example/ExampleScripts/Json/TestScriptableObj.json";
            if (System.IO.File.Exists(jsonFilePath))
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(jsonFilePath))
                {
                    jsonStr = sr.ReadToEnd();
                }
            }
            else
            {
                Debug.LogError("json is null,assetPath:" + jsonStr);
            }
#endif
            //test jsonStr
            //string jsonStr = "{\"name\":\"TestScriptableObj\",\"hideFlags\":0,\"num1\":111,\"num2\":222,\"v2\":{\"x\":3.40000009536743,\"y\":5.59999990463257},\"v3\":{\"x\":1.20000004768372,\"y\":3.40000009536743,\"z\":5.59999990463257},\"quaternion\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":0.0},\"color1\":{\"r\":0.943396210670471,\"g\":0.307048767805099,\"b\":0.307048767805099,\"a\":0.0},\"color2\":{\"r\":46,\"g\":206,\"b\":238,\"a\":0},\"bounds\":{\"center\":{\"x\":1.0,\"y\":2.0,\"z\":3.0},\"size\":{\"x\":8.0,\"y\":10.0,\"z\":12.0}},\"rect\":{\"x\":1.0,\"y\":2.0,\"width\":3.0,\"height\":4.0},\"animationCurve\":{\"keys\":[{\"time\":0,\"value\":0,\"inTangent\":2,\"outTangent\":2,\"inWeight\":0,\"outWeight\":0,\"weightedMode\":0,\"tangentMode\":0},{\"time\":1,\"value\":1,\"inTangent\":0,\"outTangent\":0,\"inWeight\":0,\"outWeight\":0,\"weightedMode\":0,\"tangentMode\":0}],\"length\":2,\"preWrapMode\":8,\"postWrapMode\":8}}";

            return jsonStr;
        }
    }
}
