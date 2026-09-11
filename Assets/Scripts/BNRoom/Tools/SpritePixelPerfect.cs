using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpritePixelPerfect : MonoBehaviour
{
    [Header("自动计算")]
    public bool autoFit = true;          // 是否自动适配
    public float manualScale = 1f;       // 手动缩放值

    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;

        if (autoFit && mainCamera != null && spriteRenderer.sprite != null)
        {
            FitPixelPerfect();
        }
    }

    void FitPixelPerfect()
    {
        // 获取精灵的纹理像素尺寸
        int spritePixelWidth = spriteRenderer.sprite.texture.width;
        int spritePixelHeight = spriteRenderer.sprite.texture.height;

        // 获取摄像机的正交大小和屏幕高度
        float orthoSize = mainCamera.orthographicSize;
        float screenHeight = Screen.height;

        // 计算需要放大的倍数（使1像素纹理对应1像素屏幕）
        // 假设精灵的 PPU 是导入时设定的值
        float ppu = spriteRenderer.sprite.pixelsPerUnit;

        // 缩放公式：scale = ppu * 2 * orthoSize / screenHeight
        float scale = ppu * 2f * orthoSize / screenHeight;

        // 应用缩放（保持宽高比一致）
        transform.localScale = Vector3.one * scale;
        // Debug.LogError(spriteRenderer.sprite.name + " : " + spriteRenderer.bounds.size.x);
    }

    // 如果屏幕大小变化（如窗口调整），可实时更新
    void Update()
    {
        return;
        if (autoFit && mainCamera != null && spriteRenderer.sprite != null)
        {
            FitPixelPerfect();
        }
    }
}