using UnityEngine;
using UnityEngine.UI;

namespace Mirror
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/Network Manager HUD")]
    [RequireComponent(typeof(NetworkManager))]
    [HelpURL("https://mirror-networking.gitbook.io/docs/components/network-manager-hud")]
    public class NetworkManagerHUDWorldSpace : MonoBehaviour
    {
        NetworkManager manager;

        public Canvas canvas; // Assign your World Space Canvas in the Inspector
        public Button hostButton;
        public Button clientButton;
        public Button serverButton;
        public Button stopButton;
        public InputField addressInput;
        public Text statusText;

        void Awake()
        {
            manager = GetComponent<NetworkManager>();
            SetupUI();
        }

        void SetupUI()
        {
            // Ensure all buttons are wired up
            if (hostButton != null)
                hostButton.onClick.AddListener(StartHost);

            if (clientButton != null)
                clientButton.onClick.AddListener(StartClient);

            if (serverButton != null)
                serverButton.onClick.AddListener(StartServer);

            if (stopButton != null)
                stopButton.onClick.AddListener(StopEverything);
        }

        void Update()
        {
            UpdateStatus();
        }

        void StartHost()
        {
            manager.StartHost();
        }

        void StartClient()
        {
            manager.networkAddress = addressInput != null ? addressInput.text : "localhost";
            manager.StartClient();
        }

        void StartServer()
        {
            manager.StartServer();
        }

        void StopEverything()
        {
            if (NetworkServer.active && NetworkClient.isConnected)
                manager.StopHost();
            else if (NetworkClient.isConnected)
                manager.StopClient();
            else if (NetworkServer.active)
                manager.StopServer();
        }

        void UpdateStatus()
        {
            if (statusText != null)
            {
                if (NetworkServer.active && NetworkClient.active)
                    statusText.text = "Host: Running";
                else if (NetworkServer.active)
                    statusText.text = "Server: Running";
                else if (NetworkClient.isConnected)
                    statusText.text = $"Client: Connected to {manager.networkAddress}";
                else
                    statusText.text = "Not connected";
            }
        }
    }
}
