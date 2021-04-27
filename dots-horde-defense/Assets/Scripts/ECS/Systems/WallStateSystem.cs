using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;

public class WallStateSystem : SystemBase
{
	private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
	
	
	protected override void OnCreate()
	{
		base.OnCreate();
		_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}
	
	protected override void OnUpdate()
	{
		var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
		
		var grid = GridController.Instance.Grid;
		var gridWidth = grid.GetWidth();
		var pathNodes = ConvertGridNodesToPathNodes(grid.GetNodes());

		Entities.WithAll<WallData, GridPositionData, Tag_NeedsStateUpdate>().ForEach((
				Entity entity,
				int entityInQueryIndex,
				in WallData wallData,
				in GridPositionData gridPositionData) =>
			{
				var topNodeHasBuilding = pathNodes[gridPositionData.IndexInGrid + gridWidth].HasBuilding;
				var rightNodeHasBuilding = pathNodes[gridPositionData.IndexInGrid + 1].HasBuilding;
				var botNodeHasBuilding = pathNodes[gridPositionData.IndexInGrid - gridWidth].HasBuilding;
				var leftNodeHasBuilding = pathNodes[gridPositionData.IndexInGrid - 1].HasBuilding;
				
				if (topNodeHasBuilding) { ecb.RemoveComponent<DisableRendering>(wallData.TopPart.Index, wallData.TopPart); }
				else { ecb.AddComponent<DisableRendering>(wallData.TopPart.Index, wallData.TopPart); }
				
				if (rightNodeHasBuilding) { ecb.RemoveComponent<DisableRendering>(wallData.RightPart.Index, wallData.RightPart); }
				else { ecb.AddComponent<DisableRendering>(wallData.RightPart.Index, wallData.RightPart); }
				
				if (botNodeHasBuilding) { ecb.RemoveComponent<DisableRendering>(wallData.BotPart.Index, wallData.BotPart); }
				else { ecb.AddComponent<DisableRendering>(wallData.BotPart.Index, wallData.BotPart); }
				
				if (leftNodeHasBuilding) { ecb.RemoveComponent<DisableRendering>(wallData.LeftPart.Index, wallData.LeftPart); }
				else { ecb.AddComponent<DisableRendering>(wallData.LeftPart.Index, wallData.LeftPart); }
				
				ecb.RemoveComponent<Tag_NeedsStateUpdate>(entityInQueryIndex, entity);
			}
		).WithReadOnly(pathNodes).WithDisposeOnCompletion(pathNodes).ScheduleParallel();
		
		_endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
	}
	

	private NativeArray<NativeGridNode> ConvertGridNodesToPathNodes(GridNode[] gridNodes)
	{
		var nativeGridNodes = new NativeArray<NativeGridNode>(gridNodes.Length, Allocator.TempJob);
		
		foreach (var gridNode in gridNodes)
		{
			var nativeGridNode = new NativeGridNode()
			{
				Index = gridNode.GridIndex,
				X = gridNode.X,
				Z = gridNode.Y,
				HasBuilding = gridNode.Building != Entity.Null,
			};

			nativeGridNodes[gridNode.GridIndex] = nativeGridNode;
		}

		return nativeGridNodes;
	}
	
	private struct NativeGridNode
	{
		public int Index;						// Index in Grid
		public int X;							// X pos in grid
		public int Z;							// Z pos in grid
		public bool HasBuilding;				// Is occupied by Building
	}
}