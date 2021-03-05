using System;
using Unity.Entities;
using Unity.Mathematics;

public struct PathfindingData : ISharedComponentData, IEquatable<PathfindingData>
{
	public float3[] FlowField;
	
	
	public bool Equals(PathfindingData other)
	{
		return Equals(FlowField, other.FlowField);
	}

	public override bool Equals(object obj)
	{
		return obj is PathfindingData other && Equals(other);
	}

	public override int GetHashCode()
	{
		return (FlowField != null ? FlowField.GetHashCode() : 0);
	}
}