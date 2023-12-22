using MFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 标题：缓存物品静态属性
/// 功能：
/// 作者：毛俊峰
/// 时间：2023.
/// </summary>
public class ItemStaticData
{
    public static Dictionary<string, bool> m_ItemStaticDic;

    private static void Init()
    {
        m_ItemStaticDic = new Dictionary<string, bool>
        {
            { "Bathtub", true },
            { "Bed", true },
            { "Bigsink", true },
            { "Bin", false },
            { "Book", false },
            { "Cabinet", true },
            { "Chair", true },
            { "Clothes", false },
            { "Cup", false },
            { "Desk", true },
            { "Drink", false },
            { "Food", false },
            { "Knife", false },
            { "Lamp", true },
            { "PC", true },
            { "Pile", true },
            { "Plant", true },
            { "Pot", false },
            { "Sink", true },
            { "Sofa", true },
            { "TV", true }
        };
    }

    /// <summary>
    /// 是否为静态物品 t-是
    /// </summary>
    /// <param name="itemType"></param>
    /// <returns></returns>
    public static bool GetItemStatic(string itemType)
    {
        if (m_ItemStaticDic == null)
        {
            Init();
        }
        bool res = true;
        if (m_ItemStaticDic.ContainsKey(itemType))
        {
            res = m_ItemStaticDic[itemType];
        }
        else
        {
            Debugger.Log("无法获取当天物体静态属性 itemType：" + itemType);
        }
        return res;
    }
}
