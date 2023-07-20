using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：测试UI提示框
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2022.06.10
    /// 版本：1.0
    /// </summary>
    public class TestUIHintWindow : MonoBehaviour
    {
        private void Start()
        {
            Debug.LogError("演示UIHintWindow窗体资源的加载、自定义按钮事件、文本设置，需要先导入Unity资源包后，再取消注释后续代码即可。UnityPackagePath:Assets/MFramework/Example/AssetsUnityPackage/ExampleAssetsUIHintWindow.unitypackage");

            ////文本,自动销毁时间，自动销毁后回调，背景透明遮罩
            //LoadUIHintWindows.GetInstance.LoadHintLogInfo("简单日志测试文本");
            //LoadUIHintWindows.GetInstance.LoadHintLogInfo("简单日志测试文本 3s后自动消失", 3f, () => { Debug.Log("简单日志 3s后自动消失事件"); });
            ////文本结构(string数组或者TextSetting结构体)，自动销毁时间，自动销毁后回调，背景透明遮罩
            //LoadUIHintWindows.GetInstance.LoadHintBorderNotBtn(new string[] { "无按钮内容文本测试123", "无按钮窗体标题" }, -1);
            //LoadUIHintWindows.GetInstance.LoadHintBorderNotBtn(new string[] { "无按钮内容文本测试123", "无按钮窗体标题" }, 5f, () => { Debug.Log("无按钮自动消失事件"); });
            ////文本结构(string数组或者TextSetting结构体)，按钮文本，按钮事件，背景透明遮罩
            //LoadUIHintWindows.GetInstance.LoadHintBorderOneBtn(new string[] { "单按钮内容文本测试123", "单按钮窗体标题" }, "单按按钮文本", () => { Debug.Log("单按钮按钮事件"); });
            ////文本结构(string数组或者TextSetting结构体)，左按钮文本，右按钮文本，左按钮事件，右按钮事件，背景透明遮罩
            //LoadUIHintWindows.GetInstance.LoadHintBorderTwoBtn(new string[] { "双按钮内容文本测试123", "双按钮窗体标题" }, "左按钮文本", "右按钮文本",
            //    () => { Debug.Log("左按钮按钮事件"); }, () => { Debug.Log("右按钮按钮事件"); });
        }
    }
}