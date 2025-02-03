using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mirror.Discovery
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/My Network Discovery HUD")]
    public class MyNetworkDiscoveryHud : MonoBehaviour
    {
        readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();

        public NetworkDiscovery networkDiscovery;

        [Header("UI Elements")]
        public Button findServersButton;
        public Button startHostButton;
        public Button startServerButton;
        public Button stopButton;

        void Start()
        {
            if (networkDiscovery == null)
                networkDiscovery = GetComponent<NetworkDiscovery>();

            // Attach button actions
            findServersButton.onClick.AddListener(FindServers);
            startHostButton.onClick.AddListener(StartHost);
            startServerButton.onClick.AddListener(StartServer);
            stopButton.onClick.AddListener(Stop);
        }

        void FindServers()
        {
            discoveredServers.Clear();
            networkDiscovery.StartDiscovery();
        }

        void StartHost()
        {
            discoveredServers.Clear();
            NetworkManager.singleton.StartHost();
            networkDiscovery.AdvertiseServer();
        }

        void StartServer()
        {
            discoveredServers.Clear();
            NetworkManager.singleton.StartServer();
            networkDiscovery.AdvertiseServer();
        }

        void Stop()
        {
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopHost();
            }
            else if (NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopClient();
            }
            else if (NetworkServer.active)
            {
                NetworkManager.singleton.StopServer();
            }

            networkDiscovery.StopDiscovery();
        }

        public void OnDiscoveredServer(ServerResponse info)
        {
            if (!discoveredServers.ContainsKey(info.serverId))
            {
                discoveredServers[info.serverId] = info;

                // Automatically connect to the first found server
                if (discoveredServers.Count == 1)
                {
                    Connect(info);
                }
            }
        }

        void Connect(ServerResponse info)
        {
            Debug.Log($"Connecting to server at {info.EndPoint.Address}");
            networkDiscovery.StopDiscovery();
            NetworkManager.singleton.StartClient(info.uri);
        }
    }
}
