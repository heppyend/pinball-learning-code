using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicInfinityList : MonoBehaviour
{
    [Header("单元格尺寸（宽，高）")]
    public Vector2 CellSize;
    [Header("单元格间隙（水平，垂直）")]
    public Vector2 SpacingSize;
    [Header("列数(必须大于等于1)")]
    public int ColumnCount = 1;
    [Header("动态格子prefab")]
    public GameObject ItemPrefab;
    [Header("隐藏的动态格子数(必须大于等于1)")]
    public int HideItemCount = 1;

    //渲染格子数
    protected int itemCount;
    //蒙板尺寸
    private Vector2 maskSize;
    //蒙板矩形
    private Rect maskRect;
    //父节点
    protected RectTransform rectParent;
    //渲染脚本集合
    protected List<DynamicInfinityItem> itemList;
    //渲染格子字典
    private Dictionary<int, DynamicRect> rectDic;
    //数据来源
    protected IList dataProviders;
    //是否完成初始化
    protected bool hasInited = false;

    //初始化渲染脚本
    virtual public void InitList(Action<DynamicInfinityItem> onSelect = null, Action<DynamicInfinityItem> onUpdate = null)
    {
        if (hasInited) return;
        //父节点
        rectParent = transform as RectTransform;
        //蒙板尺寸
        maskSize = transform.parent.GetComponent<RectTransform>().sizeDelta;
        //通过蒙板尺寸和格子尺寸计算需要的渲染格子数(多一个动态隐藏)
        itemCount = ColumnCount * (Mathf.CeilToInt(maskSize.y / GetBlockSizeY()) + HideItemCount);
        updateDynmicRects(itemCount);
        itemList = new List<DynamicInfinityItem>();
        for (int i = 0; i < itemCount; i++)
        {
            var child = Instantiate(ItemPrefab).transform;
            child.SetParent(rectParent);
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;
            child.gameObject.layer = rectParent.gameObject.layer;
            var dfItem = child.GetOrAddComponent<DynamicInfinityItem>();
            itemList.Add(dfItem);
            itemList[i].DRect = rectDic[i];
            itemList[i].OnSelectHandler = onSelect;
            itemList[i].OnUpdateDataHandler = onUpdate;
            child.gameObject.SetActive(false);
            updateChildTransformPos(child, i);
        }
        setRectParentSize(itemCount);
       // Debug.LogError("刷新移动格子？"+hasInited);
        hasInited = true;
    }

    //获得数据来源
    public IList GetDataProvider()
    {
        return dataProviders;
    }

    /// <summary>
    /// 数据变化，刷新数据
    /// </summary>
    public void RefreshDataProvider()
    {
        if (ReferenceEquals(dataProviders,null))
            throw new Exception("dataProviders 为空！请先使用SetDataProvider ");
        updateDynmicRects(dataProviders.Count);
        setRectParentSize(dataProviders.Count);
        clearAllItemListDr();
    }

    /// <summary>
    /// 设置父节点的尺寸
    /// </summary>
    /// <param name="itemCount"></param>
    private void setRectParentSize(int count)
    {
        rectParent.sizeDelta = new Vector2(rectParent.sizeDelta.x, Mathf.CeilToInt((count * 1.0f / ColumnCount)) * GetBlockSizeY());
        maskRect = new Rect(0, -maskSize.y, maskSize.x, maskSize.y);
    }

    /// <summary>
    /// 更新各个渲染格子的位置
    /// </summary>
    /// <param name="child">格子物体</param>
    /// <param name="index">下标</param>
    private void updateChildTransformPos(Transform child, int index)
    {
        //行
        int row = index / ColumnCount;
        //列
        int column = index % ColumnCount;
        var v2Pos = new Vector2();
        v2Pos.x = column * GetBlockSizeX();
        v2Pos.y = -CellSize.y - row * GetBlockSizeY();
        ((RectTransform)child).anchoredPosition3D = Vector3.zero;
        ((RectTransform)child).anchoredPosition = v2Pos;
    }

    //获得格子数的尺寸
    public float GetBlockSizeY() { return CellSize.y + SpacingSize.y; }
    public float GetBlockSizeX() { return CellSize.x + SpacingSize.x; }

    //更新动态渲染格
    private void updateDynmicRects(int count)
    {
        rectDic = new Dictionary<int, DynamicRect>();
        for (int i = 0; i < count; ++i)
        {
            int row = i / ColumnCount;
            int column = i % ColumnCount;
            DynamicRect dRect = new DynamicRect(column * GetBlockSizeX(), -row * GetBlockSizeY() - CellSize.y, CellSize.x, CellSize.y, i);
            rectDic[i] = dRect;
        }
    }

    /// <summary>
    /// 设置数据来源
    /// </summary>
    /// <param name="datas">数据</param>
    public void SetDataProvider(IList datas)
    {
        updateDynmicRects(datas.Count);
        setRectParentSize(datas.Count);
        dataProviders = datas;
        clearAllItemListDr();
    }

    /// <summary>
    /// 重置位置
    /// </summary>
    public void Reset()
    {
        rectParent.anchoredPosition = new Vector2(rectParent.anchoredPosition.x, 0);
    }

    //清理可复用的渲染格子
    private void clearAllItemListDr()
    {
        if (!ReferenceEquals(itemList, null))
        {
            foreach (var item in itemList)
                item.DRect = null;
        }
    }

    protected void UpdateItem()
    {
        maskRect.y = -maskSize.y - rectParent.anchoredPosition.y;
        var inOverlaps = new Dictionary<int, DynamicRect>();
        foreach (var rect in rectDic.Values)
        {
            if (rect.Overlaps(maskRect))
                inOverlaps.Add(rect.Index, rect);
        }
        foreach (var item in itemList)
        {
            if (!ReferenceEquals(item.DRect, null)&& !inOverlaps.ContainsKey(item.DRect.Index))
                item.DRect = null;
        }

        foreach (var rect in inOverlaps.Values)
        {
            if (ReferenceEquals(getDynmicItem(rect), null))
            {
                var item = getNullDynmicItem();
                item.DRect = rect;
                updateChildTransformPos(item.transform, rect.Index);
                if (!ReferenceEquals(dataProviders, null) && rect.Index < dataProviders.Count)
                {
                    item.SetData(dataProviders[rect.Index]);
                  //  Debug.LogError("开启中...."+rect.Index+" , "+dataProviders.Count);
                }
            }
        }
    }

    /// <summary>
    /// 获得待渲染的脚本
    /// </summary>
    /// <returns></returns>
    private DynamicInfinityItem getNullDynmicItem()
    {
        foreach (var item in itemList)
        {
            if (ReferenceEquals(item.DRect, null))
                return item;
        }
        throw new Exception("Error");
    }

    /// <summary>
    /// 通过动态格子获得动态渲染脚本
    /// </summary>
    /// <param name="rect">动态格子</param>
    /// <returns></returns>
    private DynamicInfinityItem getDynmicItem(DynamicRect rect)
    {
        foreach (var item in itemList)
        {
            if (ReferenceEquals(item.DRect, null))
                continue;
            if (rect.Index == item.DRect.Index)
                return item;
        }
        return null;
    }

    private void Update()
    {
        if (hasInited)
        {
            UpdateItem();
            
        }
            
    }
}
