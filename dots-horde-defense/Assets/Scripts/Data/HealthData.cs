using Unity.Entities;

public struct HealthData : IComponentData
{
	public int HealthMax;
	public int HealthCurrent;
}