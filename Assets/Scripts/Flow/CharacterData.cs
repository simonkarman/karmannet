using System;
using UnityEngine;

public class CharacterData {
    private readonly Guid id;
    private readonly Guid clientId;
    private readonly Color color;
    private readonly GameObject instance;

    private Vector2 syncedPosition;
    public Vector2 Position {
        get {
            return instance.transform.position;
        }
        private set {
            syncedPosition = value;
            instance.transform.position = value;
        }
    }

    public CharacterData(Guid id, Guid clientId, Vector2 position, Color color, GameObject instance) {
        this.id = id;
        this.clientId = clientId;
        this.color = color;
        this.instance = instance;
        instance.GetComponent<Character>().SetColor(color);
        instance.name = "Character " + id.ToString();
        Position = position;
    }

    public CharacterSpawnPacket GetSpawnPacket() {
        return new CharacterSpawnPacket(id, clientId, Position, color);
    }

    public CharacterDestroyPacket GetDestroyPacket() {
        return new CharacterDestroyPacket(id);
    }

    public bool RequestPositionSyncCheck() {
        if ((syncedPosition - Position).sqrMagnitude > 0.001f) {
            syncedPosition = Position;
            return true;
        }
        return false;
    }

    public CharacterUpdatePositionPacket GetUpdatePositionPacket() {
        return new CharacterUpdatePositionPacket(id, Position);
    }

    public void SetPosition(Vector2 position) {
        Position = position;
    }

    public Guid GetId() {
        return id;
    }

    public void Destroy() {
        UnityEngine.Object.Destroy(instance);
    }
}