using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 拓展类
/// </summary>
public static class MyExtension
{
    /// <summary>
    /// 获取子物体中T类型，除去父物体和子物体的子物体
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="trans"></param>
    /// <returns></returns>
    public static T[] GetComponentsExceptParentAndChildedChild<T>(this Transform trans)
    {
        List<T> list = new List<T>();
        for (int i = 0; i < trans.childCount; i++)
        {
            int index = i;
            T t = trans.GetChild(index).GetComponent<T>();
            if (t!=null)
            {
                list.Add(t);
            }
           
        }
        return list.ToArray();
    }

    /// <summary>
    /// 获取子物体中T类型，除去父物体和子物体的子物体
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="go"></param>
    /// <returns></returns>
    public static T[] GetComponentsExceptParentAndChildedChild<T>(this GameObject go)
    {
        List<T> list = new List<T>();
        for (int i = 0; i < go.transform.childCount; i++)
        {
            int index = i;
            T t = go.transform.GetChild(index).GetComponent<T>();
            if (t != null)
            {
                list.Add(t);
            }

        }
        return list.ToArray();
    }

    /// <summary>
    /// 根据子物体的名称，找到子物体
    /// </summary>
    /// <param name="currentTransform">子物体所在层级</param>
    /// <param name="childName">子物体名称</param>
    /// <returns></returns>
    public static Transform FindChildForName(this Transform currentTransform, string childName)
    {
        Transform childTrans = currentTransform.Find(childName);
        if (childTrans != null)
        {
            return childTrans;
        }
        for (int i = 0; i < currentTransform.childCount; i++)
        {
            childTrans = FindChildForName(currentTransform.GetChild(i), childName);
            if (childTrans != null)
            {
                return childTrans;
            }
        }
        return null;
    }
}
