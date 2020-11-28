using KarmanProtocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerFlow : MonoBehaviour {
    public const int DEFAULT_SERVER_PORT = 14641;
    public static readonly Guid GAME_ID = Guid.Parse("60b2225e-bdb8-4235-a3bf-85c563eb9c86");

    [SerializeField]
    private int startDelay = 2;
    [SerializeField]
    private int maxNumberOfClients = 2;

    private KarmanServer server;

    public Action<int> OnShutdownTimeLeft;

    protected void Awake() {
        server = new KarmanServer(GAME_ID);
        server.OnClientAcceptanceCallback += (Action<string> reject) => {
            if (server.GetClientCount() >= maxNumberOfClients) {
                reject("Server full");
            }
        };
    }

    protected IEnumerator Start() {
        Debug.Log(string.Format("Server is starting in {0} second(s)", startDelay));
        yield return new WaitForSeconds(startDelay);
        server.Start(DEFAULT_SERVER_PORT);
    }

    protected void OnDestroy() {
        if (server.IsRunning()) {
            server.Shutdown();
        }
    }

    public KarmanServer GetKarmanServer() {
        return server;
    }

    public void ScheduleShutdown() {
        StartCoroutine(DoScheduledShutdown());
    }

    private IEnumerator<YieldInstruction> DoScheduledShutdown() {
        int shutdownTimeLeft = 5;
        do {
            MessagePacket messagePacket = new MessagePacket(string.Format("The server is shutting down in {0} seconds!", shutdownTimeLeft));
            server.Broadcast(messagePacket);
            OnShutdownTimeLeft(shutdownTimeLeft);
            yield return new WaitForSeconds(1);
            shutdownTimeLeft -= 1;
        } while (shutdownTimeLeft > 0);
        server.Shutdown();
        OnShutdownTimeLeft(0);
    }
}
