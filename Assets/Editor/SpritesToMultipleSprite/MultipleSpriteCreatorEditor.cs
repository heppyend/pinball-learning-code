using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Momos.Tools.SpritesToMultipleSprite
{
    internal class MultipleSpriteCreatorEditor : EditorWindow
    {
        internal class SpriteItem
        {
            // 新版本
            public static implicit operator SpriteRect(SpriteItem item)
            {
                SpriteRect data = new SpriteRect();
                data.rect = new Rect(item._anchorPosition, item._size);
                data.name = item._name;
                data.alignment = (int)SpriteAlignment.Center;
                data.pivot = new Vector2(0.5f, 0.5f);
                return data;
            }

            private Vector2Int _anchorPosition;
            private Vector2Int _size;
            private string _name;

            public SpriteItem(Vector2Int anchorPosition, Vector2Int size, string name)
            {
                this._anchorPosition = anchorPosition;
                this._size = size;
                this._name = name;
            }
        }

        private string _outName = "Texture";
        private int _padding = 2;
        [SerializeField] private Sprite[] sprites;
        
        private SerializedObject _serializedObject;
        private SerializedProperty _spritesProperty;

        [MenuItem("Tools/处理图片/图片合并")]
        private static void ShowWindow()
        {
            MultipleSpriteCreatorEditor window = GetWindow<MultipleSpriteCreatorEditor>("多精灵创建器");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }
        
        private void OnEnable()
        {
            _serializedObject = new SerializedObject(this);
            _spritesProperty = _serializedObject.FindProperty("sprites");
        }

        private void OnGUI()
        {
            GUILayout.Label("多精灵贴图创建工具", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _outName = EditorGUILayout.TextField("输出名称", _outName);
            _padding = EditorGUILayout.IntField("间距", _padding);
            
            _serializedObject.Update();
            EditorGUILayout.PropertyField(_spritesProperty, true);
            _serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            GUILayout.Label("要手动开启Read/Write", EditorStyles.boldLabel);

            if (GUILayout.Button("创建", GUILayout.Height(30)))
            {
                if (sprites == null || sprites.Length == 0)
                {
                    EditorUtility.DisplayDialog("错误", "请至少添加一个精灵！", "确定");
                    return;
                }

                CreateMultipleSprite();
            }
        }

         /// <summary>
        /// 创建多个精灵
        /// </summary>
        private void CreateMultipleSprite()
        {
            List<Sprite> validSprites = ValidateAndPrepareSprites();
            
            if (validSprites.Count == 0)
            {
                EditorUtility.DisplayDialog("错误", "没有有效的精灵可以处理！", "确定");
                return;
            }
            
            TextureCreator textureCreator = new TextureCreator
            {
                padding = _padding
            };

            SpriteRect[] spriteItems = ProcessSprites(validSprites, textureCreator);
            
            string path = SaveTexture(textureCreator, spriteItems);
            
            if (!string.IsNullOrEmpty(path))
            {
                ConfigureSpriteImporter(path, spriteItems);
                EditorUtility.DisplayDialog("成功", "多精灵贴图创建完成！", "确定");
                Close();
            }
        }

        /// <summary>
        /// 验证并准备有效的精灵列表
        /// </summary>
        private List<Sprite> ValidateAndPrepareSprites()
        {
            List<Sprite> validSprites = new List<Sprite>();
            
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] == null || sprites[i].texture == null)
                {
                    Debug.LogWarning($"索引 {i} 的精灵为空或没有纹理，已跳过。");
                    continue;
                }
                
                Texture2D texture = sprites[i].texture;
                string assetPath = AssetDatabase.GetAssetPath(texture);
                
                if (string.IsNullOrEmpty(assetPath))
                {
                    Debug.LogWarning($"索引 {i} 的精灵 '{sprites[i].name}' 不是资产文件，已跳过。");
                    continue;
                }
                
                TextureImporter sourceImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                
                if (sourceImporter == null)
                {
                    Debug.LogWarning($"无法获取索引 {i} 的精灵 '{sprites[i].name}' 的纹理导入器，已跳过。");
                    continue;
                }
                
                if (!sourceImporter.isReadable)
                {
                    Debug.Log($"正在设置精灵 '{sprites[i].name}' 的纹理为可读...");
                    sourceImporter.isReadable = true;
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                }
                
                validSprites.Add(sprites[i]);
            }
            
            // 按面积降序排序
            validSprites.Sort((a, b) =>
            {
                int areaA = a.texture.width * a.texture.height;
                int areaB = b.texture.width * b.texture.height;
                return areaB.CompareTo(areaA);
            });
            
            return validSprites;
        }

        /// <summary>
        /// 处理精灵并生成精灵矩形数据
        /// </summary>
        private SpriteRect[] ProcessSprites(List<Sprite> validSprites, TextureCreator textureCreator)
        {
            SpriteRect[] spriteItems = new SpriteRect[validSprites.Count];
            int totalSprites = validSprites.Count;
            
            for (int i = 0; i < totalSprites; i++)
            {
                EditorUtility.DisplayProgressBar("处理精灵", $"正在处理: {validSprites[i].name}", (float)i / totalSprites);
                
                try
                {
                    Vector2Int size = new Vector2Int(validSprites[i].texture.width, validSprites[i].texture.height);
                    Color[][] colors = ExtractColorsFromTexture(validSprites[i].texture);
                    
                    spriteItems[i] = new SpriteItem(textureCreator.Add(colors), size, validSprites[i].name);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"处理精灵 '{validSprites[i].name}' 时出错: {ex.Message}");
                }
            }
            
            EditorUtility.ClearProgressBar();
            return spriteItems;
        }

        /// <summary>
        /// 从纹理中提取颜色数据
        /// </summary>
        private Color[][] ExtractColorsFromTexture(Texture2D texture)
        {
            int width = texture.width;
            int height = texture.height;
            
            // 使用 GetPixels 批量获取，性能更好
            Color[] allPixels = texture.GetPixels();
            
            // 转换为二维数组格式
            Color[][] colors = new Color[height][];
            for (int y = 0; y < height; y++)
            {
                colors[y] = new Color[width];
                Array.Copy(allPixels, y * width, colors[y], 0, width);
            }
            
            return colors;
        }

        /// <summary>
        /// 保存纹理到文件
        /// </summary>
        private string SaveTexture(TextureCreator textureCreator, SpriteRect[] spriteItems)
        {
            string path = EditorUtility.OpenFolderPanel("保存", "", "");
            
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("用户取消了操作。");
                return null;
            }
            
            try
            {
                path = textureCreator.Create(_outName, path);
                AssetDatabase.Refresh();
                
                // 转换为 Unity Assets 路径
                string assetPath = "Assets/" + path.Remove(0, Application.dataPath.Length);
                return assetPath;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"保存纹理时出错: {ex.Message}");
                EditorUtility.DisplayDialog("错误", $"保存失败: {ex.Message}", "确定");
                return null;
            }
        }

        /// <summary>
        /// 配置精灵导入器
        /// </summary>
        private void ConfigureSpriteImporter(string assetPath, SpriteRect[] spriteItems)
        {
            TextureImporter outputImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            
            if (outputImporter == null)
            {
                Debug.LogError($"无法获取 TextureImporter，路径: {assetPath}");
                return;
            }
            
            outputImporter.textureType = TextureImporterType.Sprite;
            outputImporter.alphaIsTransparency = true;
            outputImporter.spriteImportMode = SpriteImportMode.Multiple;
            outputImporter.spritePixelsPerUnit = 100;
            
            SpriteDataProviderFactories factory = new SpriteDataProviderFactories();
            factory.Init();
            ISpriteEditorDataProvider dataProvider = factory.GetSpriteEditorDataProviderFromObject(outputImporter);
            dataProvider.InitSpriteEditorDataProvider();
            dataProvider.SetSpriteRects(spriteItems);
            dataProvider.Apply();

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
        }


    }
}
