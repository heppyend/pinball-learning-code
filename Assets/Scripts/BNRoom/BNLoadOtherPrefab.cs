/******************************************************************************
 * 
 *  Title:  弹珠项目
 *
 *  Version:  1.0版
 *
 *  Description: 加载的预制体实现
 *
 *  Author:  LingBin
 *       
 *  Date:  2026
 * 
 ******************************************************************************/
using System.Collections.Generic;
using UnityEngine;

public class BNLoadOtherPrefab : DDOLSingleton<BNLoadOtherPrefab>
{
    private Dictionary<string, GameObject> dicOtherPrefab = new Dictionary<string, GameObject>();        //存储预制体对象的字典
    public Dictionary<string, GameObject> DicOtherPrefab
    {
        get { return dicOtherPrefab; }
    }
    private bool HasInitOtherPrefab;

    /// <summary>
    /// 提前加载需要的其他预制体
    /// </summary>
    /// <returns></returns>
    public void InitOtherPrefab()
    {
        if (HasInitOtherPrefab) return;
        HasInitOtherPrefab = true;
        int count = SysDefines.preloadPrefab.Length;
        for (int i = 0; i < count; i++)
        {
            GameObject otherPrefab = ResManager.Instance.LoadPrefab(SysDefines.preloadPrefab[i][1]);    //加载预制体
            DicOtherPrefab.Add(SysDefines.preloadPrefab[i][0], otherPrefab);                            //将预制体添加到字典中保存
        }

    }

    /// <summary>
    /// 实例化预制体
    /// </summary>
    /// <param name="prefabName"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    public GameObject CreateOtherPrefab(string prefabName, Transform transform)
    {
        GameObject prefab;
        GameObject newObj = null;
        if (DicOtherPrefab.TryGetValue(prefabName, out prefab))                          //加载海盗的预制体
        {
            newObj = ObjectPoolManager.Instance.Spawn(prefab, transform);      //生成Object，并完成实例化
            //newObj.SetActive(false);
        }
        else
        {
            Debug.LogError("没有找到预制体！" + prefabName);
        }

        return newObj;
    }
    public GameObject CreateOtherPrefab(string prefabName, Transform transform, Vector3 pos)
    {
        GameObject newObj = CreateOtherPrefab(prefabName, transform);
        newObj.transform.localPosition = pos;
        newObj.transform.localEulerAngles = Vector3.zero;
        return newObj;
    }

    /// <summary>
    /// 删除预制体
    /// </summary>
    /// <param name="prefab"></param>
    public void DestroyAllOtherPrefab()
    {
        //foreach (var kvp in DicOtherPrefab)
        //{
        //    Destroy(kvp.Value); // 销毁实例化的对象
        //}
        //Debug.LogWarning("清除其他预制体的资源文件！！！");
        DicOtherPrefab.Clear(); // 清空字典
        Resources.UnloadUnusedAssets(); // 卸载未使用的资源
    }

    private void OnDestroy()
    {
        DestroyAllOtherPrefab();
    }

}
