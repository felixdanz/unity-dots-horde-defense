using Unity.Entities;
using Unity.Mathematics;

public struct RequestPathfindingData : IComponentData
{
	public float3 StartPosition;
	public float3 TargetPosition;
}