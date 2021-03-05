using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

public class FlowFieldSystem : SystemBase
{
	// private GridNode[] _nodes;
	// private int _width, _height;
	//
	// private Dictionary<float3, PathfindingData> _createdFlowFields;
	//
	// private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
	//
	//
	// protected override void OnCreate()
	// {
	// 	_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	//
	// 	// TODO(FD): meh -.-
	// 	_width = 100;
	// 	_height = 100;
	// 	_nodes = new GridNode[_width * _height];
	// 	
	// 	for (int z = 0, i = 0; z < _width; z++)
	// 	for (int x = 0; x < _height; x++, i++)
	// 	{
	// 		// TODO(FD): implement later (isNodeBlocked)
	// 		// if (SphereCast(
	// 		// 	new float3(x, 1, z),
	// 		// 	new float3(x, -1, z),
	// 		// 	0.5f,
	// 		// 	out var hitEntity))
	// 		// {
	// 		// 	
	// 		// }
	//
	// 		var node = _nodes[i] = new GridNode(i, new float3(x, 0, z));
	// 		UpdateNodeNeighbours(node, i);
	// 	}
	// }
	//
	// protected override void OnUpdate()
	// {
	// 	var ecb = _endSimulationEcbSystem.CreateCommandBuffer();
	// 	
	// 	Entities.WithAll<RequestPathfindingData>().ForEach((
	// 		Entity entity,
	// 		int entityInQueryIndex,
	// 		in RequestPathfindingData requestPathfindingData) =>
	// 	{
	// 		var closestNodeIsFound = TryGetClosestNode(requestPathfindingData.TargetPosition, out var closestNode);
	// 		
	// 		ecb.RemoveComponent<RequestPathfindingData>(entity);
	// 		
	// 		if (!closestNodeIsFound)
	// 			return;
	//
	// 		if (_createdFlowFields.TryGetValue(closestNode.GridPosition, out var foundPathfindingData))
	// 		{
	// 			ecb.AddSharedComponent(entity, foundPathfindingData);
	// 		}
	// 		else
	// 		{
	// 			var flowField = CreateFlowField(closestNode);
	// 			var newPathfindingData = new PathfindingData()
	// 			{
	// 				FlowField = flowField,
	// 			};
	// 			_createdFlowFields.Add(closestNode.GridPosition, newPathfindingData);
	// 			ecb.AddSharedComponent(entity, foundPathfindingData);
	// 		}
	// 	}).WithoutBurst().Run();
	// }
	//
	// private void UpdateNodeNeighbours(GridNode node, int nodeIndex)
	// {
	// 	if (nodeIndex % _width > 0)
	// 	{
	// 		node.SetNeighbour(GridDirection.Left, _nodes[nodeIndex - 1]);
	// 		_nodes[nodeIndex - 1].SetNeighbour(GridDirection.Right, node);
	// 		
	// 		if (nodeIndex >= _width)
	// 		{
	// 			node.SetNeighbour(GridDirection.DownLeft, _nodes[nodeIndex - _width - 1]);
	// 			_nodes[nodeIndex - _width - 1].SetNeighbour(GridDirection.UpRight, node);
	//         
	// 			_nodes[nodeIndex - 1].SetNeighbour(GridDirection.DownRight, _nodes[nodeIndex - _width]);
	// 			_nodes[nodeIndex - _width].SetNeighbour(GridDirection.UpLeft, _nodes[nodeIndex - 1]);
	// 		}
	// 	}
	//     
	// 	if (nodeIndex >= _width)
	// 	{
	// 		node.SetNeighbour(GridDirection.Down, _nodes[nodeIndex - _width]);
	// 		_nodes[nodeIndex - _width].SetNeighbour(GridDirection.Up, node);
	// 	}
	// }
	//
	// private bool TryGetClosestNode(float3 inputPosition, out GridNode closestNode)
	// {
	// 	closestNode = null;
	// 	
	// 	var rX = UnityEngine.Mathf.RoundToInt(inputPosition.x);
	//     var rZ = UnityEngine.Mathf.RoundToInt(inputPosition.z);
	//     var index = rX + rZ * _width;
	// 	
	//     closestNode = _nodes[index];
	//     return true;
	// }
	//
	// private float3[] CreateFlowField(GridNode targetNode)
	// {
	// 	var flowField = new float3[_width * _height];
	// 	
	// 	var openSet = new List<GridNode>();
	// 	var closedSet = new List<GridNode>();
	// 	
	// 	openSet.Add(targetNode);
	//
	// 	while (openSet.Count > 0)
	// 	{
	// 		var currentNode = openSet[0];
	// 		
	// 		for (var i = 0; i < openSet.Count)
	// 	}
	// }
	//
	// public List<Cell> FindPath(Cell startCell, Cell targetCell)
	// {
	// 	var openSet = new List<Cell>();
	// 	var closedSet = new List<Cell>();
	//
	// 	openSet.Add(startCell);
	//
	// 	while (openSet.Count > 0)
	// 	{
	// 		var currentTile = openSet[0];
 //        
	// 		for (var i = 1; i < openSet.Count; i++)
	// 		{
	// 			if (openSet[i].FCost < currentTile.FCost || 
	// 			    openSet[i].FCost == currentTile.FCost && openSet[i].HCost < currentTile.HCost)
	// 			{
	// 				currentTile = openSet[i];
	// 			}
	// 		}
	//
	// 		openSet.Remove(currentTile);
	// 		closedSet.Add(currentTile);
	//
	// 		// path found
	// 		if (currentTile == targetCell)
	// 		{
	// 			return RetracePath(startCell, targetCell);
	// 		}
 //        
	// 		// continue searching
	// 		foreach (var neighbour in currentTile.GetNeighbours())
	// 		{
	// 			if (neighbour is null || closedSet.Contains(neighbour))
	// 				continue;
 //            
	// 			var distanceToNeighbour = CellCoordinate.DistanceBetween(currentTile.GetCoordinate(), neighbour.GetCoordinate());
	// 			var newMovementCostToNeighbour = currentTile.GCost + distanceToNeighbour + neighbour.GetTerrain().GetMovementCost();
	//
	// 			// if neighbours has good movement cost add to open set
	// 			if (newMovementCostToNeighbour < neighbour.GCost || !openSet.Contains(neighbour))
	// 			{
	// 				neighbour.GCost = newMovementCostToNeighbour;
	// 				neighbour.HCost = CellCoordinate.DistanceBetween(neighbour.GetCoordinate(), targetCell.GetCoordinate());
	// 				neighbour.Parent = currentTile;
	//
	// 				if (!openSet.Contains(neighbour))
	// 					openSet.Add(neighbour);
	// 			}
	// 		}
	// 	}
	//
	// 	Debug.Log("Pathfinder: no path found");
	// 	return null;
	// }
	
	protected override void OnUpdate()
	{
		
	}
}