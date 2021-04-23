using System.Collections.Generic;
using Unity.Entities;
using Unity.Physics.Systems;
using UnityEngine;

public class Grid
{
	private readonly int _width;
	private readonly int _height;
	private readonly float _cellSize;
	private readonly Vector3 _origin;
	private readonly GridNode[] _nodes;
	
	
	public Grid(int width, int height, float cellSize, Vector3 origin)
	{
		_width = width;
		_height = height;
		_cellSize = cellSize;
		_origin = origin;
		_nodes = new GridNode[_width * _height];

		var buildPhysicsWorldSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>();

		for (int y = 0, i = 0; y < _height; y++)
		for (int x= 0; x < _width; x++, i++)
		{
			var worldPosition = new Vector3(x, 0, y) * _cellSize + _origin;
			var hasHitObstacle = PhysicsUtilities.SphereCast(
				buildPhysicsWorldSystem,
				PhysicsUtilities.GetCollisionFilter(PhysicCategories.Obstacle),
				worldPosition + Vector3.up * 5,
				worldPosition - Vector3.up * 5,
				0.5f,
				out _);
			
			var node = _nodes[i] = new GridNode(i, x, y, worldPosition, hasHitObstacle);
			UpdateNodeNeighbours(node, i);
		}
	}

	private void UpdateNodeNeighbours(GridNode node, int nodeIndex)
	{
		if (nodeIndex % _width > 0)
		{
			node.SetNeighbour(GridDirection.Left, _nodes[nodeIndex - 1]);
			_nodes[nodeIndex - 1].SetNeighbour(GridDirection.Right, node);

			if (nodeIndex >= _width)
			{
				node.SetNeighbour(GridDirection.DownLeft, _nodes[nodeIndex - _width - 1]);
				_nodes[nodeIndex - _width - 1].SetNeighbour(GridDirection.UpRight, node);

				_nodes[nodeIndex - 1].SetNeighbour(GridDirection.DownRight, _nodes[nodeIndex - _width]);
				_nodes[nodeIndex - _width].SetNeighbour(GridDirection.UpLeft, _nodes[nodeIndex - 1]);
			}
		}

		if (nodeIndex >= _width)
		{
			node.SetNeighbour(GridDirection.Down, _nodes[nodeIndex - _width]);
			_nodes[nodeIndex - _width].SetNeighbour(GridDirection.Up, node);
		}
	}
	
	// TODO(FD): closest node sometimes returns false node (offset by 1)
	public GridNode GetClosestNode(Vector3 worldPosition)
	{
		var percentX = worldPosition.x / (_width * _cellSize);
		var percentY = worldPosition.z / (_height * _cellSize);
		
		percentX = Mathf.Clamp(percentX, 0.0f, 1.0f);
		percentY = Mathf.Clamp(percentY, 0.0f, 1.0f);
		
		var intX = Mathf.RoundToInt(percentX * (_width - 1));
		var intY = Mathf.RoundToInt(percentY * (_height - 1));
		
		return _nodes[intX + intY * _width];
	}
	
	public List<GridNode> GetNodesOnLineBetween(GridNode nodeA, Vector3 positionB)
	{
		var nodeB = GetClosestNode(positionB);
		return GetNodesOnLineBetween(nodeA, nodeB);
	}
	
	public List<GridNode> GetNodesOnLineBetween(GridNode nodeA, GridNode nodeB)
	{
		if (nodeA == nodeB)
			return new List<GridNode>() { nodeA };
		
		var result = new List<GridNode>();

		var xDiff = Mathf.Abs(nodeB.X - nodeA.X);
		var yDiff = Mathf.Abs(nodeB.Y - nodeA.Y);
		
		var startNode = xDiff >= yDiff
			? nodeA.X <= nodeB.X 
				? nodeA 
				: nodeB
			: nodeA.Y <= nodeB.Y
				? nodeA
				: nodeB;
		
		if (xDiff >= yDiff)
		{
			for (var i = 0; i < xDiff; i++)
			{
				var newX = startNode.X + i;
				result.Add(_nodes[newX + startNode.Y * _width]);
			}
		}
		else
		{
			for (var i = 0; i < yDiff; i++)
			{
				var newY = startNode.Y + i;
				result.Add(_nodes[startNode.X + newY * _width]);
			}
		}

		return result;
	}
	
	public int GetWidth() => _width;
	
	public int GetHeight() => _height;
	
	public float GetCellSize() => _cellSize;
	
	public Vector3 GetOrigin() => _origin;
	
	public GridNode[] GetNodes() => _nodes;
}
