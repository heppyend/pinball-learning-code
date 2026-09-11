//#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
//#endif

public class LayerDebug : MonoBehaviour
{
    [ContextMenu("打印所有Layers")]
    void DebugAllLayers()
    {
        DebugCurrentLayer();
    }

    private void Awake()
    {
        DebugCurrentLayer();
    }
    [ContextMenu("打印当前对象Layer")]
    void DebugCurrentLayer()
    {
        Debug.Log($"{gameObject.name} - Layer: {gameObject.layer} - {LayerMask.LayerToName(gameObject.layer)}");
    }
}