using System;
using UnityEngine;

public class MineData {
    private readonly Guid id;
    private readonly float timeOfExplosion;
    private readonly GameObject instance;

    public MineData(Guid id, Vector2 position, float duration, GameObject instance) {
        this.id = id;
        this.instance = instance;
        timeOfExplosion = Time.timeSinceLevelLoad + duration;
        instance.GetComponent<Mine>().SetDuration(duration);
        instance.name = "Mine " + id.ToString();
        instance.transform.position = position;
    }

    public MineSpawnPacket GetSpawnPacket() {
        return new MineSpawnPacket(id, instance.transform.position, timeOfExplosion - Time.timeSinceLevelLoad);
    }

    public Guid GetId() {
        return id;
    }
}