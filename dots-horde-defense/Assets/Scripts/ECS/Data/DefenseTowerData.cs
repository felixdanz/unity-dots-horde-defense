using Unity.Entities;

public struct DefenseTowerData : IComponentData
{
	public Entity TowerHead;
	public Entity AttackTarget;
	public float AttackRange;
	public int AttackDamage;
}