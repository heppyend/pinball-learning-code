
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YooAsset;

public class StartGame:MonoBehaviour
{
    private IEnumerator Start()
    {
       Debug.Log("[WebGL诊断] StartGame.Start 开始");
       // this.gameObject.AddComponent<KernelController>();
       if (!SysDefines.IsCheckVersion)
       {
            //初始化UI界面
            //var go = Resources.Load<GameObject>(SysDefines.UIPREFAB + "Canvas");

           AssetHandle handle = YooAssets.LoadAssetAsync<GameObject>("Canvas");
           yield return handle;
           Debug.Log($"[WebGL诊断] Canvas 句柄完成：状态={handle.Status}，错误={handle.LastError}");
           if (handle.Status != EOperationStatus.Succeed || handle.AssetObject == null)
           {
               Debug.LogError($"[WebGL诊断] Canvas 加载失败：状态={handle.Status}，错误={handle.LastError}");
               handle.Release();
               yield break;
           }
           GameObject go = handle.InstantiateSync();
           if (go == null)
           {
               Debug.LogError("[WebGL诊断] Canvas 实例化失败");
               handle.Release();
               yield break;
           }
           Debug.Log("[WebGL诊断] Canvas 实例化成功");
           

            //System.Console.Write("StartGame 0");
            //Debug.LogError("StartGame 0");
            var copyGo = Instantiate(go, Vector3.zero, Quaternion.identity);
            //Debug.LogError("copyGo " + copyGo.name);
            //System.Console.Write("StartGame 1");
            //var root = copyGo.transform.GetComponent<AppRoot>();
            //Destroy(root);
            go.gameObject.SetActive(false);
            //var es = go.transform.Find("EventSystem");
            //Debug.Log($"Prefab name is {go.name}{es}");
            //es.gameObject.SetActive(false);
            //Destroy(es.gameObject);

            GameController.go = copyGo;
            Debug.Log("[WebGL诊断] 开始 GameController.Init");
            GameController.Instance.Init();
            Debug.Log("[WebGL诊断] GameController.Init 完成");
            //Debug.LogError("StartGame 2");
            //System.Console.Write("StartGame 2");
            SysDefines.IsCheckVersion = true;//todo
        }

        //Debug.Log("你好，我成功实现了代码热更了！");
        //Debug.Log("哈哈，我又来热更了！"); 
        //Debug.Log("第三次热更");
        Debug.Log("热更---2016-1-16");
    }
}
