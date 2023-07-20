using MFramework;
using UnityEngine;

/// <summary>
/// 标题：测试UIManager
/// 功能：UI窗体的显示、隐藏、获取UI窗体实体、获取UI窗体上所挂载的脚本
/// 作者：毛俊峰
/// 时间：2023.04.20
/// 版本：1.0
/// </summary>
public class TestUIManager : MonoBehaviour
{
    private void Start()
    {
        Debug.LogError("演示UIMananger案例 需要先导入Unity资源包后，再取消注释后续代码即可。UnityPackagePath:Assets/MFramework/Example/AssetsUnityPackage/ExampleAssetsUIManager.unitypackage");

        ////设置UICanvas根节点
        //UIManager.SetUIRootCanvas(1920, 1080, 0);

        ////显示UI窗体  规则：事先已经在UIFormConfig.RegisterUIForm()中注册了UI预制体所要挂载的同名脚本
        //UIManager.GetInstance.Show<UIFormTest>();

        ////隐藏UI窗体
        //UIManager.GetInstance.Hide<UIFormTest>();

        ////显示UI窗体  规则：为UI窗体添加指定脚本T，T需要继承UIFormBase
        //UIManager.GetInstance.Show<UIFormTest>("Assets/GameMain/UI/UIForms/Main/UIFormMain.prefab");

        ////调用UI窗体所挂载的脚本中的方法
        //UIManager.GetInstance.GetUIFormLogicScript<UIFormTest>().Test();

        ////获取UI窗体实体
        //UIManager.GetInstance.GetUIFormEntity<UIFormTest>().name = "UIForm123";
    }
}
