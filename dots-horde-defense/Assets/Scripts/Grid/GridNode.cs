using UnityEngine;
using UnityEngine.Rendering;

public class GridNode
{
    public int GridIndex { get; }
    public int X { get; }
    public int Z { get; }
    public Vector3 WorldPosition { get; }
    public bool IsBlocked { get; private set; }

    private readonly GridNode[] _neighbours = new GridNode[8];


    public GridNode(int gridIndex, int x, int z, Vector3 worldPosition, bool isBlocked)
    {
        GridIndex = gridIndex;
        X = x;
        Z = z;
        WorldPosition = worldPosition;
        IsBlocked = isBlocked;
    }

    public void SetIsBlocked(bool value) => IsBlocked = value;

    public void SetNeighbour(GridDirection direction, GridNode neighbour)
    {
        _neighbours[(int) direction] = neighbour;
    }
    
    public GridNode[] Neighbours => _neighbours;
    
    public GridNode Neighbour(GridDirection direction) => _neighbours[(int)direction];

    
}