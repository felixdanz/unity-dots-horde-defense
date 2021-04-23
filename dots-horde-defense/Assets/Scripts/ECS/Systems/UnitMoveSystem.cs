using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class MoveSystem : SystemBase
{
	private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
	
	protected override void OnCreate()
	{
		_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}
	
	
	protected override void OnUpdate()
	{
		var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
		var deltaTime = Time.DeltaTime;
		
		// normal A* pathfinding movement
		Entities.ForEach((
			Entity entity,
			int entityInQueryIndex,
			ref Translation translation,
			ref MovementSpeed movementSpeed,
			ref DynamicBuffer<PathPointElement> pathBuffer,
			ref PathfindingData pathfindingData
			) => 
			{
				if (pathfindingData.CurrentPathIndex < 0)
					return;
				
				var distanceToTarget = math.distance(
					pathBuffer[pathfindingData.CurrentPathIndex].Position, 
					translation.Value);
				
				if (distanceToTarget == 0)
				{
					pathfindingData.CurrentPathIndex--;
					return;
				}
				
				MoveTo(
					ref translation, 
					pathBuffer[pathfindingData.CurrentPathIndex].Position,
					movementSpeed.Value, 
					deltaTime); 
			}
			).ScheduleParallel();
		
		// flowField pathfinding movement
		// Entities.ForEach((
		// 		Entity entity,
		// 		int entityInQueryIndex,
		// 		ref Translation translation,
		// 		ref MovementSpeed movementSpeed,
		// 		ref FlowFieldData flowFieldData
		// 	) => 
		// 	{
		// 		
		// 		var distanceToTarget = math.distance(
		// 			flowFieldData.TargetPosition, 
		// 			translation.Value);
		// 		
		// 		if (distanceToTarget == 0)
		// 		{
		// 			ecb.RemoveComponent<FlowFieldData>(entityInQueryIndex, entity);
		// 			return;
		// 		}
		// 		
		// 		// MoveTo(
		// 		// 	ref translation, 
		// 		// 	pathBuffer[activePathfindingData.CurrentPathIndex].Position,
		// 		// 	movementSpeed.Value, 
		// 		// 	deltaTime); 
		// 	}
		// ).ScheduleParallel();
	}

	private static void MoveTo(
		ref Translation translation, 
		float3 target, 
		float speed,
		float deltaTime)
	{
		var adjustedSpeed = speed * deltaTime;
		var targetDirection = target - translation.Value;
		var normalizedTargetDirection = math.normalize(targetDirection);
		
		var offset = normalizedTargetDirection * adjustedSpeed;
		var remainingOffset = target - translation.Value;
		
		offset = math.length(remainingOffset) < math.length(offset)
			? remainingOffset
			: offset;
		
		translation.Value += offset;
	}
}

