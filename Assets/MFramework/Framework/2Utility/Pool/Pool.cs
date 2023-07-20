using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：简单对象池
    /// 功能：获取对象、回收对象、获取正在使用的对象
    /// 作者：毛俊峰
    /// 时间：2022.07.17
    /// 版本：1.0
    /// </summary>
    public class Pool<T> : AbPool<T>
    {
        /// <summary>
        /// 回收对象时回到
        /// </summary>
        private Action<T> m_RecycleMethod;

        /// <summary>
        /// 获取对象时回调 
        /// </summary>
        protected Action<T> m_GetObjCallback;

        /// <summary>
        /// 简单对象池构造
        /// </summary>
        /// <param name="createObjMethod">回调 创建新对象回调（分配对象-创建新对象）</param>
        /// <param name="getObjCallback">回调 分配对象(新对象、旧对象) 后回调</param>
        /// <param name="recycleMethod">回调 回收对象后回调</param>
        /// <param name="initCount">预先创建对象的个数</param>
        public Pool(Func<T> createObjMethod, Action<T> getObjCallback = null, Action<T> recycleMethod = null, int initCount = 0)
        {
            m_ObjectFactory = new CustomObjectFactory<T>(createObjMethod);
            m_RecycleMethod = recycleMethod;
            m_GetObjCallback = getObjCallback;
            if (typeof(T) == typeof(GameObject))
            {
                //若T为GameObject 默认获取对象时 对象激活状态为True
                m_GetObjCallback += (T obj) => (obj as GameObject).gameObject.SetActive(true);
                //若T为GameObject 默认回收对象时 对象激活状态为False
                m_RecycleMethod += (T obj) => (obj as GameObject).gameObject.SetActive(false);
            }

            for (int i = 0; i < initCount; i++)
            {
                m_CacheUnuserObj.Push(m_ObjectFactory.Create());
            }
        }

        /// <summary>
        /// 分配对象
        /// </summary>
        /// <returns></returns>
        public override T Allocate()
        {
            T obj = base.Allocate();
            m_GetObjCallback?.Invoke(obj);
            return obj;
        }

        /// <summary>
        /// 回收指定对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Recycle(T obj)
        {
            m_CacheUnuserObj.Push(obj);
            m_CacheUsingObj.Remove(obj);
            m_RecycleMethod?.Invoke(obj);
            return true;
        }

        /// <summary>
        /// 回收所有正在使用的对象
        /// </summary>
        /// <returns></returns>
        public override bool RecycleAll()
        {
            while (m_CacheUsingObj.Count > 0)
            {
                Recycle(m_CacheUsingObj[0]);
            }
            return true;
        }

        /// <summary>
        /// 调用模版
        /// </summary>
        private void Test()
        {
            GameObject objRes = null;
            Transform objResParent = null;
            Pool<GameObject> pool = new Pool<GameObject>(() =>
            {
                //回调 获取对象-创建新对象后回调
                GameObject newObj = UnityEngine.Object.Instantiate<GameObject>(objRes);
                newObj.transform.SetParent(objResParent);
                newObj.gameObject.SetActive(false);
                return newObj;
            }, (GameObject cube) =>
            {
                //获取对象后回调
                //cube.gameObject.SetActive(true);
            }, (GameObject cube) =>
            {
                //回收对象时回调
                //cube.gameObject.SetActive(false);
            }, 10);
        }
    }
}