using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：引用计数器接口
    /// 功能：缓存资源的使用次数，资源引用次数为0时自动释放资源
    /// 作者：毛俊峰
    /// 时间：2022.07.24
    /// 版本：1.0
    /// </summary>
    public interface IRefCounter
    {
        /// <summary>
        /// 引用次数
        /// </summary>
        int RefCount { get; }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="refOwner"></param>
        void Retain(object refOwner = null);

        /// <summary>
        /// 持有资源
        /// </summary>
        /// <param name="refOwner"></param>
        void Release(object refOwner = null);
    }
}