using KarmanProtocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerFlow : MonoBehaviour {
    public const int DEFAULT_SERVER_PORT = 14641;

    [SerializeField]
    private int startDelay = 2;

    private KarmanServer server;

    protected void Awake() {
        server = new KarmanServer();
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
        int shutdownDelay = 5;
        MessagePacket messagePacket = new MessagePacket(string.Format("The server is shutting down in {0} seconds!", shutdownDelay));
        server.Broadcast(messagePacket);
        yield return new WaitForSeconds(shutdownDelay);
        server.Shutdown();
    }
}
