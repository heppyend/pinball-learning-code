using Pinball.Client.Services;
using Pinball.Client.UI;
using UnityEngine;

namespace Pinball.Client
{
    public sealed class ClientBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            EnsureUiClearCamera();
            ClientServices.InitializeForDevelopment();
            WireCompleteGuideEventPage();
            Debug.Log("[Client] Local development data initialized.");
        }

        private static void WireCompleteGuideEventPage()
        {
            // 场景（ClientShellStructuralRepair.RepairAll）已为该节点提供
            // ClientCompleteGuideEventPage 与 ClientUiPopup。此处只做控制器侧的依赖注入，
            // 不再创建任何组件或 UI。
            //
            // 关键：迁移后该节点位于 PopupLayer，由 ClientPopupService 统一管理可见性。
            // 这里绝不能再 AddComponent<ClientUiPage>()——那会让 ClientUiNavigator 的
            // 场景自动收录重新把它当页面，形成"两个所有者同时 SetActive"。
            ClientCompleteGuideEventPage page = Object.FindObjectOfType<ClientCompleteGuideEventPage>(true);
            if (page == null)
                return;

            ClientUiNavigator navigator = Object.FindObjectOfType<ClientUiNavigator>(true);
            ClientActivityPage activityPage = Object.FindObjectOfType<ClientActivityPage>(true);
            if (navigator != null)
                navigator.ConfigureCompleteGuideEntry(activityPage);
            page.Initialize(navigator, activityPage);
        }

        private static void EnsureUiClearCamera()
        {
            Camera[] cameras = Object.FindObjectsOfType<Camera>(true);
            foreach (Camera existing in cameras)
                if (existing != null && existing.enabled && existing.gameObject.activeInHierarchy)
                    return;

            GameObject cameraObject = new GameObject("Client UI Clear Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.cullingMask = 0;
            camera.depth = -100f;
        }
    }
}
