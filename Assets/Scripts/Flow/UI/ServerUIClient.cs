using System;
using UnityEngine;
using UnityEngine.UI;

public class ServerUIClient : MonoBehaviour {
    [SerializeField]
    private Text clientIdText = default;
    [SerializeField]
    private Text clientConnectedText = default;
    [SerializeField]
    private Text clientAverageLatencyText = default;

    private ServerFlow serverFlow;
    private Guid clientId;

    public void SetFrom(ServerFlow serverFlow, Guid clientId, bool isConnected) {
        this.serverFlow = serverFlow;
        this.clientId = clientId;
        clientIdText.text = clientId.ToString();
        clientConnectedText.text = isConnected ? "Client (connected)" : "Client (not connected)";
    }

    public Guid GetClientId() {
        return clientId;
    }

    public void Kick() {
        serverFlow.GetKarmanServer().Kick(clientId);
    }

    public void SetAverageLatency(float averageLatency) {
        clientAverageLatencyText.text = string.Format("{0} sec", averageLatency.ToString("0.00"));
    }
}
