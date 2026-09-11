
using UnityEngine;


/// <summary>
/// 动态格子
/// </summary>
public class DynamicRect
{
    //矩形数据
    private Rect mRect;
    //格子索引
    public int Index;

    public DynamicRect(float x, float y, float width, float height, int index)
    {
        Index = index;
        mRect = new Rect(x, y, width, height);
    }

    /// <summary>
    /// 是否相交
    /// </summary>
    /// <param name="otherRect"></param>
    /// <returns></returns>
    public bool Overlaps(DynamicRect otherRect)
    {
        return mRect.Overlaps(otherRect.mRect);
    }

    /// <summary>
    /// 是否相交
    /// </summary>
    /// <param name="otherRect"></param>
    /// <returns></returns>
    public bool Overlaps(Rect otherRect)
    {
        return mRect.Overlaps(otherRect);
    }
}
