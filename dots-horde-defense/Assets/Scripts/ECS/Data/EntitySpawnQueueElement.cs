using Unity.Entities;

public struct EntitySpawnQueueElement : IBufferElementData
{
    public Entity Value;
    public float SpawnTime;
}