using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using PathFinding.MethodExtensions;
using PathFinding.Nodes;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SimpleNavVolume : MonoBehaviour
{
    [Header("Area")] [SerializeField] private BoxCollider volume;
    [SerializeField] private bool canMoveDiagonal = true;

    [Header("Sampling")] [SerializeField] private float cellSize = 0.5f;
    [SerializeField] private float _maxSlope = 45f;
    [SerializeField] private LayerMask _walkableMask;
    [SerializeField] private LayerMask _obstacleMask;

    // [Header("Agent")] [SerializeField] private float _agentRadius = 0.4f;
    // [SerializeField] private float _agentHeight = 2.0f;

    [Header("Debug")] [SerializeField] private bool _drawGizmos = true;
    [SerializeField] private bool _drawGizmoLines = true;

    public List<BasePathNode> pathNodes = new();
    private List<BasePathNode> oldPath = new();

    private int randomStart = 0;
    private int randomTarget = 0;

    private float _timer = 0f;
    [SerializeField] private GameObject pathPrefab;

    [Space, Header("Colors")] [SerializeField]
    private Color startColor;

    [SerializeField] private Color targetColor;
    [SerializeField] private Color pathColor;
    [SerializeField] private Color visitedColor;
    [SerializeField] private Color lineColor;

    [Space, SerializeField] private PathFindingManagers.Heuristics heuristic;

    void Awake()
    {
        volume = GetComponent<BoxCollider>();
        volume.isTrigger = true;
    }

    [Button]
    public void Generate()
    {
        pathNodes.Clear();

        Bounds bounds = volume.bounds;
        Vector3 size = bounds.size;
        Vector3 minBounds = bounds.min;

        // Get the amount of cells that fit in the mesh (In 2D space)
        int cellsX = Mathf.CeilToInt(size.x / cellSize);
        int cellsZ = Mathf.CeilToInt(size.z / cellSize);

        Vector3 startPosition = minBounds + new Vector3(cellSize / 2f, 0f, cellSize / 2f);

        for (int x = 0; x < cellsX; x++)
        {
            for (int z = 0; z < cellsZ; z++)
            {
                Vector3 currentGridPosition = startPosition +
                                              new Vector3(x * cellSize, 0, z * cellSize);

                // I guess you can use raycast like this to find the height of the cell?
                Ray ray = new Ray(currentGridPosition + Vector3.up * bounds.size.y, Vector3.down);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, bounds.size.y * 2, _walkableMask))
                {
                    if (!bounds.Contains(hit.point)) continue;
                    if (IsWalkable(hit) is false) continue;

                    // Blocked paths
                    if (Physics.CheckSphere(
                            hit.point,
                            cellSize,
                            _obstacleMask)) continue;

                    // todo: idk if this actually works
                    // // No wall hugging
                    // if (Physics.CheckCapsule(
                    //         hit.point + Vector3.up * 0.05f,
                    //         hit.point + Vector3.up * _agentHeight,
                    //         _agentRadius,
                    //         _obstacleMask)) continue;


                    pathNodes.Add(new BasePathNode(hit.point.ToUnity()));
                }
            }
        }


        float neighborDist = cellSize * (canMoveDiagonal ? 1.5f : 1.25f);
        int connectionLimit = 8; // cardinal + diagonal

        foreach (BasePathNode pathNode in pathNodes)
        {
            int connections = 0;
            foreach (BasePathNode potentialConnection in pathNodes)
            {
                if (connections >= connectionLimit) break;
                if (pathNode == potentialConnection) continue;

                float dist = Vector3.Distance(pathNode.Coordinates.ToUnity(),
                    potentialConnection.Coordinates.ToUnity());
                if (dist <= neighborDist)
                {
                    Vector3 a = pathNode.Coordinates.ToUnity() + Vector3.up * 0.1f;
                    Vector3 bPos = potentialConnection.Coordinates.ToUnity() + Vector3.up * 0.1f;

                    if (!Physics.Linecast(a, bPos, _obstacleMask)) // make sure connection is not being blocked
                    {
                        pathNode.Neighbors.Add(potentialConnection);
                        connections++;
                    }
                }
            }
        }

        Debug.Log($"Nav generated: {pathNodes.Count} nodes");
    }

    bool IsWalkable(RaycastHit hit)
    {
        float slope = Vector3.Angle(hit.normal, Vector3.up);
        return slope <= _maxSlope;
    }

    void OnDrawGizmos()
    {
        if (!_drawGizmos || pathNodes == null) return;

        foreach (var node in pathNodes)
        {
            Gizmos.color = SwitchColorOnState(node.State);
            Gizmos.DrawSphere(node.Coordinates.ToUnity(), cellSize * 0.2f);

            if (_drawGizmoLines)
            {
                Gizmos.color = lineColor;
                foreach (var n in node.Neighbors)
                {
                    Gizmos.DrawLine(node.Coordinates.ToUnity(), n.Coordinates.ToUnity());
                }
            }
        }
    }

    private Color SwitchColorOnState(NodeState state)
    {
        if (state == NodeState.Visited) return visitedColor;
        if (state == NodeState.Path) return pathColor;

        return Color.green;
    }

    [Button]
    private void Astar()
    {
        PreSetupPathFinding();

        _ = PathFindingManagers.AstarPath(
            pathNodes[randomStart],
            pathNodes[randomTarget], heuristic, PathCallBack);
    }

    [Button]
    private void Dijkstra()
    {
        PreSetupPathFinding();

        _ = PathFindingManagers.DijkstraPath(
            pathNodes[randomStart],
            pathNodes[randomTarget], heuristic, PathCallBack);
    }

    [Button]
    private void BreadthFirstSearch()
    {
        PreSetupPathFinding();

        _ = PathFindingManagers.BreadthFirstSearch(
            pathNodes[randomStart],
            pathNodes[randomTarget], PathCallBack);
    }

    [Button]
    public void FloodFill()
    {
        PreSetupPathFinding();

        _ = PathFindingManagers.FloodFillPathAsync(
            pathNodes[randomStart], PathCallBack);
    }

    private void PathCallBack(List<BasePathNode> path, HashSet<BasePathNode> visited)
    {
        if (path == null)
        {
            Debug.LogWarning("Path failed");
            return;
        }

        Debug.Log("Time to calculate path: " + (Time.realtimeSinceStartup - _timer).ToString("F3") + " seconds");
        Debug.Log("Path generated!");

        StartCoroutine(GeneratePath(path, visited));
    }

    private void PreSetupPathFinding()
    {
        // Clean up if any old path
        oldPath.ForEach(path => { path.State = NodeState.Default; });
        oldPath.Clear();

        // Get new random targets
        randomStart = Random.Range(0, pathNodes.Count);
        randomTarget = Random.Range(0, pathNodes.Count);

        // Reset Timer
        _timer = Time.realtimeSinceStartup;
    }

    IEnumerator GeneratePath(List<BasePathNode> path, HashSet<BasePathNode> visited = null)
    {
        foreach (BasePathNode p in path)
        {
            oldPath.Add(p);
            p.State = NodeState.Path;

            // yield return new WaitForSeconds(0.1f);
        }

        if (visited == null) yield break;

        foreach (BasePathNode p in visited)
        {
            oldPath.Add(p);
            p.State = NodeState.Visited;

            // yield return new WaitForSeconds(0.01f);
        }
    }
}