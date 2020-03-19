using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridUtilities
{
    private static Vector3Int cellPosition3d;
    public static bool IsGridCellOccupied(Grid grid, Vector2Int position)
    {
        cellPosition3d.x = position.x;
        cellPosition3d.y = position.y;
        cellPosition3d.z = Mathf.RoundToInt(grid.transform.position.z);
        
        Vector3 worldCellPosition = grid.CellToWorld(cellPosition3d);
        if (Physics2D.OverlapCircle(new Vector3(worldCellPosition.x, worldCellPosition.y), 0.1f))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
