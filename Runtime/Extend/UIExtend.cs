using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UIEXtend {
    /// <summary>
    /// 计算相对于某个 RectTransform 的最小屏幕坐标
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="rectTransform"> 参考的 RectTransform</param>
    /// <returns></returns>
    public static Vector2 GetMinPositionInRectTransform(this RectTransform rect, RectTransform rectTransform) {
        var localPos = (Vector2.zero - rect.pivot) * rect.sizeDelta * rect.lossyScale;
        var worldPos = rect.TransformPoint(localPos);
        return rectTransform.InverseTransformPoint(worldPos);
    }

    public static Vector2 GetMaxPositionInRectTransform(this RectTransform rect, RectTransform rectTransform) {
        var localPos = (Vector2.one - rect.pivot) * rect.sizeDelta * rect.lossyScale;
        var worldPos = rect.TransformPoint(localPos);
        return rectTransform.InverseTransformPoint(worldPos);
    }
}
