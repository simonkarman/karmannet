using KarmanProtocol;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ServerUI : MonoBehaviour {
    [SerializeField]
    private Transform connectedClientUIParent = default;
    [SerializeField]
    private GameObject connectedClientUIPrefab = default;
    [SerializeField]
    private ServerFlow serverFlow = default;
    [SerializeField]
    private Text serverIdText = default;
    [SerializeField]
    private Text serverProtocolText = default;
    [SerializeField]
    private Text serverStatusText = default;
    [SerializeField]
    private Button scheduleShutdownButton = default;
    [SerializeField]
    private Color runningColor = Color.green;
    [SerializeField]
    private Color shutdownColor = Color.red;

    private KarmanServer karmanServer;
    private class ServerUIClientInfo {
        private Guid clientId;
        private bool connected;

        public ServerUIClientInfo(Guid clientId, bool connected) {
            this.clientId = clientId;
            this.connected = connected;
        }

        public Guid GetClientId() {
            return clientId;
        }

        public void SetConnected(bool connected) {
            this.connected = connected;
        }

        public bool IsConnected() {
            return connected;
        }
    }
    private readonly Dictionary<Guid, ServerUIClientInfo> clients = new Dictionary<Guid, ServerUIClientInfo>();

    protected void Start() {
        karmanServer = serverFlow.GetKarmanServer();
        serverProtocolText.text = KarmanServer.PROTOCOL_VERSION;
        serverIdText.text = karmanServer.id.ToString();
        karmanServer.OnRunningCallback += () => {
            serverStatusText.text = "Running";
            serverStatusText.color = runningColor;
            scheduleShutdownButton.interactable = true;
        };
        karmanServer.OnShutdownCallback += () => {
            serverStatusText.text = "Shutdown";
            serverStatusText.color = shutdownColor;
            scheduleShutdownButton.interactable = false;
        };
        karmanServer.OnClientJoinedCallback += (Guid clientId) => { clients.Add(clientId, new ServerUIClientInfo(clientId, false)); OnClientsChanged(); };
        karmanServer.OnClientConnectedCallback += (Guid clientId) => { clients[clientId].SetConnected(true); OnClientsChanged(); };
        karmanServer.OnClientDisconnectedCallback += (Guid clientId) => { clients[clientId].SetConnected(false); OnClientsChanged(); };
        karmanServer.OnClientLeftCallback += (Guid clientId) => { clients.Remove(clientId); OnClientsChanged(); };
    }

    private void OnClientsChanged() {
        List<ServerUIClientInfo> clients = new List<ServerUIClientInfo>(this.clients.Values);
        ServerUIClient[] playerUIs = connectedClientUIParent.GetComponentsInChildren<ServerUIClient>(true);
        for (int i = playerUIs.Length - 1; i >= clients.Count; i--) {
            playerUIs[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < clients.Count; i++) {
            ServerUIClient playerUI;
            if (playerUIs.Length <= i) {
                playerUI = Instantiate(connectedClientUIPrefab, connectedClientUIParent).GetComponent<ServerUIClient>();
                playerUI.transform.SetAsLastSibling();
            } else {
                playerUI = playerUIs[i];
                playerUI.gameObject.SetActive(true);
            }
            playerUI.SetFrom(serverFlow, clients[i].GetClientId(), clients[i].IsConnected());
        }
    }

    public void ScheduleShutdown() {
        serverFlow.ScheduleShutdown();
    }

    public void BackToMainMenu() {
        SceneManager.LoadScene("MainMenu");
    }
}

