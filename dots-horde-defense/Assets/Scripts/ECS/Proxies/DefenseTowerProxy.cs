using System.Collections.Generic;
using Unity.Entities;
using Unity.Physics.Authoring;
using UnityEngine;

public class DefenseTowerProxy : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
	[Header("References")]
	[SerializeField] private PhysicsShapeAuthoring physicsShapeAuthoring;
	[SerializeField] private Transform towerHead;
	
	[Header("Configuration")] 
	[SerializeField] private float attackCooldown;
	[SerializeField] private int attackDamage;
	
	
	public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
	{
		
	}
	
	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		dstManager.AddComponentData(entity, new DefenseTowerData()
		{
			TowerHead = conversionSystem.GetPrimaryEntity(towerHead),
			AttackRange = physicsShapeAuthoring.GetSphereProperties(out var orientation).Radius,
			AttackDamage = attackDamage,
		});
		
		dstManager.AddComponentData(entity, new TimerData()
		{
			Interval = attackCooldown,
		});
	}
}