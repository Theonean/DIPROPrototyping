using UnityEngine;
using System.Collections.Generic;

public class SpatialHashGrid
{
    private float cellSize;
    private Dictionary<Vector2Int, List<Vector3>> grid = new Dictionary<Vector2Int, List<Vector3>>();

    public SpatialHashGrid(float cellSize)
    {
        this.cellSize = cellSize;
    }

    private Vector2Int GetCell(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / cellSize);
        int z = Mathf.FloorToInt(position.z / cellSize);
        return new Vector2Int(x, z);
    }

    public bool IsPositionOccupied(Vector3 position, float minDistance)
    {
        Vector2Int cell = GetCell(position);
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                Vector2Int neighbor = new Vector2Int(cell.x + dx, cell.y + dz);
                if (grid.TryGetValue(neighbor, out var positions))
                {
                    foreach (var existing in positions)
                    {
                        if (Vector3.SqrMagnitude(existing - position) < minDistance * minDistance)
                        {
                            return true; // too close
                        }
                    }
                }
            }
        }
        return false;
    }

    public void AddPosition(Vector3 position)
    {
        Vector2Int cell = GetCell(position);
        if (!grid.ContainsKey(cell))
        {
            grid[cell] = new List<Vector3>();
        }
        grid[cell].Add(position);
    }
}
