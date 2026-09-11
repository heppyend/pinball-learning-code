using System;
using UnityEngine;

public class DynamicInfinityItem : MonoBehaviour
{
    public Action<DynamicInfinityItem> OnSelectHandler;
    public Action<DynamicInfinityItem> OnUpdateDataHandler;

    //动态矩形
    protected DynamicRect mDRect;

    //格子数据
    protected object Data;

    public DynamicRect DRect
    {
        set
        {
            mDRect = value;
            gameObject.SetActive(null != value);
        }
        get
        {
            return mDRect;
        }
    }

    /// <summary>
    /// 设置数据
    /// </summary>
    /// <param name="data"></param>
    public void SetData(object data)
    {
        if (ReferenceEquals(data, null)) return;
        Data = data;
        OnUpdateDataHandler?.Invoke(this);
        OnRenderer();
    }

    //重写
    virtual protected void OnRenderer() { }

    //获得数据
    public object GetData()
    {
        return Data;
    }

    //获得指定类型数据
    public T GetData<T>()
    {
        return (T)Data;
    }

}
