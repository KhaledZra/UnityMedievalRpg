// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using NaughtyAttributes;
// using PathFinding.Nodes;
// using PathFinding.Structs;
// using UnityEngine;
// using Quaternion = UnityEngine.Quaternion;
// using Vector3 = UnityEngine.Vector3;
//
// public class PathFinder : MonoBehaviour
// {
//     #region Singleton
//
//     public static PathFinder Instance { get; private set; }
//
//     private void Awake()
//     {
//         Instance = this;
//     }
//
//     #endregion
//     
//     [SerializeField] private GameObject pathPrefab;
//
//     // [SerializeField] private Vector2Int start;
//     // [SerializeField] private Vector2Int target;
//     [Space, Header("Colors")] [SerializeField]
//     private Color startColor;
//
//     [SerializeField] private Color targetColor;
//     [SerializeField] private Color pathColor;
//     [SerializeField] private Color visitedColor;
//
//     [Space, Header("Values"), SerializeField, ReadOnly]
//     private Vector3Int start = new(0, 0, 0);
//
//     [SerializeField, ReadOnly] private Vector3Int target = new(0, 0, 0);
//
//     [Space, Header("Settings")] [SerializeField]
//     private Vector3Int NavMeshSize;
//
//     [SerializeField] private LayerMask obstacleMask;
//
//     [SerializeField] public bool canMoveSideways = true;
//     [SerializeField] public bool canMoveVertical = true;
//     [Space, SerializeField] private PathFindingManagers.Heuristics heuristic;
//
//     public BasePathNode<VInt3>[,,] pathNodes;
//     private List<GameObject> oldPath = new();
//
//     private float _timer = 0f;
//
//
//     public Node this[Vector3Int v] => this[v.x, v.y, v.z];
//     public Node this[VInt3 v] => this[v.x, v.y, v.z];
//     public Node this[int x, int y, int z] => WithinBounds(x, y, z) ? pathNodes[x, y, z] : null;
//
//     public bool WithinBounds(Vector3Int v)
//     {
//         return WithinBounds(v.x, v.y, v.z);
//     }
//
//     public bool WithinBounds(int x, int y, int z)
//     {
//         return x >= 0 &&
//                y >= 0 &&
//                z >= 0 &&
//                x < pathNodes.GetUpperBound(0) + 1 &&
//                y < pathNodes.GetUpperBound(1) + 1 &&
//                z < pathNodes.GetUpperBound(2) + 1;
//     }
//
//     private void Start()
//     {
//         // StartCoroutine(UpdateNavMesh());
//
//         if (pathNodes != null)
//         {
//             foreach (Node n in pathNodes)
//             {
//                 if (n == null) continue;
//                 Destroy(n.gameObject);
//             }
//         }
//
//         transform.position = Vector3.zero;
//         CreatePathNodes();
//         UpdatePathNeighbors();
//         transform.position = Vector3.up / 2;
//     }
//
//     private void CreatePathNodes()
//     {
//         pathNodes = new Node[NavMeshSize.x, NavMeshSize.y, NavMeshSize.z];
//
//         for (int x = 0; x <= pathNodes.GetUpperBound(0); x++)
//         {
//             for (int y = 0; y <= pathNodes.GetUpperBound(1); y++)
//             {
//                 for (int z = 0; z <= pathNodes.GetUpperBound(2); z++)
//                 {
//                     Vector3Int currentLocation = new Vector3Int(x, y, z);
//                     // Check if we are hitting a blockedPath
//                     Collider[] cols = Physics.OverlapBox(currentLocation, Vector3.one,
//                         Quaternion.identity, obstacleMask);
//
//                     if (cols.Length == 0)
//                     {
//                         pathNodes[x, y, z] =
//                             Instantiate(nodePrefab,
//                                 currentLocation,
//                                 Quaternion.identity,
//                                 transform);
//
//                         pathNodes[x, y, z].Coordinates = currentLocation;
//                     }
//                 }
//             }
//         }
//     }
//
//     private void UpdatePathNeighbors()
//     {
//         for (int x = 0; x <= pathNodes.GetUpperBound(0); x++)
//         {
//             for (int y = 0; y <= pathNodes.GetUpperBound(1); y++)
//             {
//                 for (int z = 0; z < pathNodes.GetUpperBound(2); z++)
//                 {
//                     if (this[x, y, z] == null) continue;
//
//                     Vector3Int currentLocation = new Vector3Int(x, y, z);
//
//                     HashSet<Node> potentialConnections = new HashSet<Node>
//                     {
//                         this[currentLocation + Vector3Int.forward], // North
//                         this[currentLocation + Vector3Int.right], // East
//                         this[currentLocation + Vector3Int.back], // South
//                         this[currentLocation + Vector3Int.left], // West
//                     };
//
//                     // Diagonal neighbors (corners)
//                     if (canMoveSideways)
//                     {
//                         potentialConnections.UnionWith(new[]
//                         {
//                             this[currentLocation + Vector3Int.forward + Vector3Int.right], // North East
//                             this[currentLocation + Vector3Int.forward + Vector3Int.left], // North West
//                             this[currentLocation + Vector3Int.back + Vector3Int.right], // South East
//                             this[currentLocation + Vector3Int.back + Vector3Int.left], // South West
//                         });
//                     }
//
//                     // Vertical neighbors
//                     if (canMoveVertical)
//                     {
//                         // up & down
//                         List<Node> verticalNodes = new List<Node>
//                         {
//                             this[currentLocation + Vector3Int.up],
//                             this[currentLocation + Vector3Int.down]
//                         };
//
//                         if (canMoveSideways)
//                         {
//                             // Sideways stuff
//                             foreach (Node node in potentialConnections)
//                             {
//                                 if (node == null) continue;
//
//                                 verticalNodes.Add(this[node.Coordinates + Vector3Int.up]);
//                                 verticalNodes.Add(this[node.Coordinates + Vector3Int.down]);
//                             }
//                         }
//
//                         potentialConnections.UnionWith(verticalNodes);
//                     }
//
//                     pathNodes[x, y, z].Neihbours = potentialConnections.Where(node => node != null).ToList();
//                 }
//             }
//         }
//     }
//
//     private IEnumerator UpdateNavMesh()
//     {
//         while (true)
//         {
//             yield return new WaitForSeconds(1f);
//
//             if (pathNodes != null)
//             {
//                 foreach (Node n in pathNodes)
//                 {
//                     if (n == null) continue;
//                     Destroy(n.gameObject);
//                 }
//             }
//
//             transform.position = Vector3.zero;
//             CreatePathNodes();
//             UpdatePathNeighbors();
//             transform.position = Vector3.up / 2;
//         }
//     }
//
//     [Button]
//     private void GenerateNavMesh()
//     {
//         if (pathNodes != null)
//         {
//             foreach (Node n in pathNodes)
//             {
//                 if (n == null) continue;
//                 Destroy(n.gameObject);
//             }
//         }
//
//         transform.position = Vector3.zero;
//         CreatePathNodes();
//         UpdatePathNeighbors();
//         transform.position = Vector3.up / 2;
//     }
//
//     [Button]
//     private void Astar()
//     {
//         PreSetupPathFinding();
//
//         _ = PathFindingManagers.AstarPath(
//             pathNodes[start.x, start.y, start.z],
//             pathNodes[target.x, target.y, target.z],
//             heuristic,
//             PathCallBack);
//     }
//
//     [Button]
//     private void Dijkstra()
//     {
//         PreSetupPathFinding();
//
//         _ = PathFindingManagers.DijkstraPath(
//             pathNodes[start.x, start.y, start.z],
//             pathNodes[target.x, target.y, target.z],
//             heuristic, PathCallBack);
//     }
//
//     [Button]
//     private void BreadthFirstSearch()
//     {
//         PreSetupPathFinding();
//
//         _ = PathFindingManagers.BreadthFirstSearch(
//             pathNodes[start.x, start.y, start.z],
//             pathNodes[target.x, target.y, target.z],
//             PathCallBack);
//     }
//
//     [Button]
//     private void FloodFill()
//     {
//         PreSetupPathFinding();
//         
//         _ = PathFindingManagers.FloodFillPathAsync(
//             pathNodes[start.x, start.y, start.z], PathCallBack);
//     }
//     
//     private IEnumerator RunAsync(Task task) // not used but good to have
//     {
//         while (!task.IsCompleted)
//             yield return null;
//         if (task.Exception != null)
//             throw task.Exception;
//     }
//
//     private void PathCallBack(List<Node> path, HashSet<Node> visited)
//     {
//         if (path == null)
//         {
//             Debug.LogWarning("Path failed");
//             return;
//         }
//
//         Debug.Log("Time to calculate path: " + (Time.realtimeSinceStartup - _timer).ToString("F3") + " seconds");
//         Debug.Log("Path generated!");
//
//         StartCoroutine(GeneratePath(path, visited));
//     }
//
//     private void PreSetupPathFinding()
//     {
//         // Clean up if any old path
//         oldPath.ForEach(Destroy);
//         oldPath.Clear();
//
//         // Get new random targets
//         start = new Vector3Int(
//             Random.Range(0, pathNodes.GetUpperBound(0)),
//             Random.Range(0, pathNodes.GetUpperBound(1)),
//             Random.Range(0, pathNodes.GetUpperBound(2)));
//
//         target = new Vector3Int(
//             Random.Range(0, pathNodes.GetUpperBound(0)),
//             Random.Range(0, pathNodes.GetUpperBound(1)),
//             Random.Range(0, pathNodes.GetUpperBound(2)));
//         
//         // Reset Timer
//         _timer = Time.realtimeSinceStartup;
//     }
//
//     IEnumerator GeneratePath(List<Node> path, HashSet<Node> visited = null)
//     {
//         foreach (Node p in path)
//         {
//             oldPath.Add(Instantiate(pathPrefab, p.transform.position, Quaternion.identity));
//             oldPath.Last().GetComponent<Renderer>().material.color = pathColor;
//
//             // yield return new WaitForSeconds(0.01f);
//         }
//
//         if (visited == null) yield break;
//
//         foreach (Node p in visited)
//         {
//             oldPath.Add(Instantiate(pathPrefab, p.transform.position, Quaternion.identity));
//             oldPath.Last().GetComponent<Renderer>().material.color = visitedColor;
//
//             // yield return new WaitForSeconds(0.01f);
//         }
//     }
// }