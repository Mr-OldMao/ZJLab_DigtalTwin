using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 描述：单例模版类基于mono
    /// 作者：毛俊峰
    /// 时间：2022-04-02
    /// 版本：1.0
    /// </summary>
    public  class SingletonByMono<T> : MonoBehaviour where T : Component
    {
        private static T m_Instance;
        private static string singletonSceneGameName = "MFrameworkSingletonRoot";
        private static GameObject singletonRoot = null;

        public static T GetInstance
        {
            get
            {
                if (m_Instance == null)
                {
                    if (FindObjectOfType<T>())
                    {
                        m_Instance = FindObjectOfType<T>();
                    }
                    else
                    {
                        if (singletonRoot == null)
                        {
                            singletonRoot = GameObject.Find(singletonSceneGameName);
                            if (singletonRoot == null)
                            {
                                singletonRoot = new GameObject(singletonSceneGameName);
                                DontDestroyOnLoad(singletonRoot);
                            }
                        }
                        GameObject singletonSubRoot = new GameObject(typeof(T).Name);
                        singletonSubRoot.transform.SetParent(singletonRoot.transform);
                        m_Instance = singletonSubRoot.AddComponent<T>();
                    }
                }
                return m_Instance;
            }
        }
    }
}
