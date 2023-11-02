using MFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 标题：程序入口
/// 功能：初始化游戏框架、游戏逻辑
/// 作者：毛俊峰
/// 时间：2023.8.18
/// </summary>
public class GameLaunch : SingletonByMono<GameLaunch>
{
    [SerializeField]
    private LaunchModel m_LaunchModel = LaunchModel.BuilderModel;
    /// <summary>
    /// 项目运行模式
    /// </summary>
    public LaunchModel LaunchModel { get => m_LaunchModel; }

    public Scenes scene;

    public enum Scenes
    {
        MainScene1,
        MainScene2
    }

    private void Awake()
    {
        //初始化游戏框架
        this.InitFramework();
        //检查资源更新
        this.CheckHotUpdate();
        //初始化游戏逻辑
        this.InitGameLogic();
    }


    private void CheckHotUpdate()
    {
        //获取服务器资源 脚本代码版本

        //拉去下载列表

        //下载更新资源到本地

    }
    private void InitFramework()
    {
#if !UNITY_EDITOR
        DebuggerConfig.CanPrintConsoleLog = true;
        DebuggerConfig.CanPrintConsoleLogError = true;
        DebuggerConfig.CanSaveLogDataFile = false; 
#endif
    }
    private void InitGameLogic()
    {
        switch (scene)
        {
            case Scenes.MainScene1:
                switch (m_LaunchModel)
                {
                    case LaunchModel.EditorModel:
                        NetworkMqtt.GetInstance.IsWebgl = false;
                        break;
                    case LaunchModel.BuilderModel:
#if UNITY_WEBGL && !UNITY_EDITOR
                NetworkMqtt.GetInstance.IsWebgl = true;
#else
                        NetworkMqtt.GetInstance.IsWebgl = false;
#endif
                        break;
                    default:
                        break;
                }
                Debugger.Log("iswebgl:" + NetworkMqtt.GetInstance.IsWebgl, LogTag.Forever);
                GameLogic.GetInstance.Init();
                break;
            case Scenes.MainScene2:
                GameLogic2.GetInstance.Init();
                break;
            default:
                break;
        }
       
    }
}

public enum LaunchModel
{
    /// <summary>
    /// 引擎调试模式
    /// </summary>
    EditorModel,
    /// <summary>
    /// 真机打包模式
    /// </summary>
    BuilderModel,
}

