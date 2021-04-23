using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(PathfindingSystem))]
public class PathfindingRefreshSystem : SystemBase
{
	private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
	private bool _refreshRequested;
	private List<float3> _affectedPositions;
	

	protected override void OnCreate()
	{
		_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		_affectedPositions = new List<float3>();
	}
	
	protected override void OnUpdate()
	{
		if (!_refreshRequested)
			return;

		var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
		var translationGroup = GetComponentDataFromEntity<Translation>(true);
		var affectedPositions = new NativeArray<float3>(_affectedPositions.ToArray(), Allocator.TempJob);
		
		Entities.ForEach((
				Entity entity,
				int entityInQueryIndex,
				ref PathfindingData activePathfindingData,
				in DynamicBuffer<PathPointElement> pathBuffer) => 
			{
				if (activePathfindingData.CurrentPathIndex == -1)
					return;

				var needsRefresh = false;
				
				for (int i = activePathfindingData.CurrentPathIndex; i > 0; i--)
				{
					if (!affectedPositions.Contains(pathBuffer[i].Position)) 
						continue;
					
					needsRefresh = true;
					break;
				}
				
				if (!needsRefresh)
					return;
				

				var newActivePathfindingData = new PathfindingData()
				{
					CurrentPathIndex = -1,
				};
				
				ecb.SetComponent<PathfindingData>(entityInQueryIndex, entity, newActivePathfindingData);
				
				var requestPathfindingData = new RequestPathfindingData()
				{
					StartPosition = translationGroup[entity].Value,
					TargetPosition = activePathfindingData.TargetPosition,
				};
				
				ecb.AddComponent<RequestPathfindingData>(entityInQueryIndex, entity, requestPathfindingData);
			}
			).WithReadOnly(translationGroup).WithReadOnly(affectedPositions).ScheduleParallel();
		
		CompleteDependency();

		affectedPositions.Dispose();
		_affectedPositions.Clear();
		_refreshRequested = false;
	}

	public void RequestRefresh(float3 affectedPosition)
	{
		_refreshRequested = true;
		_affectedPositions.Add(affectedPosition);
	}
	
	public void RequestRefresh(List<float3> affectedPositions)
	{
		_refreshRequested = true;
		_affectedPositions.AddRange(affectedPositions);
	}
}