using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SpawnerProxy : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
	[Header("References")]
	[SerializeField] private Transform spawnPoint;
	[SerializeField] private Transform targetPoint;
    
	[Header("Configuration")]
	[SerializeField] private float productionTime;
	[SerializeField] private GameObject entityToSpawn;


	public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
	{
		referencedPrefabs.Add(entityToSpawn);
	}
    
	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		dstManager.AddComponentData(entity, new SpawnerData()
		{
			SpawnPoint = conversionSystem.GetPrimaryEntity(spawnPoint),
			TargetPoint = conversionSystem.GetPrimaryEntity(targetPoint),
			EntityToSpawn = conversionSystem.GetPrimaryEntity(entityToSpawn),
		});
        
		dstManager.AddComponentData(entity, new TimerData
		{
			Interval = productionTime,
		});
	}
}