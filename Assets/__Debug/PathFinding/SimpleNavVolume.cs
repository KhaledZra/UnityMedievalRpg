using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class NavNode
{
    public Vector3 position;
    public List<NavNode> neighbors = new();

    public NavNode(Vector3 pos)
    {
        position = pos;
    }
}

[RequireComponent(typeof(BoxCollider))]
public class SimpleNavVolume : MonoBehaviour
{
    [Header("Area")] [SerializeField] private BoxCollider volume;

    [Header("Sampling")] [SerializeField] private float cellSize = 0.5f;
    [SerializeField] private float _maxSlope = 45f;
    [SerializeField] private LayerMask _walkableMask;
    [SerializeField] private LayerMask _obstacleMask;

    [Header("Agent")] [SerializeField] private float _agentRadius = 0.4f;
    [SerializeField] private float _agentHeight = 2.0f;

    [Header("Debug")] [SerializeField] private bool _drawGizmos = true;

    [SerializeField] public List<NavNode> nodes = new List<NavNode>(); // todo refactor into the [,] system i had b4

    void Awake()
    {
        volume = GetComponent<BoxCollider>();
        volume.isTrigger = true;
    }

    // void Start()
    // {
    //     Generate();
    // }

    [Button]
    public void Generate()
    {
        nodes.Clear();

        Bounds bounds = volume.bounds;
        Vector3 size = bounds.size;
        Vector3 minBounds = bounds.min;

        // Get the amount of cells that fit in the mesh (In 2D space)
        int cellsX = Mathf.CeilToInt(size.x / cellSize);
        int cellsZ = Mathf.CeilToInt(size.z / cellSize);

        Vector3 startPosition = minBounds + new Vector3(cellSize / 2f, 0, cellSize / 2f);

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


                    nodes.Add(new NavNode(hit.point));
                }
            }
        }

        // todo: probably needs a refactor or something 1.5 for sideways. 1.1 for normal
        // Adding neighbors
        float neighborDist = cellSize * 1.5f;

        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = 0; j < nodes.Count; j++)
            {
                if (i == j) continue;

                float dist = Vector3.Distance(nodes[i].position, nodes[j].position);
                if (dist <= neighborDist)
                {
                    Vector3 a = nodes[i].position + Vector3.up * 0.1f;
                    Vector3 bPos = nodes[j].position + Vector3.up * 0.1f;

                    if (!Physics.Linecast(a, bPos, _obstacleMask))
                    {
                        nodes[i].neighbors.Add(nodes[j]);
                    }
                }
            }
        }

        Debug.Log($"Nav generated: {nodes.Count} nodes");
    }

    bool IsWalkable(RaycastHit hit)
    {
        float slope = Vector3.Angle(hit.normal, Vector3.up);
        return slope <= _maxSlope;
    }

    void OnDrawGizmos()
    {
        if (!_drawGizmos || nodes == null) return;

        Gizmos.color = Color.green;
        foreach (var node in nodes)
        {
            Gizmos.DrawSphere(node.position, cellSize * 0.2f);

            Gizmos.color = Color.yellow;
            foreach (var n in node.neighbors)
            {
                Gizmos.DrawLine(node.position, n.position);
            }

            Gizmos.color = Color.green;
        }
    }
}