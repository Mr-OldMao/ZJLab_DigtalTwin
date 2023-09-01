using MFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 标题：UI窗体基类
/// 功能：
/// 作者：毛俊峰
/// 时间：2022.12.29
/// </summary>
public class UIFormBase : MonoBehaviour, IUIFormBase
{
    /// <summary>
    /// 当前窗体是否正在显示
    /// </summary>
    public bool IsShow { get; private set; }
    /// <summary>
    /// 获取UI窗体层级
    /// </summary>
    public UILayerType GetUIFormLayer { get; protected set; }

    public virtual void Show()
    {
        gameObject.SetActive(true);
        IsShow = true;
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
        IsShow = false;
    }
    protected virtual void OnEnable()
    {

    }
    protected virtual void Awake()
    {
       
    }
    protected virtual void Start()
    {
        RegisterUIEvnet();
    }
    protected virtual void Update()
    {

    }

    protected virtual void FixedUpdate()
    {

    }

    protected virtual void LateUpdate()
    {

    }

    protected virtual void OnDestroy()
    {

    }
    protected virtual void OnDisable()
    {

    }

    protected virtual void RegisterUIEvnet()
    {

    }
}
