using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 「首个场景加载完成」的跨场景执行宿主。
///
/// 背景（2026-09-22 微信小游戏容器日志取证）：首个场景用 <see cref="LoadSceneMode.Single"/> 加载，
/// `Boot` 场景被卸载时 `Init` 及其协程一并销毁 ⇒ 写在协程 `yield return` 之后的“加载后”代码
/// 永远不会执行（日志中该分支的 `WebGLFirstScreen` 输出 0 命中）。
/// 因此这段逻辑改由本对象承担：`Init` 在触发加载【之前】创建本对象并标记 `DontDestroyOnLoad`，
/// 等场景切换完成事件到达后执行一次，然后自毁。
///
/// 约束：本类**不声明任何 `[SerializeField]` 字段** —— 避免编辑器侧与播放器侧字段布局不一致，
/// 那会让 Unity 直接拒绝构建（`script class layout is incompatible between the editor and the player`）。
/// </summary>
public sealed class WebGLPostSceneLoadRunner : MonoBehaviour
{
    private bool _handled;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_handled)
            return;

        _handled = true;
        SceneManager.sceneLoaded -= OnSceneLoaded;

        Debug.Log($"首个场景已加载（sceneLoaded 事件）：{scene.name}，模式={mode}；由跨场景宿主执行加载后处理。");
        Init.RunPostSceneLoadCompatibility();
        Destroy(gameObject);
    }
}
