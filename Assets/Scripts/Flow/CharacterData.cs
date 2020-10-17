using System;
using UnityEngine;

public class CharacterData {
    private readonly Guid id;
    private readonly Guid clientId;
    private readonly Color color;
    private readonly GameObject instance;

    private Vector2 lastSetPosition;

    public CharacterData(Guid id, Guid clientId, Vector2 position, Color color, GameObject instance) {
        this.id = id;
        this.clientId = clientId;
        this.color = color;
        this.instance = instance;
        instance.GetComponent<Character>().SetColor(color);
        instance.name = "Character " + id.ToString();
        SetPosition(position);
    }

    public CharacterSpawnPacket GetSpawnPacket() {
        return new CharacterSpawnPacket(id, clientId, GetActivePosition(), color);
    }

    public CharacterDestroyPacket GetDestroyPacket() {
        return new CharacterDestroyPacket(id);
    }

    public bool RequestPositionSyncCheck() {
        if ((lastSetPosition - GetActivePosition()).sqrMagnitude > 0.001f) {
            lastSetPosition = GetActivePosition();
            return true;
        }
        return false;
    }

    public CharacterUpdatePositionPacket GetUpdatePositionPacket() {
        return new CharacterUpdatePositionPacket(id, GetActivePosition());
    }

    public Vector2 GetActivePosition() {
        return instance.transform.position;
    }

    public void SetPosition(Vector2 position) {
        lastSetPosition = position;
        instance.transform.position = position;
    }

    public Guid GetId() {
        return id;
    }

    public void Destroy() {
        UnityEngine.Object.Destroy(instance);
    }
}