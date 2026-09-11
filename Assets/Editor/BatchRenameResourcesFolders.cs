using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesFolderRenamer : EditorWindow
{
    private string targetParentFolder = "Assets"; // 默认搜索整个Assets目录

    [MenuItem("Tools/Resources to Resources1")]
    public static void ShowWindow()
    {
        GetWindow<ResourcesFolderRenamer>("Resources Folder Renamer");
    }

    void OnGUI()
    {
        GUILayout.Label("批量重命名 Resources 文件夹", EditorStyles.boldLabel);

        targetParentFolder = EditorGUILayout.TextField("搜索目录:", targetParentFolder);

        if (GUILayout.Button("开始重命名"))
        {
            if (EditorUtility.DisplayDialog("确认",
                $"确定要将 {targetParentFolder} 下所有 Resources 文件夹重命名为 Resources1？",
                "确定", "取消"))
            {
                RenameResourcesFolders();
            }
        }
    }

    void RenameResourcesFolders()
    {
        if (!Directory.Exists(targetParentFolder))
        {
            Debug.LogError($"目录不存在: {targetParentFolder}");
            return;
        }

        // 获取所有Resources文件夹（排除隐藏文件夹）
        string[] allResourcesDirs = Directory.GetDirectories(
            targetParentFolder,
            "Resources",
            SearchOption.AllDirectories);

        // 从最深层的文件夹开始处理（避免父目录重命名影响子目录路径）
        List<string> sortedPaths = new List<string>(allResourcesDirs);
        sortedPaths.Sort((a, b) => b.Length.CompareTo(a.Length));

        int successCount = 0;
        int skipCount = 0;

        foreach (string dirPath in sortedPaths)
        {
            string parentDir = Path.GetDirectoryName(dirPath);
            string newPath = Path.Combine(parentDir, "Resources1");

            // 检查目标文件夹是否已存在
            if (Directory.Exists(newPath))
            {
                Debug.LogWarning($"跳过: {dirPath} → 目标文件夹已存在: {newPath}");
                skipCount++;
                continue;
            }

            // 执行重命名
            string error = AssetDatabase.MoveAsset(dirPath, newPath);

            if (string.IsNullOrEmpty(error))
            {
                Debug.Log($"重命名成功: {dirPath} → {newPath}");
                successCount++;
            }
            else
            {
                Debug.LogError($"重命名失败 {dirPath}: {error}");
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("完成",
            $"操作完成！\n成功: {successCount} 个\n跳过: {skipCount} 个",
            "确定");
    }
}