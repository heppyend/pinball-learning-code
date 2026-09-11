using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BNMarbleLogic;

public class BNGlobalManager : MonoBehaviour
{
    public static BNGlobalManager Instance;

    public int GameMode;

    public void Awake()
    {
        #region 单例初始化
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        #endregion
    }
}
