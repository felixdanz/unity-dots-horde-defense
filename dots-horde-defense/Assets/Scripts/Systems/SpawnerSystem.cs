using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class SpawnerSystem : SystemBase
{
	private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;


	protected override void OnCreate()
	{
		_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}
	
	protected override void OnUpdate()
	{
		var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
		var localToWorldGroup = GetComponentDataFromEntity<LocalToWorld>(true);
		var translationGroup = GetComponentDataFromEntity<Translation>(true);
		var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;		

		Entities
			.WithNativeDisableParallelForRestriction(randomArray)
			.ForEach((
				int nativeThreadIndex,
				int entityInQueryIndex, 
				ref SpawnerData spawnerData, 
				ref TimerData timer) => 
			{
				if (!timer.IsDone)
					return;
	            
				timer.ElapsedTime = 0;
				timer.IsDone = false;
				
				var instEntity = ecb.Instantiate(entityInQueryIndex, spawnerData.EntityToSpawn);

				var random = randomArray[nativeThreadIndex];
				var spawnPosition = new float3(random.NextFloat(10.0f, 90.0f), 2, 95);
				
				var newTranslation = new Translation()
				{
					//Value = localToWorldGroup[spawnerData.SpawnPoint].Position,
					Value = spawnPosition,
				};
				ecb.SetComponent<Translation>(entityInQueryIndex, instEntity, newTranslation);
				
				var requestPathfindingData = new RequestPathfindingData()
				{
					StartPosition = spawnPosition,
					TargetPosition = new float3(random.NextFloat(10.0f, 90.0f), 2, 10),
					//TargetPosition = translationGroup[spawnerData.TargetPoint].Value,
				};
				ecb.AddComponent<RequestPathfindingData>(entityInQueryIndex, instEntity, requestPathfindingData);
				randomArray[nativeThreadIndex] = random;
			}).ScheduleParallel();

		_endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
	}
}
