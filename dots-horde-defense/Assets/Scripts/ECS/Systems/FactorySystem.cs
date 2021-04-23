using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(TimerSystem))]
public class FactorySystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;


    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    
    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
        var translationsGroup = GetComponentDataFromEntity<Translation>(true);
        
        Entities.ForEach((
            int entityInQueryIndex,
            ref FactoryData factoryData,
            ref TimerData timer,
            ref DynamicBuffer<EntitySpawnQueueElement> buildQueue) =>
        {
            if (buildQueue.Length == 0 || !timer.IsDone)
                return;
            
            timer.ElapsedTime = 0;
            timer.IsDone = false;
            
            var currentBuildTarget = buildQueue[0];
            buildQueue.RemoveAt(0);

            var instEntity = ecb.Instantiate(entityInQueryIndex, currentBuildTarget.Value);
            ecb.SetComponent(entityInQueryIndex, instEntity, new Translation
            {
                Value = translationsGroup[factoryData.SpawnPoint].Value
            });

            //TODO(FD): replace with Gathering Position
            // ecb.AddComponent(entityInQueryIndex, instEntity, new MoveTargetPosition
            // {
            //     Value = new float3(50f, 4f, 50f)
            // });
        }).WithReadOnly(translationsGroup).ScheduleParallel();
        
        _endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
    }
}