using System;
using UnityEngine;
using UnityEngine.UI;

public class ImageMarquee : MonoBehaviour
{
    public float speed = 0.5f;
    private Material mat;

    void Start()
    {
        // 获取材质实例（确保是独立材质）
        mat = GetComponent<Image>().material;
        // 如果不想创建材质，也可用 materialForRendering
        // mat = GetComponent<Image>().materialForRendering; 
    }

    void Update()
    {
        float offset = (mat.mainTextureOffset.x + speed * Time.deltaTime) % 1f;
        mat.mainTextureOffset = new Vector2(offset, 0);
    }

    private void OnDestroy()
    {
        mat.mainTextureOffset = Vector2.zero;
    }
}