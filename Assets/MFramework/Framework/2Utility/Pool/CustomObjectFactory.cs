using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：对象池  工厂实现类
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2022.
    /// 版本：1.0
    /// </summary>
    public class CustomObjectFactory<T> : IObjectFactory<T>
    {
        private Func<T> m_CreateObjMethod;

        public CustomObjectFactory(Func<T> createObjMethod)
        {
            m_CreateObjMethod = createObjMethod;
        }

        public T Create()
        {
            return m_CreateObjMethod();
        }
    }
}