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

    }
    private void InitGameLogic()
    {
        GameLogic.GetInstance.Init();
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

