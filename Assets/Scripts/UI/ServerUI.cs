using System.Collections.Generic;
using UnityEngine;
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
    private Text serverNameText = default;
    [SerializeField]
    private Text serverProtocolText = default;
    [SerializeField]
    private Text serverStatusText = default;

    protected void Start() {
        serverFlow.OnClientsChanged += OnClientsChanged;
    }

    private void OnClientsChanged(IReadOnlyList<IConnectedKarmanClient> clients) {
        ServerUIConnectedClient[] playerUIs = connectedClientUIParent.GetComponentsInChildren<ServerUIConnectedClient>(true);
        for (int i = playerUIs.Length - 1; i >= clients.Count; i--) {
            playerUIs[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < clients.Count; i++) {
            ServerUIConnectedClient playerUI;
            if (playerUIs.Length <= i) {
                playerUI = Instantiate(connectedClientUIPrefab, connectedClientUIParent).GetComponent<ServerUIConnectedClient>();
                playerUI.transform.SetAsLastSibling();
            } else {
                playerUI = playerUIs[i];
                playerUI.gameObject.SetActive(true);
            }
            playerUI.SetFrom(serverFlow, clients[i]);
        }
    }

    protected void Update() {
        serverIdText.text = serverFlow.GetServerId().ToString();
        serverNameText.text = serverFlow.GetServerName();
        serverProtocolText.text = serverFlow.GetServerProtocolVersion();
        serverStatusText.text = "Server " + (serverFlow.IsServerRunning() ? "is running" : "was shutdown");
    }

    public void ScheduleShutdown() {
        serverFlow.ScheduleShutdown();
    }
}

