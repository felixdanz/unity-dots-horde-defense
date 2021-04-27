using Unity.Entities;
using UnityEngine;

public class GridNode
{
    public int GridIndex { get; }
    public int X { get; }
    public int Y { get; }
    public Vector3 WorldPosition { get; }
    
    private readonly GridNode[] _neighbours = new GridNode[8];
    
    public bool IsBlocked { get; private set; }

    // TODO(FD): experimental
    public Entity Building;


    public GridNode(int gridIndex, int x, int y, Vector3 worldPosition, bool isBlocked)
    {
        GridIndex = gridIndex;
        X = x;
        Y = y;
        WorldPosition = worldPosition;
        IsBlocked = isBlocked;
        Building = Entity.Null;
    }

    public void SetIsBlocked(bool value) => IsBlocked = value;

    public void SetNeighbour(GridDirection direction, GridNode neighbour)
    {
        _neighbours[(int) direction] = neighbour;
    }
    
    public GridNode[] Neighbours => _neighbours;
    
    public GridNode Neighbour(GridDirection direction) => _neighbours[(int)direction];

    
}