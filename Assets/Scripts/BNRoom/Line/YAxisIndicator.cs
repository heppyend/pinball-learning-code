using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class YAxisIndicator : MonoBehaviour
{
    public float axisLength = 2f; // Y轴指示线长度

    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        // 使用本地坐标，线随物体移动/旋转
        lineRenderer.useWorldSpace = false;

        // 2个点：起点和终点
        lineRenderer.positionCount = 2;

        // 起点在物体原点，终点沿Y轴向上
        lineRenderer.SetPosition(0, new Vector3(0,0.25f,0));
        lineRenderer.SetPosition(1, Vector3.up * axisLength);
    }
}
