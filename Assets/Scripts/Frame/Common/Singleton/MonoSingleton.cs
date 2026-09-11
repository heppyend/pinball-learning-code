using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T> {

    private static T mInstance = null;

    public static T Instance {
        get {
            if (mInstance == null) {
                mInstance = GameObject.FindObjectOfType<T>() as T;
                if (mInstance == null)
                    mInstance = new GameObject(typeof(T).ToString(), typeof(T)).GetComponent<T>();
            }
            return mInstance;
        }
    }

    private void Awake() {
        if (mInstance == null) {
            mInstance = this as T;
            mInstance.Create();
        } else
            DestroyImmediate(this.gameObject);
    }

    private void Create() {
        DontDestroyOnLoad(mInstance);
        mInstance.Initialize();
    }

    protected virtual void Initialize() { }

    private void OnApplicationQuit() {
        mInstance = null;
    }

}

