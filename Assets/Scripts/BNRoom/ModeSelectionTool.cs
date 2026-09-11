using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeSelectionTool : MonoBehaviour
{
    public static ModeSelectionTool Instance;

    public int mode;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
