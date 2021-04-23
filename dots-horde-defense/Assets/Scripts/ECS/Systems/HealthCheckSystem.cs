using Unity.Entities;

public class HealthCheckSystem : SystemBase
{
	private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

	
	protected override void OnCreate()
	{
		_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}

	protected override void OnUpdate()
	{
		var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
		
		Entities.ForEach((
			Entity entity,
			int entityInQueryIndex,
			ref HealthData healthData) => 
			{
				if (healthData.HealthCurrent <= 0)
				{
					ecb.DestroyEntity(entityInQueryIndex, entity);
				} 
			}
			).ScheduleParallel();
		
		_endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
	}
}
