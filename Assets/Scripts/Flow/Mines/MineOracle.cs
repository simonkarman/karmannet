using KarmanNet.Protocol;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MineOracle : MonoBehaviour {
    [SerializeField]
    private ServerFlow serverFlow = default;
    [SerializeField]
    private GameObject minePrefab = default;
    [SerializeField]
    private float initialSpawnDelay = 15f;
    [SerializeField]
    private float spawnRate = 2f;
    [SerializeField]
    private float minMineDuration = 2.5f;
    [SerializeField]
    private float maxMineDuration = 4f;
    [SerializeField]
    private Bounds spawnBounds = new Bounds(Vector3.zero, Vector3.one);

    private KarmanServer karmanServer;
    private readonly Dictionary<Guid, MineData> mines = new Dictionary<Guid, MineData>();
    private float nextSpawnMoment;

    protected void Start() {
        karmanServer = serverFlow.GetKarmanServer();
        karmanServer.OnClientJoinedCallback += OnClientJoined;
        nextSpawnMoment = initialSpawnDelay;

        enabled = false;
        karmanServer.OnRunningCallback += () => enabled = true;
        karmanServer.OnShutdownCallback += () => enabled = false;
    }

    private void OnClientJoined(Guid clientId, string clientName) {
        Debug.Log(string.Format("Sending all existing mines to client {0} because it joined the server", clientId));
        foreach (var mine in mines.Values) {
            karmanServer.Send(clientId, mine.GetSpawnPacket());
        }
    }

    private static Vector3 RandomPointInBounds(Bounds bounds) {
        float x = bounds.size.x * UnityEngine.Random.value;
        float y = bounds.size.y * UnityEngine.Random.value;
        float z = bounds.size.z * UnityEngine.Random.value;
        return bounds.min + new Vector3(x, y, z);
    }

    protected void FixedUpdate() {
        if (nextSpawnMoment <= Time.timeSinceLevelLoad) {
            nextSpawnMoment += spawnRate;
            GameObject instance = Instantiate(minePrefab, transform);
            MineData mine = new MineData(Guid.NewGuid(), transform.position + RandomPointInBounds(spawnBounds), UnityEngine.Random.Range(minMineDuration, maxMineDuration), instance);
            Debug.Log("Spawning a new mine: " + mine.GetId());
            karmanServer.Broadcast(mine.GetSpawnPacket());
        }
    }

    protected void OnDrawGizmosSelected() {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(spawnBounds.center, spawnBounds.size);
    }
}
