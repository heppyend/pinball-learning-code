
/******************************************************************************
 * 
 *  Title:  捕鱼项目
 *
 *  Version:  1.0版
 *
 *  Description:
 *
 *  Author:  WangXingXing
 *       
 *  Date:  2018
 * 
 ******************************************************************************/

using UnityEngine;

static public class MethodExtension
{
    static public T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        T result = go.GetComponent<T>();
        if (null == result)
            result = go.AddComponent<T>();
        return result;
    }

    static public T GetOrAddComponent<T>(this Transform transform) where T : Component
    {
        return GetOrAddComponent<T>(transform.gameObject);
    }

    static public T GetOrAddComponent<T>(this Component component) where T : Component
    {
        return GetOrAddComponent<T>(component.gameObject);
    }


    // 新增：带1个参数的版本
    static public T GetOrAddComponent<T, TParam1>(this GameObject go, TParam1 param1) where T : Component
    {
        T result = go.GetComponent<T>();
        if (null == result)
        {
            // 使用反射创建带参数的组件实例
            result = go.AddComponent(typeof(T)) as T;
            // 获取匹配的构造函数并调用
            var ctor = typeof(T).GetConstructor(new[] { typeof(TParam1) });
            ctor?.Invoke(result, new object[] { param1 });
        }
        return result;
    }

    // 新增：带2个参数的版本
    static public T GetOrAddComponent<T, TParam1, TParam2>(this GameObject go, TParam1 param1, TParam2 param2) where T : Component
    {
        T result = go.GetComponent<T>();
        if (null == result)
        {
            result = go.AddComponent(typeof(T)) as T;
            var ctor = typeof(T).GetConstructor(new[] { typeof(TParam1), typeof(TParam2) });
            ctor?.Invoke(result, new object[] { param1, param2 });
        }
        return result;
    }

    // 为Transform添加对应的带参数重载
    static public T GetOrAddComponent<T, TParam1>(this Transform transform, TParam1 param1) where T : Component
    {
        return GetOrAddComponent<T, TParam1>(transform.gameObject, param1);
    }

    static public T GetOrAddComponent<T, TParam1, TParam2>(this Transform transform, TParam1 param1, TParam2 param2) where T : Component
    {
        return GetOrAddComponent<T, TParam1, TParam2>(transform.gameObject, param1, param2);
    }

    // 为Component添加对应的带参数重载
    static public T GetOrAddComponent<T, TParam1>(this Component component, TParam1 param1) where T : Component
    {
        return GetOrAddComponent<T, TParam1>(component.gameObject, param1);
    }

    static public T GetOrAddComponent<T, TParam1, TParam2>(this Component component, TParam1 param1, TParam2 param2) where T : Component
    {
        return GetOrAddComponent<T, TParam1, TParam2>(component.gameObject, param1, param2);
    }
}