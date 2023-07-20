using MFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 标题：UI窗体基类接口
/// 功能：
/// 作者：毛俊峰
/// 时间：2023.01.04
/// </summary>
interface IUIFormBase
{
    bool IsShow { get; }
    UILayerType GetUIFormLayer { get; }
}
