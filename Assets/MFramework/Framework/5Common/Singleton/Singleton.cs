using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 描述：单例模版类
    /// 作者：毛俊峰
    /// 时间：2022-04-02
    /// 版本：1.0
    /// </summary>
    public class Singleton<T> where T : new()
    {
        private static T m_Instance = new T();

        public static T GetInstance
        {
            get
            {
                return m_Instance;
            }
        }
    } 
}
