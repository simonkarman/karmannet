using System;
using UnityEngine;
using UnityEngine.UI;

public class ServerUIClient : MonoBehaviour {
    [SerializeField]
    private Text clientIdText = default;
    [SerializeField]
    private Text clientConnectedText = default;

    private ServerFlow serverFlow;
    private Guid clientId;

    public void SetFrom(ServerFlow serverFlow, Guid clientId, bool isConnected) {
        this.serverFlow = serverFlow;
        this.clientId = clientId;
        clientIdText.text = clientId.ToString();
        clientConnectedText.text = isConnected ? "Client (connected)" : "Client (not connected)";
    }

    public void Kick() {
        serverFlow.GetKarmanServer().Kick(clientId);
    }
}
