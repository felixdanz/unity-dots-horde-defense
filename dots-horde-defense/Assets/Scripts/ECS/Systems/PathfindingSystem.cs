using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public class PathfindingSystem : SystemBase
{
	private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;


	protected override void OnCreate()
	{
		_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}

	protected override void OnUpdate()
	{
		var ecb = _endSimulationEcbSystem.CreateCommandBuffer();
		
		var grid = GridController.Instance.Grid;
		var gridWidth = grid.GetWidth();
		var gridHeight = grid.GetHeight();
		var gridCellSize = grid.GetCellSize();
		
		var pathNodes = ConvertGridNodesToPathNodes(grid.GetNodes());
		
		var findPathJobList = new List<FindPathJob>();
		var jobHandleList = new NativeList<JobHandle>(Allocator.Temp);
		
		Entities.ForEach((
				Entity entity,
				int entityInQueryIndex,
				in RequestPathfindingData requestPathfindingData) => 
			{
				var tmpPathNodes = new NativeArray<PathNode>(pathNodes, Allocator.TempJob);
				var tmpTargetIndex = new NativeArray<int>(1, Allocator.TempJob);

				var startGridPosition = GetGridPositionFromWorld(
					gridWidth, gridHeight, gridCellSize, 
					requestPathfindingData.StartPosition);
				var targetGridPosition = GetGridPositionFromWorld(
					gridWidth, gridHeight, gridCellSize, 
					requestPathfindingData.TargetPosition);
				
				var findPathJob = new FindPathJob()
				{
					RequestingEntity = entity,
					PathNodes = tmpPathNodes,
					GridWidth = gridWidth,
					StartGridPosition = startGridPosition,
					TargetGridPosition = targetGridPosition,
					TargetNodeIndex = tmpTargetIndex,
				};

				findPathJobList.Add(findPathJob);
				jobHandleList.Add(findPathJob.Schedule());
				
				ecb.RemoveComponent<RequestPathfindingData>(entity);
			}
			).WithoutBurst().Run();
		
		JobHandle.CompleteAll(jobHandleList);
		
		foreach (var findPathJob in findPathJobList)
		{
			new SetBufferPathJob()
			{
				RequestingEntity = findPathJob.RequestingEntity,
				PathNodes = findPathJob.PathNodes,
				TargetNodeIndex = findPathJob.TargetNodeIndex,
				PathPointBufferFromEntity = GetBufferFromEntity<PathPointElement>(),
				PathCurrentIndexDataFromEntity = GetComponentDataFromEntity<PathfindingData>(),
			}.Run();
		}
		
		jobHandleList.Dispose();
		pathNodes.Dispose();
	}

	[BurstCompile]
	private struct SetBufferPathJob : IJob
	{
		public Entity RequestingEntity;

		[DeallocateOnJobCompletion] public NativeArray<PathNode> PathNodes;
		[DeallocateOnJobCompletion] public NativeArray<int> TargetNodeIndex;

		public BufferFromEntity<PathPointElement> PathPointBufferFromEntity;
		public ComponentDataFromEntity<PathfindingData> PathCurrentIndexDataFromEntity;
		
		public void Execute()
		{
			var pathPointBuffer = PathPointBufferFromEntity[RequestingEntity];
			pathPointBuffer.Clear();

			var targetNode = PathNodes[TargetNodeIndex[0]];

			if (targetNode.ParentIndex == -1)
			{
				PathCurrentIndexDataFromEntity[RequestingEntity] = new PathfindingData()
				{
					CurrentPathIndex = -1,
				};
				return;
			}
			
			pathPointBuffer.Add(new PathPointElement() { Position = targetNode.WorldPosition });
			
			var currentNode = targetNode;
			while (currentNode.ParentIndex != -1)
			{
				var parentNode = PathNodes[currentNode.ParentIndex];
				pathPointBuffer.Add(new PathPointElement() { Position = parentNode.WorldPosition });
				currentNode = parentNode;
			}

			PathCurrentIndexDataFromEntity[RequestingEntity] = new PathfindingData()
			{
				CurrentPathIndex = pathPointBuffer.Length - 1,
				StartPosition = pathPointBuffer[pathPointBuffer.Length - 1].Position,
				TargetPosition = pathPointBuffer[0].Position,
			};
		}
	}

	[BurstCompile]
	private struct FindPathJob : IJob
	{
		public Entity RequestingEntity;
		public NativeArray<PathNode> PathNodes;
		public int GridWidth;
		public int2 StartGridPosition;
		public int2 TargetGridPosition;
		public NativeArray<int> TargetNodeIndex;

		public void Execute()
		{
			var startNodeIndex = StartGridPosition.x + StartGridPosition.y * GridWidth;
			TargetNodeIndex[0] = TargetGridPosition.x + TargetGridPosition.y * GridWidth;
			
			var neighbourOffsets = new NativeArray<int>(8, Allocator.Temp)
			{
				[0] = GridWidth,			// up
				[1] = GridWidth + 1,		// up right
				[2] = 1,					// right
				[3] = (-GridWidth) + 1,		// down right
				[4] = (-GridWidth),			// down
				[5] = -(GridWidth + 1),		// down left
				[6] = -1,					// left
				[7] = GridWidth - 1,		// up left
			};

			var openSet = new NativeList<int>(Allocator.Temp);
			var closedSet = new NativeList<int>(Allocator.Temp);
			
			openSet.Add(startNodeIndex);
			
			while (openSet.Length > 0)
			{
				var currentNode = PathNodes[openSet[0]];

				for (var i = 1; i < openSet.Length; i++)
				{
					var tempNode = PathNodes[openSet[i]];
					
					if (tempNode.FCost < currentNode.FCost ||
					    tempNode.FCost == currentNode.FCost && tempNode.HCost < currentNode.HCost)
					{
						currentNode = tempNode;
					}
				}

				if (currentNode.Index == TargetNodeIndex[0])
					break;

				for (var i = 0; i < openSet.Length; i++)
				{
					if (openSet[i] != currentNode.Index) 
						continue;
					
					openSet.RemoveAtSwapBack(i);
					break;
				}
				
				closedSet.Add(currentNode.Index);

				for (var i = 0; i < neighbourOffsets.Length; i++)
				{
					var neighbourIndex = currentNode.Index + neighbourOffsets[i];

					if (neighbourIndex < 0 || neighbourIndex >= PathNodes.Length)
						continue;
					
					if (closedSet.Contains(neighbourIndex))
						continue;

					var neighbourNode = PathNodes[neighbourIndex];
					
					if (neighbourNode.IsBlocked)
						continue;
					
					var movementCostToNeighbour =
						currentNode.GCost + GetDistanceBetweenPathNodes(currentNode, neighbourNode);

					if (movementCostToNeighbour < neighbourNode.GCost || !openSet.Contains(neighbourIndex))
					{
						neighbourNode.GCost = movementCostToNeighbour;
						neighbourNode.HCost = GetDistanceBetweenPathNodes(neighbourNode, PathNodes[TargetNodeIndex[0]]);
						neighbourNode.ParentIndex = currentNode.Index;
						PathNodes[neighbourIndex] = neighbourNode;
						
						if (!openSet.Contains(neighbourIndex))
							openSet.Add(neighbourIndex);
					}
				}
			}
			
			neighbourOffsets.Dispose();
			openSet.Dispose();
			closedSet.Dispose();
		}
	}

	private static int2 GetGridPositionFromWorld(int gridWidth, int gridHeight, float cellSize, float3 worldPosition)
	{
		var percentX = worldPosition.x / (gridWidth * cellSize);
		var percentZ = worldPosition.z / (gridHeight * cellSize);

		percentX = math.clamp(percentX, 0.0f, 1.0f);
		percentZ = math.clamp(percentZ, 0.0f, 1.0f);

		var intX = (int) Math.Round(percentX * (gridWidth - 1), 0);
		var intZ = (int) Math.Round(percentZ * (gridHeight - 1), 0);

		return new int2(intX, intZ);
	}
	
	private static int GetDistanceBetweenPathNodes(PathNode nodeA, PathNode nodeB)
	{
		var distanceX = math.abs(nodeA.X - nodeB.X);
		var distanceZ = math.abs(nodeA.Z - nodeB.Z);

		return (distanceX > distanceZ)
			? 14 * distanceZ + 10 * (distanceX - distanceZ) 
			: 14 * distanceX + 10 * (distanceZ - distanceX);
	}

	private NativeArray<PathNode> ConvertGridNodesToPathNodes(GridNode[] gridNodes)
	{
		var pathNodes = new NativeArray<PathNode>(gridNodes.Length, Allocator.Temp);
		
		foreach (var gridNode in gridNodes)
		{
			var pathNode = new PathNode()
			{
				Index = gridNode.GridIndex,
				ParentIndex = -1,
				X = gridNode.X,
				Z = gridNode.Y,
				WorldPosition = gridNode.WorldPosition,
				IsBlocked = gridNode.IsBlocked,
			};

			pathNodes[gridNode.GridIndex] = pathNode;
		}

		return pathNodes;
	}
	
	private struct PathNode
	{
		public int Index;						// Index in Grid
		public int ParentIndex;					// Index of PathParent in Grid
	
		public int X;							// X pos in grid
		public int Z;							// Z pos in grid
		public float3 WorldPosition;			// Position in World Space	

		public int GCost;						// distance to starting node
		public int HCost;						// distance to target node
		public int FCost => GCost + HCost;		// combined cost

		public bool IsBlocked;					// blocked = not accessible
	}
}