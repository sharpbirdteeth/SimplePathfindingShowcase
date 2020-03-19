using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridPosition2d : MonoBehaviour
{
    Grid ParentGrid;
    [SerializeField]
    Vector2Int startingGridPosition;
    private Vector2Int _gridPosition;
    public Vector2Int GridPosition
    {
        get { return this._gridPosition; }
        set
        {
            if (MoveToGridPosition(value))
            {
                _gridPosition = value;
            }
        }
    }

    void Start()
    {
        ParentGrid = gameObject.GetComponentInParent<Grid>();
        Debug.Assert(ParentGrid != null, "Grid component not found in parent object.");
        GridPosition = startingGridPosition;
    }

    // Not intended for use on each frame because of 'new' allocations.
    bool MoveToGridPosition(Vector2Int gridPosition)
    {
        Debug.Assert(ParentGrid);
        // As this is a 2D project, the cell Z position is irrelevant. The IsGridCellOccupied check is in 2D.
        Vector3Int cellPosition3d = new Vector3Int(gridPosition.x, gridPosition.y, -3);
        Vector3 worldCellPosition = ParentGrid.CellToWorld(cellPosition3d);
        if(GridUtilities.IsGridCellOccupied(ParentGrid, gridPosition))
        {
            Debug.LogError("Attempted to move \"" + gameObject.name + "\" to occupied grid position: " + gridPosition.ToString());
            return false;
        }
        else
        {
            gameObject.transform.position = worldCellPosition - Vector3.forward;
            return true;
        }
    }
}
