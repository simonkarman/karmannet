using UnityEngine;

public class ClientGameFlow : MonoBehaviour {
    [SerializeField]
    private float messageSendInternal = 10f;
    [SerializeField]
    private float stayConnectedDuration = 60f;

    private AsynchronousClient client = null;
    private float timeSinceLastMessage = 0f;

    public void Start() {
        string username = string.Format("User-" + (Random.value * 10000).ToString("0000"));
        string connectionString = "localhost";
        client = new AsynchronousClient(connectionString, username, MessageCallback);
        if (client.Connected) {
            client.Send(string.Format("Hello, server! My username is {0}.", username));
        }
    }

    public void Update() {
        if (client == null || !client.Connected) {
            client = null;
            return;
        }

        if (client.RealtimeSinceConnectionEstablished > stayConnectedDuration) {
            client.Disconnect();
        }

        timeSinceLastMessage += Time.deltaTime;
        if (timeSinceLastMessage >= messageSendInternal) {
            timeSinceLastMessage -= messageSendInternal;
            client.Send(string.Format("{0} has been around for {1} second(s).", client.username, client.RealtimeSinceConnectionEstablished));
        }
    }

    public void MessageCallback(string message) {
        new GameObject(message).transform.parent = transform;
    }
}
