using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class MoveSystem : SystemBase
{
	protected override void OnUpdate()
	{
		var deltaTime = Time.DeltaTime;
		
		Entities.ForEach((
			Entity entity,
			int entityInQueryIndex,
			ref Translation translation,
			ref MovementSpeed movementSpeed,
			ref DynamicBuffer<PathPointElement> pathBuffer,
			ref ActivePathfindingData activePathfindingData
			) => 
			{
				if (activePathfindingData.CurrentPathIndex < 0)
					return;
				
				var distanceToTarget = math.distance(
					pathBuffer[activePathfindingData.CurrentPathIndex].Position, 
					translation.Value);
				
				if (distanceToTarget == 0)
				{
					activePathfindingData.CurrentPathIndex--;
					return;
				}
				
				MoveTo(
					ref translation, 
					pathBuffer[activePathfindingData.CurrentPathIndex].Position,
					movementSpeed.Value, 
					deltaTime); 
			}
			).ScheduleParallel();
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

