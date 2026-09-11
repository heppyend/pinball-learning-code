using System.IO;
using UnityEngine;

namespace Momos.Tools.SpritesToMultipleSprite
{
    /// <summary>
    /// 用法:
    /// 1.创建后调用<see cref="Add(Color[][])"/>方法加入色块,方法返回值为 色块被放置到的贴图像素位置(左下原点);
    /// 2.全部加入后调用<see cref="Create(string, string)"/>方法创建贴图.
    /// </summary>
    public class TextureCreator
    {
        #region Constants & Properties

        /// <summary> 默认颜色(透明) </summary>
        public Color32 DefaultColor => new Color32(0, 0, 0, 0);
        
        /// <summary> 第二色 占位用 </summary>
        public Color32 SecondColor => new Color32(0, 0, 0, 1);

        /// <summary> 间隔填充(单边像素数) </summary>
        public int padding = 2;

        /// <summary> 填充是一边的值,对于区域则是两边都要填充 </summary>
        public Vector2Int PaddingArea => padding * Vector2Int.one * 2;
        
        /// <summary> 贴图大小(正方形边长) </summary>
        public int Size => colorTable.GetLength(0);

        #endregion

        #region Private Fields

        /// <summary> 色彩表 </summary>
        private Color[,] colorTable;
        
        /// <summary> 绘制指针(下一个可用位置) </summary>
        private Vector2Int pointer;
        
        /// <summary> 缓存的贴图宽度 </summary>
        private int width;
        
        /// <summary> 缓存的贴图高度 </summary>
        private int height;

        #endregion

        #region Constructor

        /// <summary>
        /// 创建默认大小的 TextureCreator(1x1)
        /// </summary>
        public TextureCreator()
        {
            Initialize(1, 1);
        }

        /// <summary>
        /// 创建指定大小的 TextureCreator
        /// </summary>
        /// <param name="size">贴图尺寸(正方形)</param>
        public TextureCreator(Vector2Int size)
        {
            Initialize(size.x, size.y);
        }

        /// <summary>
        /// 初始化色彩表和指针
        /// </summary>
        private void Initialize(int width, int height)
        {
            this.width = width;
            this.height = height;
            colorTable = new Color[width, height];
            pointer = Vector2Int.zero;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 把一块颜色区域加入贴图
        /// </summary>
        /// <param name="colors">颜色二维数组</param>
        /// <returns>左下角锚点位置,失败返回 (-1, -1)</returns>
        public Vector2Int Add(Color[][] colors)
        {
            if (colors == null || colors.Length == 0 || colors[0].Length == 0)
            {
                Debug.LogWarning("传入的颜色数组为空");
                return -Vector2Int.one;
            }

            Vector2Int size = new Vector2Int(colors[0].Length, colors.Length);
            Vector2Int position = FindValidPosition(ref size);
            
            while (position == -Vector2Int.one)
            {
                Expand();
                position = FindValidPosition(ref size);
            }
    
            DrawColors(position, colors, size);
            return position + new Vector2Int(padding, padding);
        }

        /// <summary>
        /// 创建并保存贴图文件
        /// </summary>
        /// <param name="name">文件名(不含扩展名)</param>
        /// <param name="savePath">保存路径,为空则使用 StreamingAssets</param>
        /// <returns>完整文件路径</returns>
        public string Create(string name = "Texture", string savePath = "")
        {
            Texture2D texture = GenerateTexture();
            
            // 确定保存路径
            if (string.IsNullOrEmpty(savePath))
            {
                savePath = Application.streamingAssetsPath;
            }
            else if (!Directory.Exists(savePath))
            {
                Debug.LogWarning($"路径不存在: {savePath}, 使用默认路径");
                savePath = Application.streamingAssetsPath;
            }

            // 确保目录存在
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            string fullPath = Path.Combine(savePath, $"{name}.png");
            SaveTexture(texture, fullPath);
            
            return fullPath;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 生成 Unity Texture2D 对象
        /// </summary>
        private Texture2D GenerateTexture()
        {
            Texture2D texture = new Texture2D(width, height);
            texture.filterMode = FilterMode.Point;
            
            // 使用 SetPixels32 批量设置
            Color32[] pixels = new Color32[width * height];
            int index = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color color = ColorsEqual(colorTable[x, y], SecondColor) ? DefaultColor : colorTable[x, y];
                    pixels[index++] = color;
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            
            return texture;
        }

        /// <summary>
        /// 保存贴图到文件
        /// </summary>
        private void SaveTexture(Texture2D texture, string path)
        {
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            Debug.Log($"成功创建图片: {path}");
        }

        /// <summary>
        /// 在指定位置绘制颜色区域
        /// </summary>
        private void DrawColors(Vector2Int position, Color[][] colors, Vector2Int size)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    int targetX = position.x + x;
                    int targetY = position.y + y;
                    
                    // 边界检查
                    // if (targetX >= width || targetY >= height)
                    //     continue;

                    // 判断是否在 padding 区域内
                    bool isPadding = x < padding || 
                                    x >= size.x - padding || 
                                    y < padding || 
                                    y >= size.y - padding;

                    if (isPadding)
                    {
                        colorTable[targetX, targetY] = SecondColor;
                    }
                    else
                    {
                        // 映射到 colors 数组的索引(去除padding偏移)
                        int colorX = x - padding;
                        int colorY = y - padding;
                        
                        // 默认色像素使用第二色占位
                        Color sourceColor = colors[colorY][colorX];
                        colorTable[targetX, targetY] = ColorsEqual(sourceColor, DefaultColor)? SecondColor : sourceColor;
                    }
                }
            }
        }
        
