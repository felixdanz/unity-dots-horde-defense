using Unity.Entities;
using Unity.Mathematics;

public struct PathfindingData : IComponentData
{
	public int CurrentPathIndex;

	public float3 StartPosition;
	public float3 TargetPosition;
}