using UnityEditor;
using UnityEngine;
using System.IO;

public class FindMissingScripts : EditorWindow
{
    [MenuItem("Tools/Find Missing Scripts")]
    public static void FindMissing()
    {
        string[] allPrefabs = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);

        foreach (string prefabPath in allPrefabs)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
           // Debug.LogError(">>>>> : "+ prefabPath);
            Component[] components = prefab.GetComponentsInChildren<Component>(true);
            foreach (Component component in components)
            {
                if (component == null) // 丢失的脚本会变成 null
                {
                    Debug.LogError($"Missing script in Prefab: {prefabPath}", prefab);
                    break;
                }
            }
        }

        Debug.Log("Missing script check completed!");
    }
}