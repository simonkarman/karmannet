using System;
using UnityEngine;
using UnityEngine.UI;

public class ServerUIClient : MonoBehaviour {
    [SerializeField]
    private Text clientIdText = default;
    [SerializeField]
    private Text clientTitleText = default;
    [SerializeField]
    private Text clientAverageLatencyText = default;

    private ServerFlow serverFlow;
    private Guid clientId;

    public void SetFrom(ServerFlow serverFlow, Guid clientId, string clientName, bool isConnected) {
        this.serverFlow = serverFlow;
        this.clientId = clientId;
        clientIdText.text = clientId.ToString();
        clientTitleText.text = clientName + (isConnected ? " (connected)" : " (not connected)");
    }

    public Guid GetClientId() {
        return clientId;
    }

    public void Kick() {
        serverFlow.GetKarmanServer().Kick(clientId, "Server admin decision");
    }

    public void SetAverageLatency(int averageLatency) {
        clientAverageLatencyText.text = string.Format("{0}ms", averageLatency);
    }
}