        /// <summary>
        /// 比较颜色
        /// </summary>
        /// <returns></returns>
        private bool ColorsEqual(Color a, Color b)
        {
            return Mathf.Approximately(a.r, b.r) && 
                   Mathf.Approximately(a.g, b.g) && 
                   Mathf.Approximately(a.b, b.b) && 
                   Mathf.Approximately(a.a, b.a);
        }


        /// <summary>
        /// 寻找可用的绘制位置
        /// </summary>
        private Vector2Int FindValidPosition(ref Vector2Int size)
        {
            // 需求区域包含 padding
            Vector2Int requiredSize = size + PaddingArea;
            
            // 检查是否需要归零指针
            bool needResetPointer = (pointer.x >= width || pointer.y >= height);
            if (needResetPointer)
            {
                pointer = Vector2Int.zero;
            }

            bool isZeroStart = (pointer == Vector2Int.zero);

            // 遍历查找可用位置
            for (int x = pointer.x; x < width; x++)
            {
                for (int y = pointer.y; y < height; y++)
                {
                    if (IsAreaAvailable(x, y, requiredSize))
                    {
                        MovePointerToNext(new Vector2Int(x, y), requiredSize);
                        // Debug.Log("当前图片放置的位置：" + x + "," + y);
                        return new Vector2Int(x, y);
                    }
                }
            }

            // 如果未从原点开始搜索,则从头再试一次
            if (!isZeroStart)
            {
                pointer = Vector2Int.zero;
                return FindValidPosition(ref size);
            }

            // 完全无法找到可用位置
            return -Vector2Int.one;
        }

        /// <summary>
        /// 检查指定区域是否可用(全为默认色)
        /// </summary>
        private bool IsAreaAvailable(int startX, int startY, Vector2Int size)
        {
            // 边界检查
            if (startX + size.x > width || startY + size.y > height)
                return false;

            // 逐像素检查
            for (int x = startX; x < startX + size.x; x++)
            {
                for (int y = startY; y < startY + size.y; y++)
                {
                    if (colorTable[x, y] != DefaultColor)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 移动指针到下一次检测区域的起点
        /// </summary>
        private void MovePointerToNext(Vector2Int targetPos, Vector2Int drawingAreaSize)
        {
            // 移动到当前区域右侧,继续横向搜索
            pointer = targetPos + new Vector2Int(drawingAreaSize.x, 0);
        }

        /// <summary>
        /// 扩大贴图像素(翻倍)
        /// </summary>
        private void Expand()
        {
            int newWidth = width * 2;
            int newHeight = height * 2;
            
            Color[,] newColorTable = new Color[newWidth, newHeight];
            
            // 复制原有数据
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    newColorTable[x, y] = colorTable[x, y];
                }
            }

            colorTable = newColorTable;
            width = newWidth;
            height = newHeight;
        }

        #endregion
    }
}
