using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NaughtyAttributes;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

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

    public Node this[Vector2Int v] => this[v.x, v.y];

    public Node this[int x, int y] => WithinBounds(x, y) ? pathNodes[x, y] : null;

    public bool WithinBounds(Vector2Int v)
    {
        return WithinBounds(v.x, v.y);
    }

    public bool WithinBounds(int x, int y)
    {
        return x >= 0 &&
               y >= 0 &&
               x < tiles.GetUpperBound(0) + 1 &&
               y < tiles.GetUpperBound(1) + 1;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        tiles = GridGenerator.Instance.tiles;

        StartCoroutine(UpdateNavMesh());
    }

    private void CreatePathNodes()
    {
        pathNodes = new Node[tiles.GetUpperBound(0) + 1, tiles.GetUpperBound(1) + 1];

        for (int i = 0; i <= tiles.GetUpperBound(0); i++)
        {
            for (int j = 0; j <= tiles.GetUpperBound(1); j++)
            {
                // Incase it's missing or null
                if (tiles[i, j] == null) continue;

                // Check if we are hitting a blockedPath
                Collider[] cols = Physics.OverlapBox(tiles[i, j].transform.position, Vector3.one,
                    Quaternion.identity, 1 << 11);

                if (cols.Length == 0)
                {
                    pathNodes[i, j] =
                        Instantiate(nodePrefab,
                            GridGenerator.Instance.tiles[i, j].transform.position,
                            Quaternion.identity,
                            transform);
                }
            }
        }
    }

    private void UpdatePathNeighbors()
    {
        for (int i = 0; i <= tiles.GetUpperBound(0); i++)
        {
            for (int j = 0; j <= tiles.GetUpperBound(1); j++)
            {
                if (tiles[i, j] == null) continue;
                if (this[i, j] == null) continue;

                List<Node> potentialConnections = new List<Node>
                {
                    this[i, j + 1], // North
                    this[i, j - 1], // South
                    this[i + 1, j], // East
                    this[i - 1, j], // West
                };

                // Diagonal neighbors (corners)
                if (canMoveSideways)
                {
                    potentialConnections.AddRange(new[]
                    {
                        this[i + 1, j + 1], // NorthEast
                        this[i - 1, j + 1], // NorthWest
                        this[i + 1, j - 1], // SouthEast
                        this[i - 1, j - 1], // SouthWest
                    });
                }

                pathNodes[i, j].Neihbours = potentialConnections.Where(node => node != null).ToList();
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

    private IEnumerator UpdateNavMesh()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
        
            if (pathNodes != null)
            {
                foreach (Node n in pathNodes)
                {
                    if (n == null) continue;
                    Destroy(n.gameObject);
                }
            }
            
            transform.position = Vector3.zero;
            CreatePathNodes();
            UpdatePathNeighbors();
            transform.position = Vector3.up / 2;
        }
    }

    [Button]
    private void GenerateNavMesh()
    {
        if (pathNodes != null)
        {
            foreach (Node n in pathNodes)
            {
                if (n == null) continue;
                Destroy(n.gameObject);
            }
        }


        CreatePathNodes();
        UpdatePathNeighbors();
        transform.position = Vector3.up / 2;
    }

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

        if (newPath == null) return;

        foreach (Node n in newPath)
        {
            oldPath.Add(Instantiate(pathPrefab, n.transform.position, Quaternion.identity));
            oldPath.Last().GetComponent<Renderer>().material.color = pathColor;
        }
    }
}