using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class AutoAssetBundleMarker : EditorWindow
{
    private string rootFolder = "Assets/Prefabs"; // 默认扫描的根文件夹

    [MenuItem("Tools/Auto AssetBundle Marker")]
    public static void ShowWindow()
    {
        AssetDatabase.RemoveUnusedAssetBundleNames();
        //GetWindow<AutoAssetBundleMarker>("Auto AB Marker");
    }

    void OnGUI()
    {
        GUILayout.Label("Auto Mark Prefabs with Folder Names", EditorStyles.boldLabel);

        rootFolder = EditorGUILayout.TextField("Root Folder:", rootFolder);

        if (GUILayout.Button("Scan and Mark AssetBundles"))
        {
            MarkPrefabsWithFolderNames();
        }
    }

    void MarkPrefabsWithFolderNames()
    {
        if (!Directory.Exists(rootFolder))
        {
            Debug.LogError($"Directory not found: {rootFolder}");
            return;
        }

        // 获取所有子文件夹
        string[] subFolders = Directory.GetDirectories(rootFolder, "*", SearchOption.AllDirectories);

        int markedCount = 0;

        foreach (string folder in subFolders)
        {
            // 转换为Unity相对路径
            string unityFolderPath = folder.Replace("\\", "/");

            // 获取文件夹名作为AssetBundle名称
            string folderName = new DirectoryInfo(folder).Name.ToLower();
            string bundleName = $"prefabs_{folderName}";

            // 获取该文件夹下所有预制体
            string[] prefabPaths = Directory.GetFiles(folder, "*.prefab", SearchOption.TopDirectoryOnly);

            foreach (string prefabPath in prefabPaths)
            {
                string unityPrefabPath = prefabPath.Replace("\\", "/");
                AssetImporter importer = AssetImporter.GetAtPath(unityPrefabPath);

                if (importer != null)
                {
                    importer.assetBundleName = bundleName;
                    markedCount++;
                    Debug.Log($"Marked: {unityPrefabPath} -> {bundleName}", AssetDatabase.LoadAssetAtPath<Object>(unityPrefabPath));
                }
            }
        }

        AssetDatabase.RemoveUnusedAssetBundleNames();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Complete",
            $"Successfully marked {markedCount} prefabs with folder-based AssetBundle names!",
            "OK");
    }
}