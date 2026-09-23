using Pinball.Client;
using UnityEditor;
using UnityEngine;

namespace Pinball.Client.Editor
{
    [CustomEditor(typeof(ClientLoadingPage))]
    public sealed class ClientLoadingPageEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
            {
                if (GUILayout.Button("播放 5 秒加载演示", GUILayout.Height(30f)))
                {
                    foreach (Object selectedTarget in targets)
                        ((ClientLoadingPage)selectedTarget).PlayFiveSecondPreview();
                }

                if (GUILayout.Button("走 ClientLoadingService 演示（顺带验证 SystemLayer 开关）", GUILayout.Height(26f)))
                    ClientLoadingDemo.Play();
            }

            if (!EditorApplication.isPlaying)
                EditorGUILayout.HelpBox("进入 Play 模式后按钮可用。演示会模拟分段到达的真实加载进度，并平滑追赶到 100%。", MessageType.Info);
        }
    }

    /// <summary>
    /// Play 模式下的加载页演示 —— **走 `ClientLoadingService`**，因此同时验证
    /// “打开/关闭恒 inactive 的 `SystemLayer`”这条真实链路（Inspector 上那个按钮只切页面自身）。
    ///
    /// 用 `EditorApplication.update` 计时，**不往场景里塞任何对象**，也不进 Player 构建。
    /// 也是启动流程接线的**参照实现**：`Begin → SetProgress/SetLoadingText → Complete → Hide`。
    /// </summary>
    public static class ClientLoadingDemo
    {
        private const double TotalSeconds = 5.0;
        private static readonly float[] Checkpoints = { 0f, 0.08f, 0.22f, 0.47f, 0.63f, 0.82f, 0.94f, 1f };

        private static int _step;
        private static double _endTime;
        private static double _nextStepTime;
        private static bool _running;

        [MenuItem("Client/加载页/演示 5 秒加载（走 ClientLoadingService）", false, 40)]
        public static void Play()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[Client] 请先进入 Play 模式，再用此入口演示加载页。");
                return;
            }

            if (_running)
                Stop();

            ClientLoadingService.Begin("正在检查更新");
            _step = 1;
            _running = true;
            _endTime = EditorApplication.timeSinceStartup + TotalSeconds;
            _nextStepTime = EditorApplication.timeSinceStartup;
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }

        private static void Tick()
        {
            double now = EditorApplication.timeSinceStartup;

            if (_step < Checkpoints.Length && now >= _nextStepTime)
            {
                float value = Checkpoints[_step];
                ClientLoadingService.SetProgress(value);
                ClientLoadingService.SetLoadingText(value < 0.5f ? "正在检查更新" : "正在下载资源");
                _step++;
                _nextStepTime = now + (TotalSeconds / Checkpoints.Length);
            }

            if (now < _endTime)
                return;

            ClientLoadingService.Complete();
            ClientLoadingService.Hide();
            Debug.Log("[Client] 加载页演示结束（Complete + Hide，SystemLayer 已关闭）。");
            Stop();
        }

        private static void Stop()
        {
            _running = false;
            EditorApplication.update -= Tick;
        }
    }
}
