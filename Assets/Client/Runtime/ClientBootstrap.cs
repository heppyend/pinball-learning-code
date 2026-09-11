using Pinball.Client.Services;
using UnityEngine;

namespace Pinball.Client
{
    public sealed class ClientBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            ClientServices.InitializeForDevelopment();
            Debug.Log("[Client] Local development data initialized.");
        }
    }
}
