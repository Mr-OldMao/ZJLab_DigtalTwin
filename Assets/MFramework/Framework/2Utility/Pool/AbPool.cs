using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：对象池抽象类
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2022.07.17
    /// 版本：1.0
    /// </summary>
    public abstract class AbPool<T> : IPool<T>
    {
        /// <summary>
        /// 缓存未使用(对象池剩余)的对象
        /// </summary>
        protected Stack<T> m_CacheUnuserObj = new Stack<T>();
        /// <summary>
        /// 缓存正在使用的对象
        /// </summary>
        protected List<T> m_CacheUsingObj = new List<T>();



        protected IObjectFactory<T> m_ObjectFactory;

        /// <summary>
        /// 获取当前对象池剩余对象个数
        /// </summary>
        public int GetCurUnuserObjCount
        {
            get => m_CacheUnuserObj.Count;
        }

        /// <summary>
        /// 获取对象池正在使用的个数
        /// </summary>
        public int GetCurUningObjCount
        {
            get => m_CacheUsingObj.Count;
        }

        /// <summary>
        /// 获取正在使用的对象集合
        /// </summary>
        /// <returns></returns>
        public List<T> GetUsingObjs
        {
            get => m_CacheUsingObj;
        }


        /// <summary>
        /// 获取正在使用的对象通过对象名获取
        /// 注意：1.初始化构造时泛型T为GameObject可用  2.若正在使用的对象集合有多个与objName相同的对象名，则返回第一个遍历到的对象
        /// </summary>
        /// <param name="objName"></param>
        /// <returns></returns>
        public T GetUsingObjsByObjName(string objName)
        {
            if (typeof(T) == typeof(GameObject))
            {
                foreach (T item in m_CacheUsingObj)
                {
                    GameObject obj = item as GameObject;
                    if (obj.name == objName)
                    {
                        return item;
                    }
                }

            }
            return default;
        }

        /// <summary>
        /// 分配对象
        /// </summary>
        /// <returns></returns>
        public virtual T Allocate()
        {
            T obj = GetCurUnuserObjCount > 0 ? m_CacheUnuserObj.Pop() : m_ObjectFactory.Create();
            m_CacheUsingObj.Add(obj);
            return obj;
        }



        /// <summary>
        /// 回收指定对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public abstract bool Recycle(T obj);

        /// <summary>
        /// 回收所有对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public abstract bool RecycleAll();
    }
}