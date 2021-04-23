using Unity.Entities;

[UpdateInGroup(typeof(AfterSimulationGroup))]
public class InitializationSystem : SystemBase
{
	private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;


	protected override void OnCreate()
	{
		_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}

	protected override void OnUpdate()
	{
		var ecb = _endSimulationEcbSystem.CreateCommandBuffer();
		
		// TODO(FD): rework Tag to better express InitializationType
		Entities.WithAll<Tag_NeedsInitialization>().ForEach((
			Entity entity, 
			int entityInQueryIndex,
			GridController gridController) =>
			{
				gridController.InitializeGrid();
				ecb.RemoveComponent<GridController>(entity);
			}).WithoutBurst().Run();
	}
}