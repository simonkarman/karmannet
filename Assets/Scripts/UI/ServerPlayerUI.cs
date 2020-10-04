using UnityEngine;
using UnityEngine.UI;

public class ServerPlayerUI : MonoBehaviour {
    [SerializeField]
    private Text connectionIdText = default;
    [SerializeField]
    private Text nameText = default;
    [SerializeField]
    private Text lastReceivedMessageText = default;

    private ServerFlow.ServerFlowPlayer serverFlowPlayer;

    public void SetFrom(ServerFlow.ServerFlowPlayer serverFlowPlayer) {
        this.serverFlowPlayer = serverFlowPlayer;
        connectionIdText.text = serverFlowPlayer.GetConnectionId().ToString();
        nameText.text = serverFlowPlayer.GetName();
        lastReceivedMessageText.text = serverFlowPlayer.GetLastReceivedMessage();
    }

    public void Kick() {
        serverFlowPlayer.Kick();
    }
}
