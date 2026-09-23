using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client.UI
{
    /// <summary>
    /// 最小 UI 性能探针（诊断用，可随时移除）。
    ///
    /// 目的：为"Play 模式发卡 / 点击后延迟但最终生效"提供**日志证据**，而不是靠现象猜根因。
    /// 每 <see cref="_reportEveryFrames"/> 帧汇总一次：平均帧时、最差帧时、超阈帧数，
    /// 以及当时的场景规模（活跃 GameObject 数、可射线 Graphic 数、启用的 Animator 数）。
    ///
    /// 用法：挂在 ClientCanvas 上（ClientShellController 会自动补挂）。
    /// 关闭：把 _enabled 取消勾选，或直接删掉该组件。
    /// **2026-09-20 起默认关闭** —— 每 300 帧的汇总（含"场景规模"遍历，会是控制台最大噪声源）
    /// 在排查 UI 交互问题时反而掩盖关键日志。需要时再勾上。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ClientUiPerfProbe : MonoBehaviour
    {
        [Tooltip("是否采样。诊断结束后取消勾选即可完全停止开销。默认关闭。")]
        [SerializeField] private bool _enabled = false;

        [Tooltip("帧时超过该值（毫秒）记一次超阈。33ms ≈ 低于 30FPS。")]
        [SerializeField] private float _spikeThresholdMs = 33f;

        [Tooltip("每多少帧汇总输出一次。")]
        [SerializeField] private int _reportEveryFrames = 300;

        [Tooltip("是否同时记录场景规模（遍历开销较大，用低频汇总即可掩盖）。")]
        [SerializeField] private bool _countSceneScale = true;

        private readonly StringBuilder _builder = new StringBuilder(256);
        private int _frames;
        private float _accumMs;
        private float _worstMs;
        private int _spikeCount;
        private int _worstFrameNumber;
        private int _totalFrames;

        private void Update()
        {
            if (!_enabled)
                return;

            float ms = Time.unscaledDeltaTime * 1000f;
            _totalFrames++;
            _frames++;
            _accumMs += ms;

            if (ms > _worstMs)
            {
                _worstMs = ms;
                _worstFrameNumber = _totalFrames;
            }
            if (ms > _spikeThresholdMs)
                _spikeCount++;

            if (_frames < Mathf.Max(30, _reportEveryFrames))
                return;

            _builder.Length = 0;
            _builder.Append("近 ").Append(_frames).Append(" 帧：平均 ")
                    .Append((_accumMs / _frames).ToString("F1")).Append("ms")
                    .Append("（约 ").Append((1000f / Mathf.Max(0.01f, _accumMs / _frames)).ToString("F0")).Append(" FPS）")
                    .Append("  最差 ").Append(_worstMs.ToString("F1")).Append("ms@帧").Append(_worstFrameNumber)
                    .Append("  超阈(>").Append(_spikeThresholdMs.ToString("F0")).Append("ms)帧数 ")
                    .Append(_spikeCount).Append("/").Append(_frames);

            if (_countSceneScale)
            {
                _builder.Append("\n    场景规模：活跃 GameObject=").Append(CountActiveObjects())
                        .Append("  可射线 Graphic=").Append(CountRaycastableGraphics())
                        .Append("  启用 Animator=").Append(CountEnabledAnimators());
            }

            ClientUiTrace.Line("性能", _builder.ToString());

            _frames = 0;
            _accumMs = 0f;
            _worstMs = 0f;
            _spikeCount = 0;
        }

        private static int CountActiveObjects()
        {
            Transform[] all = Object.FindObjectsOfType<Transform>(true);
            int count = 0;
            for (int i = 0; i < all.Length; i++)
                if (all[i] != null && all[i].gameObject.activeInHierarchy)
                    count++;
            return count;
        }

        private static int CountRaycastableGraphics()
        {
            Graphic[] graphics = Object.FindObjectsOfType<Graphic>(true);
            int count = 0;
            for (int i = 0; i < graphics.Length; i++)
                if (graphics[i] != null && graphics[i].raycastTarget && graphics[i].gameObject.activeInHierarchy)
                    count++;
            return count;
        }

        private static int CountEnabledAnimators()
        {
            Animator[] animators = Object.FindObjectsOfType<Animator>(true);
            int count = 0;
            for (int i = 0; i < animators.Length; i++)
                if (animators[i] != null && animators[i].isActiveAndEnabled)
                    count++;
            return count;
        }
    }
}
