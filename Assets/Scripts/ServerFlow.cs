using System;
using System.Text;
using UnityEngine;
using Networking;

public class ServerFlow : MonoBehaviour {
    [SerializeField]
    private float lifespanNotificationInterval = 10f;
    [SerializeField]
    private float lifespan = 120f;
    [SerializeField]
    private float lifespanServerWarningThreshold = 60f;

    private float timeSinceLastLifespanNotification;
    private MultiplayerServer server;

    protected void Start() {
        server = new MultiplayerServer(OnConnected, OnDisconnected, OnFrameReceived);
    }

    public void OnConnected(Guid connectionId) {
        Debug.Log(string.Format("ServerFlow: Client {0} connected", connectionId));
        server.Send(connectionId, Encoding.ASCII.GetBytes(string.Format("Welcome {0}. You're now connected to the server!", connectionId)));
    }

    public void OnDisconnected(Guid connectionId) {
        Debug.Log(string.Format("ServerFlow: Client {0} disconnected", connectionId));
    }

    public void OnFrameReceived(Guid connectionId, byte[] frame) {
        Debug.Log(string.Format("ServerFlow: Client {0} says: {1}", connectionId, Encoding.ASCII.GetString(frame)));
    }

    protected void Update() {
        if (server.Status == ServerStatus.SHUTDOWN) {
            return;
        }

        if (server.RealtimeSinceStarted > lifespan) {
            server.Shutdown();
        }

        timeSinceLastLifespanNotification += Time.deltaTime;
        if (timeSinceLastLifespanNotification > lifespanNotificationInterval) {
            timeSinceLastLifespanNotification -= lifespanNotificationInterval;

            if (lifespan - server.RealtimeSinceStarted < lifespanServerWarningThreshold) {
                string message = string.Format("ServerFlow: Server is shutting down in {0} second(s)", (lifespan - server.RealtimeSinceStarted).ToString("0"));
                Debug.LogWarning(message);
                server.Broadcast(Encoding.ASCII.GetBytes(message));
            }
        }
    }

    protected void OnDestroy() {
        if (server.Status == ServerStatus.RUNNING) {
            server.Shutdown();
        }
    }
}
