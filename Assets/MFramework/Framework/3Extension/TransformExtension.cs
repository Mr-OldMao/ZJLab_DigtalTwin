using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：Transform静态扩展
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2022.08.01
    /// 版本：1.0
    /// </summary>
    public static class TransformExtension
    {
        /// <summary>
        /// 查找游戏对象(Transform静态扩展)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="transform"></param>
        /// <param name="targetName">查找的对象名</param>
        /// <param name="includeInactive">查找范围是否包含未激活的游戏对象</param>
        /// <returns></returns>
        public static T Find<T>(this Transform transform, string targetName, bool includeInactive = true) where T : Component
        {
            T res = default;
            if (transform == null || string.IsNullOrEmpty(targetName))
            {
                res = null;
            }
            var targetArr = transform.GetComponentsInChildren<T>(includeInactive);
            for (int i = 0; i < targetArr.Length; i++)
            {
                if (targetArr[i].name == targetName)
                {
                    res = targetArr[i];
                    break;
                }
            }
            return res;
        }

        /// <summary>
        /// 批量查找游戏对象(Transform静态扩展)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="transform"></param>
        /// <param name="targetName">查找的对象名</param>
        /// <param name="includeInactive">查找范围是否包含未激活的游戏对象</param>
        /// <returns></returns>
        public static List<T> Finds<T>(this Transform transform, string targetName, bool includeInactive = true) where T : Component
        {
            List<T> res = default;
            if (transform == null || string.IsNullOrEmpty(targetName))
            {
                res = null;
            }
            var targetArr = transform.GetComponentsInChildren<T>(includeInactive);
            for (int i = 0; i < targetArr.Length; i++)
            {
                if (targetArr[i].name == targetName)
                {
                    if (res == null)
                    {
                        res = new List<T>();
                    }
                    res.Add(targetArr[i]);
                }
            }
            return res;
        }

        /// <summary>
        /// 查找游戏对象(Transform静态扩展)
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="targetName">查找的对象名</param>
        /// <param name="includeInactive">查找范围是否包含未激活的游戏对象</param>
        /// <returns></returns>
        public static GameObject Find(this Transform transform, string targetName, bool includeInactive = true)
        {
            Transform res = transform.Find<Transform>(targetName, includeInactive);
            return res ? res.gameObject : null;
        }

        /// <summary>
        /// 设置对象激活状态(Transform静态扩展)
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="isShow"></param>
        public static void SetActive(this Transform transform, bool isShow)
        {
            transform.gameObject.SetActive(isShow);
        }

        /// <summary>
        /// 重置坐标、旋转、缩放(Transform静态扩展)
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="isLocal"></param>
        public static void Reset(this Transform transform, bool isLocal = true)
        {
            if (!transform) return;
            if (isLocal)
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.Euler(Vector3.zero);
                transform.localScale = Vector3.one;
            }
            else
            {
                transform.position = Vector3.zero;
                transform.rotation = Quaternion.Euler(Vector3.zero);
                transform.localScale = Vector3.one;
            }
        }
    }
}