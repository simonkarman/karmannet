using UnityEngine;
using UnityEngine.UI;

public class ServerUIConnectedClient : MonoBehaviour {
    [SerializeField]
    private Text clientIdText = default;
    [SerializeField]
    private Text clientNameText = default;

    private ServerFlow serverFlow;
    private IConnectedKarmanClient connectedKarmanClient;

    public void SetFrom(ServerFlow serverFlow, IConnectedKarmanClient connectedKarmanClient) {
        this.serverFlow = serverFlow;
        this.connectedKarmanClient = connectedKarmanClient;
        clientIdText.text = connectedKarmanClient.GetClientId().ToString();
        clientNameText.text = connectedKarmanClient.GetClientName();
    }

    public void Kick() {
        serverFlow.Kick(connectedKarmanClient.GetClientId());
    }
}
