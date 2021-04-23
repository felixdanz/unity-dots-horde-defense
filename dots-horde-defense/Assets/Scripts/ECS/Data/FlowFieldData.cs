using Unity.Entities;
using Unity.Mathematics;

public struct FlowFieldData : IComponentData
{
	public int2 FlowFieldKey;
	public float3 TargetPosition;
}