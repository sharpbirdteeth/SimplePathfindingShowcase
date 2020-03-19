using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(GridPosition2d))]
[RequireComponent(typeof(GridPathRenderer))]
public class GridPathfinder : MonoBehaviour
{
    Grid ParentGrid;

    [SerializeField]
    int movementPointsPerTurn = 6;
    Vector2Int previousMousePosition;
    public Vector2Int debugTargetPosition;
    private List<Vector2Int> cellPositionsInRange;

    bool hasClicked;
    Vector2Int mouse2dPos;

    // Must happen after GridPosition2d.Start() and GridPathRenderer.Start(). The script execution order in the project settings ensures this.
    void Start()
    {
        this.ParentGrid = gameObject.GetComponentInParent<Grid>();
        cellPositionsInRange = new List<Vector2Int>();
        cellPositionsInRange = GetCellsWithinRange();
        gameObject.GetComponent<GridPathRenderer>().DrawReachableSprites(cellPositionsInRange.ToArray());
    }

    void FixedUpdate()
    {
        Vector3 mouse3dSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse2dPos.x = ParentGrid.WorldToCell(mouse3dSpace).x;
        mouse2dPos.y = ParentGrid.WorldToCell(mouse3dSpace).y;
        // Don't check for a path unless the mouse is over a different cell.
        if (previousMousePosition != mouse2dPos)
        {
            Queue<Vector2Int> path = GetPathToCell(mouse2dPos);
            gameObject.GetComponent<GridPathRenderer>().DrawPathLine(path.ToArray());
            previousMousePosition = mouse2dPos;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
           if(GetPathToCell(mouse2dPos).Count > 0)
            {
                GetComponent<GridPosition2d>().GridPosition = mouse2dPos;
                GetComponent<GridPathRenderer>().DrawReachableSprites(GetCellsWithinRange().ToArray());
            }
        }
    }

    List<Vector2Int> GetCellsWithinRange()
    {
        cellPositionsInRange.Clear();
        Vector2Int playerPosition = gameObject.GetComponent<GridPosition2d>().GridPosition;
        Vector2Int offset = new Vector2Int();

        int Range = movementPointsPerTurn;
        for (int x = Range; x >= -Range; x--)
        {
            for (int y = Range; y >= -Range; y--)
            {
                if ((Mathf.Abs(x) + Mathf.Abs(y) > Range) == false)// Check if movement is out of player's range without diagonal movement.
                { 
                    offset.x = x;
                    offset.y = y;
                    if (GridUtilities.IsGridCellOccupied(ParentGrid, playerPosition + offset) == false)
                    {
                        cellPositionsInRange.Add(playerPosition + offset);
                    }
                }
            }
        }

        return cellPositionsInRange;
    }

    // Returns empty queue if there is no valid path. e.g. out of range or blocked.
    // Uses breadth-first search as we have a small amount of cells and need to guarantee an optimal path.
    // TODO: Consider caching a path to each cell that is queried, and returning it if it is present and valid.
    // TODO: Consider moving the allocations out to the class level. This would make the class messier but decrease garbage accumulation.
    public Queue<Vector2Int> GetPathToCell(Vector2Int targetCellPosition)
    {
        Queue<Vector2Int> pathQueue = new Queue<Vector2Int>();
        Vector2Int playerPosition = GetComponent<GridPosition2d>().GridPosition;

        // Position is out of range or blocked, early return.
        if (cellPositionsInRange.Contains(targetCellPosition) == false) { return pathQueue; } 

        Queue<SearchCell> unsearchedCellQueue = new Queue<SearchCell>();
        List<Vector2Int> searchedCells = new List<Vector2Int>();
        Vector2Int searchDirection = new Vector2Int();
        bool hasFoundPath = false;
        SearchCell foundTargetCell = null;

        // Start search at our current position.
        unsearchedCellQueue.Enqueue(new SearchCell(playerPosition, null));
        while (unsearchedCellQueue.Count > 0 && hasFoundPath == false)
        {
            SearchCell searchingCell = unsearchedCellQueue.Dequeue();
            // Search in x direction
            for (int x = -1; x <= 1; x++)
            {
                // Search in y direction
                for (int y = -1; y <= 1; y++)
                {
                    // This check disables searching diagonally.
                    if ((Mathf.Abs(x) + Mathf.Abs(y) > 1) == false) 
                    {
                        searchDirection.x = x;
                        searchDirection.y = y;
                        SearchCell nextCell = new SearchCell(playerPosition + searchDirection, searchingCell)
                        {
                            Position = searchingCell.Position + searchDirection,
                            SearchedFrom = searchingCell
                        };

                        // Don't re-search searched cells.
                        if (searchedCells.Contains(searchingCell.Position + searchDirection) == false) 
                        {
                            searchedCells.Add(nextCell.Position);
                            if (GridUtilities.IsGridCellOccupied(ParentGrid, nextCell.Position) == false)
                            {
                                unsearchedCellQueue.Enqueue(nextCell);
                                if (nextCell.Position == targetCellPosition)
                                {
                                    foundTargetCell = nextCell;
                                    hasFoundPath = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        if (hasFoundPath)
        {
            while(foundTargetCell.Position != playerPosition)
            {                
                pathQueue.Enqueue(foundTargetCell.Position);
                foundTargetCell = foundTargetCell.SearchedFrom;
            }
        }
        // Final range check.
        if (pathQueue.Count > movementPointsPerTurn) 
        {
            pathQueue.Clear();
        }

        return pathQueue;
    }
}

class SearchCell
{
    public SearchCell(Vector2Int position, SearchCell searchedFrom)
    {
        Position = position;
        SearchedFrom = searchedFrom;
    }
    public Vector2Int Position { get; set; }
    public SearchCell SearchedFrom { get; set; }
}

#if UNITY_EDITOR

[CustomEditor(typeof(GridPathfinder))]
public class GridPathfinderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GridPathfinder gridPathfinder = (GridPathfinder)target;
        if (GUILayout.Button("Debug print route result."))
        {
            Queue<Vector2Int> path = gridPathfinder.GetPathToCell(gridPathfinder.debugTargetPosition);
            string pathString = "";
            foreach (Vector2Int vector2Int in path.ToArray())
            {
                pathString += vector2Int.ToString() + ", ";
            }
            Debug.Log(pathString);
        }
    }
}

#endif