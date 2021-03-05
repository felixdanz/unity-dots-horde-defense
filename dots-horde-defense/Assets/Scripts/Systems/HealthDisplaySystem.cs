using Unity.Entities;

public class HealthDisplaySystem : SystemBase
{
	protected override void OnUpdate()
	{
		// TODO(FD)
		// Display health bar if (HealthCurrent < HealthMax)
		
		Entities.ForEach((
			Entity entity,
			int entityInQueryIndex,
			ref HealthData healthData) =>
		{
			
		}).ScheduleParallel();
	}
}
