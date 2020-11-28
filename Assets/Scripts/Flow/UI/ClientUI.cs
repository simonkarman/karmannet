using KarmanProtocol;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ClientUI : MonoBehaviour {
    [SerializeField]
    private ClientFlow clientFlow = default;
    [SerializeField]
    private Text clientIdText = default;
    [SerializeField]
    private Text clientProtocolText = default;
    [SerializeField]
    private Text clientStatusText = default;
    [SerializeField]
    private Text clientConnectionStatusText = default;
    [SerializeField]
    private Button leaveButton = default;
    [SerializeField]
    private Color successColor = Color.green;
    [SerializeField]
    private Color failureColor = Color.red;

    private KarmanClient karmanClient;
    protected void Start() {
        karmanClient = clientFlow.GetKarmanClient();
        clientIdText.text = karmanClient.id.ToString();
        clientProtocolText.text = KarmanServer.PROTOCOL_VERSION;
        karmanClient.OnJoinedCallback += () => {
            clientStatusText.text = "Joined";
            clientStatusText.color = successColor;
        };
        karmanClient.OnConnectedCallback += () => {
            clientConnectionStatusText.text = "Connected";
            clientConnectionStatusText.color = successColor;
            leaveButton.interactable = true;
        };
        karmanClient.OnDisconnectedCallback += () => {
            clientConnectionStatusText.text = "Not connected";
            clientConnectionStatusText.color = failureColor;
            leaveButton.interactable = false;
        };
        karmanClient.OnLeftCallback += () => {
            clientStatusText.text = "Left";
            clientStatusText.color = failureColor;
        };
    }

    public void Leave() {
        karmanClient.Leave("Client decision");
    }

    public void BackToMainMenu() {
        SceneManager.LoadScene("MainMenu");
    }
}
