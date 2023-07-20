using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：字符串镜头扩展类
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2022.08.05
    /// 版本：1.0
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// 首字母大写
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StringFirstUpper(this string str, string targetStr)
        {
            if (string.IsNullOrEmpty(targetStr) || string.IsNullOrEmpty(targetStr))
            {
                return str;
            }
            return targetStr.Substring(0, 1).ToUpper() + targetStr.Substring(1);
        }
    }
}