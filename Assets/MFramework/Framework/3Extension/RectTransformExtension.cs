using UnityEngine;

/// <summary>
/// 描述：RectTranform静态扩展类
/// 作者：毛俊峰
/// 时间：2022.10.18
/// 版本：1.0
/// </summary>
public static class RectTransformExtension
{
    /// <summary>
    /// 重置坐标、旋转、缩放、锚点(RectTransform静态扩展)
    /// </summary>
    /// <param name="rectTransform"></param>
    /// <param name="anchorMin"></param>
    /// <param name="anchorMax"></param>
    /// <param name="pivot"></param>
    public static void Reset(this RectTransform rectTransform, Vector2 anchorMin = default, Vector2 anchorMax = default, Vector2 pivot = default)
    {
        if (rectTransform == null) return;
        rectTransform.anchorMin = anchorMin == default ? Vector2.zero : anchorMin;
        rectTransform.anchorMax = anchorMax == default ? Vector2.one : anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.one;
        rectTransform.pivot = pivot == default ? new Vector2(0.5f, 0.5f) : pivot;
        rectTransform.anchoredPosition3D = Vector3.zero;
        rectTransform.localScale = Vector3.one;
    }
}
