using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class FactoryProxy : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    [Header("References")]
    [SerializeField] private Transform spawnPoint;
    
    [Header("Configuration")] 
    [SerializeField] private float productionTime;
    [SerializeField] private List<GameObject> buildQueue;


    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.AddRange(buildQueue);
    }
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new FactoryData
        {
            SpawnPoint = conversionSystem.GetPrimaryEntity(spawnPoint),
        });
        
        dstManager.AddComponentData(entity, new TimerData
        {
            Interval = productionTime,
        });
        
        // TODO(FD): later replace with empty queue at game start
        var buildQueueBuffer = dstManager.AddBuffer<EntitySpawnQueueElement>(entity);
        foreach (var entry in buildQueue)
        {
            buildQueueBuffer.Add(new EntitySpawnQueueElement
            {
                Value = conversionSystem.GetPrimaryEntity(entry),
                SpawnTime = productionTime,
            });
        }
    }
}
