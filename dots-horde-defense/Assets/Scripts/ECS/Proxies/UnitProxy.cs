using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public class UnitProxy : MonoBehaviour, IConvertGameObjectToEntity
{
	[Header("References")]
	[SerializeField] private GameObject markerSelected;

	[Header("Configuration")] 
	[SerializeField] private float movementSpeed;

	[Header("Testing")]
	[SerializeField] [Range(0, 1)] private int team;


	public void Convert(
		Entity entity, 
		EntityManager dstManager, 
		GameObjectConversionSystem conversionSystem)
	{
		var entityMarkerSelected = conversionSystem.GetPrimaryEntity(markerSelected);
		
		dstManager.AddComponent<DisableRendering>(entityMarkerSelected);
		
		dstManager.AddComponentData(entity, new UnitData
		{
			MarkerSelected = entityMarkerSelected,
		});
		
		dstManager.AddComponentData(entity, new MovementSpeed()
		{
			Value = movementSpeed,
		});

		dstManager.AddComponentData(entity, new HealthData()
		{
			HealthMax = 10,
			HealthCurrent = 10,
		});

		if (team == 0)
		{
			dstManager.AddComponent<Tag_TeamA>(entity);
		}
		else
		{
			dstManager.AddComponent<Tag_TeamB>(entity);
		}
		
		// pathfinding testing
		dstManager.AddBuffer<PathPointElement>(entity);
		dstManager.AddComponentData(entity, new PathfindingData()
		{
			CurrentPathIndex = -1,
		});
	}
}
