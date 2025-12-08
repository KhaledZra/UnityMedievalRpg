using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    [SerializeField] private Node nodePrefab;

    [SerializeField] private GameObject pathPrefab;

    // [SerializeField] private Vector2Int start;
    // [SerializeField] private Vector2Int target;
    [SerializeField] private Color startColor;
    [SerializeField] private Color targetColor;
    [SerializeField] private Color pathColor;
    [SerializeField] private bool canMoveSideways = true;

    public static PathFinder Instance { get; private set; }

    public Node[,] pathNodes;

    private GameObject[,] tiles;
    private List<GameObject> oldPath = new();
    [SerializeField, ReadOnly] private Vector2Int start = new Vector2Int(0, 0);
    [SerializeField, ReadOnly] private Vector2Int target = new Vector2Int(0, 0);

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        tiles = GridGenerator.Instance.tiles;

        // tiles[start.x, start.y].GetComponent<Renderer>().material.color = startColor;
        // tiles[target.x, target.y].GetComponent<Renderer>().material.color = targetColor;

        CreatePathNodes();
        UpdatePathNeighbors();
    }

    private void CreatePathNodes()
    {
        pathNodes = new Node[tiles.GetUpperBound(0) + 1, tiles.GetUpperBound(1) + 1];

        for (int i = 0; i <= tiles.GetUpperBound(0); i++)
        {
            for (int j = 0; j <= tiles.GetUpperBound(1); j++)
            {
                pathNodes[i, j] =
                    Instantiate(nodePrefab,
                        GridGenerator.Instance.tiles[i, j].transform.position,
                        Quaternion.identity,
                        transform);
            }
        }
    }

    private void UpdatePathNeighbors()
    {
        for (int i = 0; i <= tiles.GetUpperBound(0); i++)
        {
            for (int j = 0; j <= tiles.GetUpperBound(1); j++)
            {
                // Cardinal Neighbors
                if (j + 1 <= tiles.GetUpperBound(1)) pathNodes[i, j].Neihbours.Add(pathNodes[i, j + 1]); // North
                if (j - 1 >= 0) pathNodes[i, j].Neihbours.Add(pathNodes[i, j - 1]); // South
                if (i + 1 <= tiles.GetUpperBound(0)) pathNodes[i, j].Neihbours.Add(pathNodes[i + 1, j]); // East
                if (i - 1 >= 0) pathNodes[i, j].Neihbours.Add(pathNodes[i - 1, j]); // West

                // Diagonal neighbors (corners)
                if (canMoveSideways is false) continue;

                if (i + 1 <= tiles.GetUpperBound(0) && j + 1 <= tiles.GetUpperBound(1)) // NorthEast
                    pathNodes[i, j].Neihbours.Add(pathNodes[i + 1, j + 1]);
                if (i - 1 >= 0 && j + 1 <= tiles.GetUpperBound(1)) // NorthWest
                    pathNodes[i, j].Neihbours.Add(pathNodes[i - 1, j + 1]);
                if (i + 1 <= tiles.GetUpperBound(0) && j - 1 >= 0) // SouthEast
                    pathNodes[i, j].Neihbours.Add(pathNodes[i + 1, j - 1]);
                if (i - 1 >= 0 && j - 1 >= 0) // SouthWest
                    pathNodes[i, j].Neihbours.Add(pathNodes[i - 1, j - 1]);
            }
        }
    }

    // [Button]
    // private void CreatePath()
    // {
    //     List<Node> path =
    //         AStarManager.Instance.GeneratePath(pathNodes[start.x, start.y], pathNodes[target.x, target.y]);
    //
    //     foreach (Node n in path)
    //     {
    //         Instantiate(pathPrefab, n.transform.position, Quaternion.identity)
    //             .GetComponent<Renderer>().material.color = pathColor;
    //     }
    // }

    [Button]
    private void CreateRandomPath()
    {
        // Clean up if any old path
        oldPath.ForEach(Destroy);
        oldPath.Clear();
        tiles[start.x, start.y].GetComponent<Renderer>().material.color = Color.grey;
        tiles[target.x, target.y].GetComponent<Renderer>().material.color = Color.grey;

        // Get new random targets
        start = new Vector2Int(Random.Range(0, tiles.GetUpperBound(0)),
            Random.Range(0, tiles.GetUpperBound(1)));
        target = new Vector2Int(Random.Range(0, tiles.GetUpperBound(0)),
            Random.Range(0, tiles.GetUpperBound(1)));
        tiles[start.x, start.y].GetComponent<Renderer>().material.color = startColor;
        tiles[target.x, target.y].GetComponent<Renderer>().material.color = targetColor;
        
        // Generate new path
        List<Node> newPath =
            AStarManager.Instance.GeneratePath(
                pathNodes[start.x, start.y],
                pathNodes[target.x, target.y]);

        foreach (Node n in newPath)
        {
            oldPath.Add(Instantiate(pathPrefab, n.transform.position, Quaternion.identity));
            oldPath.Last().GetComponent<Renderer>().material.color = pathColor;
        }
    }
}