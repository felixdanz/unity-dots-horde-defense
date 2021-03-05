using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public class UnitControlSystem : SystemBase
{
	private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
	
	
	protected override void OnCreate()
	{
		_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}
	
	protected override void OnUpdate()
	{
		if (Input.GetMouseButtonDown(1))
		{
			var raycastSuccess = CameraController.Instance.Raycast(
				Input.mousePosition,
				PhysicsUtilities.GetCollisionFilter(PhysicCategories.Ground), 
				out var raycastHit);
			
			if (!raycastSuccess)
				return;

			var ecb = _endSimulationEcbSystem.CreateCommandBuffer();
			var translationGroup = GetComponentDataFromEntity<Translation>(true);

			Entities.WithAll<Tag_UnitSelected>().ForEach((
				Entity entity,
				int entityInQueryIndex) =>
			{
				var requestPathfindingData = new RequestPathfindingData()
				{
					StartPosition = translationGroup[entity].Value,
					TargetPosition = raycastHit.Position,
				};
				ecb.AddComponent<RequestPathfindingData>(entity, requestPathfindingData);
			}).WithReadOnly(translationGroup).Run();
			
			_endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
		}
	}
}