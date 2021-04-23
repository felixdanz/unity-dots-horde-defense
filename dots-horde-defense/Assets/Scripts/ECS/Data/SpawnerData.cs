using Unity.Entities;

public struct SpawnerData : IComponentData
{
	public Entity SpawnPoint;
	public Entity TargetPoint;
	public Entity EntityToSpawn;
}