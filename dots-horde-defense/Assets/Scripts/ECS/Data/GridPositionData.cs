using Unity.Entities;

public struct GridPositionData : IComponentData
{
	public int IndexInGrid;
	public int X;
	public int Y;
}