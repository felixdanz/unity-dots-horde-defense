using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public class WallProxy : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
	[Header("References")]
	[SerializeField] private Transform topPart;
	[SerializeField] private Transform rightPart;
	[SerializeField] private Transform botPart;
	[SerializeField] private Transform leftPart;
	
	[Header("Configuration")] 
	[SerializeField] private int health;
	

	public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
	{
		
	}
	
	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		var entityTopPart = conversionSystem.GetPrimaryEntity(topPart);
		var entityRightPart = conversionSystem.GetPrimaryEntity(rightPart);
		var entityBotPart = conversionSystem.GetPrimaryEntity(botPart);
		var entityLeftPart = conversionSystem.GetPrimaryEntity(leftPart);
		
		dstManager.AddComponent<DisableRendering>(entityTopPart);
		dstManager.AddComponent<DisableRendering>(entityRightPart);
		dstManager.AddComponent<DisableRendering>(entityBotPart);
		dstManager.AddComponent<DisableRendering>(entityLeftPart);
		
		dstManager.AddComponentData(entity, new WallData()
		{
			TopPart = entityTopPart,
			RightPart = entityRightPart,
			BotPart = entityBotPart,
			LeftPart = entityLeftPart,
		});

		dstManager.AddComponentData(entity, new GridPositionData());

		dstManager.AddComponentData(entity, new HealthData()
		{
			HealthMax = health,
			HealthCurrent = health,
		});
	}
}
