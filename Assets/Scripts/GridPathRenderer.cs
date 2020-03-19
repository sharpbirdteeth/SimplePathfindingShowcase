using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridPathRenderer : MonoBehaviour
{
    Grid ParentGrid;

    // Sprite drawing
    [SerializeField]
    GameObject pathfindingSpritesParent;
    List<GameObject> pathfindingSprites;
    int spritePoolSize;
    [SerializeField]
    GameObject pathfindingSpritePrefab;
    [SerializeField]
    Color walkableColor = Color.blue;
    [SerializeField]
    Color unwalkableColor = Color.gray;

    // Line Rendering
    [SerializeField]
    GameObject lineRendererParent;
    List<Vector3> lineWorldPositions;
    Vector3Int appendedPosition;

    [SerializeField]
    Text costDisplayText;

    // Start is called before the first frame update
    void Start()
    {
        ParentGrid = GetComponentInParent<Grid>();
        appendedPosition = new Vector3Int();
        spritePoolSize = 12 * 10; // Set pool size to the maximum amount of walkability sprites we'll need to render.
        InstantiateSpritePool(spritePoolSize);
        lineWorldPositions = new List<Vector3>();

    }

    void InstantiateSpritePool(int poolSize)
    {
        pathfindingSprites = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject newSprite = Instantiate(pathfindingSpritePrefab, pathfindingSpritesParent.transform);
            pathfindingSprites.Add(newSprite);
            newSprite.SetActive(false);
        }
    }


    public void DrawPathLine(Vector2Int[] positions)
    {
        if (costDisplayText)
        {
            if(positions.Length < 1)
            {
                costDisplayText.text = "Unreachable!";
            }
            else
            {
                costDisplayText.text = positions.Length.ToString();
            }
        }
        lineRendererParent.GetComponent<LineRenderer>().enabled = false;
        lineWorldPositions.Clear();
        if (positions.Length > 0)
        {
            lineRendererParent.GetComponent<LineRenderer>().positionCount = positions.Length + 1;
            foreach (Vector2Int gridPositions in positions)
            {
                appendedPosition.x = gridPositions.x;
                appendedPosition.y = gridPositions.y;
                appendedPosition.z = 0;
                lineWorldPositions.Add(ParentGrid.CellToWorld(appendedPosition));
            }
            lineWorldPositions.Add(gameObject.transform.position - Vector3.Scale(gameObject.transform.position, Vector3.forward)); // remove depth from player's position
            lineRendererParent.GetComponent<LineRenderer>().SetPositions(lineWorldPositions.ToArray());
            lineRendererParent.GetComponent<LineRenderer>().enabled = true;
        }

    }


    public void DrawReachableSprites(Vector2Int[] positions)
    {
        Vector3Int spriteGridPosition = new Vector3Int
        {
            z = 0
        };

        foreach (GameObject sprite in pathfindingSprites)
        {
            sprite.SetActive(false);
        }

        int index = 0;
        foreach (Vector2Int position in positions)
        {
            if (index >= pathfindingSprites.Count)
            {
                Debug.LogError("More pathfinding sprites needed than current pool size allows!");
                return;
            }
            spriteGridPosition.x = position.x;
            spriteGridPosition.y = position.y;
            pathfindingSprites[index].transform.position = ParentGrid.CellToWorld(spriteGridPosition);
            pathfindingSprites[index].SetActive(true);
            if (GetComponent<GridPathfinder>().GetPathToCell(position).Count > 0)
            {
                pathfindingSprites[index].GetComponentInChildren<SpriteRenderer>().color = walkableColor;
            }
            else
            {
                pathfindingSprites[index].GetComponentInChildren<SpriteRenderer>().color = unwalkableColor;
            }
            index++;
        }

    }
}
