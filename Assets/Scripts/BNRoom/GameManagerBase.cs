using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameManagerBase : MonoBehaviour
{
    private void Awake()
    {
        StartGame();
    }
    
    private void OnDestroy()
    {
        EndGame();
    }

    // 公共方法，所有管理器都要实现
    public abstract void StartGame();
    public abstract void EndGame();
}
