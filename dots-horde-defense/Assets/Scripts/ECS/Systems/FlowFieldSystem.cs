using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

/*
 * !! Work In Progress !!
 *   currently not used
 */
public class FlowFieldSystem : SystemBase
{
	private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
	private NativeList<(int2, NativeArray<FlowFieldNode>)> _flowFields;
	

	protected override void OnCreate()
	{
		// _endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		// _flowFields = new NativeList<(int2, NativeArray<FlowFieldNode>)>(Allocator.Persistent);
	}

	protected override void OnDestroy()
	{
		// for (var i = _flowFields.Length - 1; i > 0; i--)
		// {
		// 	_flowFields[i].Item2.Dispose();
		// }
		// _flowFields.Dispose();
	}

	protected override void OnUpdate()
	{
		return; // disabled
		
		var ecb = _endSimulationEcbSystem.CreateCommandBuffer();
			
		var grid = GridController.Instance.Grid;
		var gridWidth = grid.GetWidth();
		var gridHeight = grid.GetHeight();
		var gridCellSize = grid.GetCellSize();
		
		var flowFieldNodes = ConvertGridNodesToFlowField(grid.GetNodes());
		
		var flowFieldRequests = new List<(int2, CalculateFlowFieldJob, List<Entity>)>();
		var jobHandleList = new NativeList<JobHandle>(Allocator.Temp);

		Entities.ForEach((				
			Entity entity,
			int entityInQueryIndex,
			in RequestFlowFieldData requestFlowFieldData) =>
			{
				var targetNodeGridPosition = GetGridPositionFromWorld(
					gridWidth, 
					gridHeight, 
					gridCellSize, 
					requestFlowFieldData.TargetPosition);
				
				// check if flowField exists
				for (var i = 0; i < _flowFields.Length; i++)
				{
					if (_flowFields[i].Item1.x == targetNodeGridPosition.x &&
					    _flowFields[i].Item1.y == targetNodeGridPosition.y)
					{
						
					}
				}
				
				// check if request exists
				for (var i = 0; i < flowFieldRequests.Count; i++)
				{
					if (flowFieldRequests[i].Item1.x == targetNodeGridPosition.x &&
					    flowFieldRequests[i].Item1.y == targetNodeGridPosition.y)
					{
						flowFieldRequests[i].Item3.Add(entity);
						ecb.RemoveComponent<RequestFlowFieldData>(entity); 
						return;
					}
				}
				
				var targetNodeIndex = targetNodeGridPosition.x + targetNodeGridPosition.y * gridWidth;
				var flowField = new NativeArray<FlowFieldNode>(flowFieldNodes, Allocator.Persistent);
				
				var calculateFlowFieldJob = new CalculateFlowFieldJob()
				{
					FlowField = flowField,
					GridWidth = gridWidth,
					TargetNodeIndex = targetNodeIndex,
				};
				
				flowFieldRequests.Add((
					targetNodeGridPosition, 
					calculateFlowFieldJob, 
					new List<Entity>() { entity }
					));
				
				jobHandleList.Add(calculateFlowFieldJob.Schedule());
				
				ecb.RemoveComponent<RequestFlowFieldData>(entity);
			}
			).WithoutBurst().Run();
		
		JobHandle.CompleteAll(jobHandleList);

		foreach (var (targetGridPosition, flowFieldJob, requestingEntities) in flowFieldRequests)
		{
			_flowFields.Add((targetGridPosition, flowFieldJob.FlowField));
			
			foreach (var requestingEntity in requestingEntities)
			{
				var flowFieldData = new FlowFieldData()
				{
					FlowFieldKey = targetGridPosition,
					TargetPosition = flowFieldNodes[flowFieldJob.TargetNodeIndex].WorldPosition,
				};
				
				ecb.AddComponent<FlowFieldData>(requestingEntity, flowFieldData);
			}
		}

		jobHandleList.Dispose();
		flowFieldNodes.Dispose();
	}

	[BurstCompile]
	private struct CalculateFlowFieldJob : IJob
	{
		public NativeArray<FlowFieldNode> FlowField;
		public int GridWidth;
		public int TargetNodeIndex;

		public void Execute()
		{
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
			
			var vectorsToNeighbours = new NativeArray<float3>(8, Allocator.Temp)
			{
				[0] = new float3(0, 0, 1),		// up
				[1] = new float3(1, 0, 1),		// up right
				[2] = new float3(1, 0, 0),		// right
				[3] = new float3(1, 0, -1),	// down right
				[4] = new float3(0, 0, -1),	// down
				[5] = new float3(-1, 0, -1),	// down left
				[6] = new float3(-1, 0, 0),	// left
				[7] = new float3(-1, 0, 1),	// up left
			};

			var openSet = new NativeList<int>(Allocator.Temp);
			var closedSet = new NativeList<int>(Allocator.Temp);
			
			openSet.Add(TargetNodeIndex);
			
			while (openSet.Length > 0)
			{
				var currentNodeIndex = openSet[0];
				var currentNode = FlowField[currentNodeIndex];
				
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
					var neighbourIndex = currentNodeIndex + neighbourOffsets[i];
					
					if (neighbourIndex < 0 || neighbourIndex >= FlowField.Length ||
					    closedSet.Contains(neighbourIndex))
						continue;

					var neighbourNode = FlowField[neighbourIndex];
					
					if (neighbourNode.IsBlocked)
						continue;

					var neighbourDistance = currentNode.Distance + 1;

					if (neighbourDistance < neighbourNode.Distance || !openSet.Contains(neighbourIndex))
					{
						neighbourNode.Distance = neighbourDistance;
						FlowField[neighbourIndex] = neighbourNode;

						if (!openSet.Contains(neighbourIndex))
							openSet.Add(neighbourIndex);
					}
				}
			}

			for (var nodeIndex = 0; nodeIndex < FlowField.Length; nodeIndex++)
			{
				var currentNode = FlowField[nodeIndex];
				var bestDistance = int.MaxValue;

				for (var i = 0; i < neighbourOffsets.Length; i++)
				{
					var neighbourIndex = nodeIndex + neighbourOffsets[i];

					if (neighbourIndex < 0 || neighbourIndex >= FlowField.Length)
						continue;

					var neighbourNode = FlowField[neighbourIndex];

					if (neighbourNode.Distance < bestDistance)
					{
						bestDistance = neighbourNode.Distance;
						currentNode.Direction = vectorsToNeighbours[i];
					}
				}
				
				FlowField[nodeIndex] = currentNode;
			}
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

	private NativeArray<FlowFieldNode> ConvertGridNodesToFlowField(GridNode[] gridNodes)
	{
		var flowField = new NativeArray<FlowFieldNode>(gridNodes.Length, Allocator.Temp);
		
		foreach (var gridNode in gridNodes)
		{
			var flowFieldNode = new FlowFieldNode()
			{
				Index = gridNode.GridIndex,
				X = gridNode.X,
				Z = gridNode.Y,
				WorldPosition = gridNode.WorldPosition,
				Distance = int.MaxValue,
				Direction = new float3(0, 0,0 ),
				IsBlocked = gridNode.IsBlocked,
			};

			flowField[gridNode.GridIndex] = flowFieldNode;
		}

		return flowField;
	}
}

public struct FlowFieldNode
{
	public int Index;					// Index in Grid

	public int X;						// X pos in grid
	public int Z;						// Z pos in grid
	public float3 WorldPosition;		// Position in World Space

	public int Distance;				// Distance	from target		
	public float3 Direction;			// Direction to next node

	public bool IsBlocked;				// blocked = not accessible
}