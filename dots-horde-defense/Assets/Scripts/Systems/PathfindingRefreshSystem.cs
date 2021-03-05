using Unity.Entities;
using Unity.Transforms;

[UpdateAfter(typeof(PathfindingSystem))]
public class PathfindingRefreshSystem : SystemBase
{
	private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
	private bool _refreshRequested;
	

	protected override void OnCreate()
	{
		_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}
	
	protected override void OnUpdate()
	{
		if (!_refreshRequested)
			return;

		var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
		var translationGroup = GetComponentDataFromEntity<Translation>(true);
		
		Entities.ForEach((
				Entity entity,
				int entityInQueryIndex,
				ref ActivePathfindingData activePathfindingData) => 
			{
				if (activePathfindingData.CurrentPathIndex == -1)
					return;

				var newActivePathfindingData = new ActivePathfindingData()
				{
					CurrentPathIndex = -1,
				};
				
				ecb.SetComponent<ActivePathfindingData>(entityInQueryIndex, entity, newActivePathfindingData);
				
				var requestPathfindingData = new RequestPathfindingData()
				{
					StartPosition = translationGroup[entity].Value,
					TargetPosition = activePathfindingData.TargetPosition,
				};
				
				ecb.AddComponent<RequestPathfindingData>(entityInQueryIndex, entity, requestPathfindingData);
			}
			).WithReadOnly(translationGroup).ScheduleParallel();

		_refreshRequested = false;
	}

	public void RequestRefresh() => _refreshRequested = true;
}