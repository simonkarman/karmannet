using KarmanNet.Networking;
using KarmanNet.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
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
    private Text numberOfClientsConnectedText = default;
    [SerializeField]
    private Button scheduleShutdownButton = default;
    [SerializeField]
    private Text scheduleShutdownText = default;
    [SerializeField]
    private Color runningColor = Color.green;
    [SerializeField]
    private Color shutdownColor = Color.red;
    [SerializeField]
    private Text acceptingClientsButtonText = default;
    [SerializeField]
    private Image acceptingClientsButtonBackground = default;
    [SerializeField]
    private Color acceptingClientsColor = Color.green;
    [SerializeField]
    private Color rejectingClientsColor = Color.red;

    private KarmanServer karmanServer;
    private class ServerUIClientInfo {
        private readonly Guid clientId;
        private readonly string clientName;
        private bool connected;

        public ServerUIClientInfo(Guid clientId, string clientName, bool connected) {
            this.clientId = clientId;
            this.clientName = clientName;
            this.connected = connected;
        }

        public Guid GetClientId() {
            return clientId;
        }

        public string GetClientName() {
            return clientName;
        }

        public void SetConnected(bool connected) {
            this.connected = connected;
        }

        public bool IsConnected() {
            return connected;
        }
    }
    private readonly Dictionary<Guid, ServerUIClientInfo> clients = new Dictionary<Guid, ServerUIClientInfo>();
    private bool showClients = false;
    private bool acceptingClients = false;

    protected void Start() {
        karmanServer = serverFlow.GetKarmanServer();
        serverProtocolText.text = KarmanServer.KARMAN_PROTOCOL_VERSION;
        serverIdText.text = karmanServer.id.ToString();
        karmanServer.OnClientAcceptanceCallback += (Action<string> reject) => {
            if (!acceptingClients) {
                reject("Server is not accepting new clients");
            }
        };
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
        karmanServer.OnClientJoinedCallback += (clientId, clientName) => { clients.Add(clientId, new ServerUIClientInfo(clientId, clientName, false)); OnClientsChanged(); };
        karmanServer.OnClientConnectedCallback += (clientId) => { clients[clientId].SetConnected(true); OnClientsChanged(); };
        karmanServer.OnClientDisconnectedCallback += (clientId) => { clients[clientId].SetConnected(false); OnClientsChanged(); };
        karmanServer.OnClientLeftCallback += (clientId, reason) => { clients.Remove(clientId); OnClientsChanged(); };

        serverFlow.OnShutdownTimeLeft += (int secondsLeft) => {
            if (secondsLeft == 0) {
                scheduleShutdownText.text = "Shutdown completed";
            } else {
                scheduleShutdownText.text = string.Format("Shutdown in {0} second(s)", secondsLeft);
            }
        };
        ToggleAcceptClients();

        LatencyOracle latencyOracle = serverFlow.GetComponentInChildren<LatencyOracle>();
        latencyOracle.OnClientAverageLatencyUpdatedCallback += OnClientAverageLatencyUpdated;
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
                playerUI.gameObject.SetActive(showClients);
            }
            playerUI.SetFrom(serverFlow, clients[i].GetClientId(), clients[i].GetClientName(), clients[i].IsConnected());
        }
        numberOfClientsConnectedText.text = string.Format("{0} client(s) connected", clients.Count);
    }

    private void OnClientAverageLatencyUpdated(Guid clientId, int averageLatency) {
        connectedClientUIParent
            .GetComponentsInChildren<ServerUIClient>(true)
            .Where(playerUI => playerUI.GetClientId().Equals(clientId))
            .First()
            .SetAverageLatency(averageLatency);
    }

    public void ScheduleShutdown() {
        serverFlow.ScheduleShutdown();
        if (acceptingClients) {
            ToggleAcceptClients();
        }
    }

    public void BackToMainMenu() {
        serverFlow.GetKarmanServer().Shutdown();
        ThreadManager.Flush();
        SceneManager.LoadScene("MainMenu");
    }

    public void ToggleShowClients() {
        showClients = !showClients;
        ServerUIClient[] playerUIs = connectedClientUIParent.GetComponentsInChildren<ServerUIClient>(true);
        for (int i = 0; i < clients.Count; i++) {
            playerUIs[i].gameObject.SetActive(showClients);
        }
    }

    public void ToggleAcceptClients() {
        acceptingClients = !acceptingClients;
        acceptingClientsButtonBackground.color = acceptingClients ? acceptingClientsColor : rejectingClientsColor;
        acceptingClientsButtonText.text = acceptingClients ? "Accepting new Clients" : "Rejecting new Clients";
    }
}

