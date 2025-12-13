using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class PathFindingManagers
{
    public enum Heuristics
    {
        Manhattan,
        Chebsyshev,
        Octile,
        Euclidean
    }

    // Custom comparer for SortedSet
    private class TileComparer : IComparer<Node>
    {
        public int Compare(Node a, Node b)
        {
            float f = a.FScore() - b.FScore();
            if (f < 0) return -1;
            if (f > 0) return 1;

            // Dupe check
            return a.GetHashCode().CompareTo(b.GetHashCode());
        }
    }

    // Custom VInt3 struct
    public struct VInt3
    {
        public readonly int x;
        public readonly int y;
        public readonly int z;

        public VInt3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static VInt3 operator +(VInt3 a, VInt3 b) => new(a.x + b.x, a.y + b.y, a.z + b.z);
        public static VInt3 operator -(VInt3 a, VInt3 b) => new(a.x - b.x, a.y - b.y, a.z - b.z);

        public static readonly VInt3 Forward = new(0, 0, 1);
        public static readonly VInt3 Back = new(0, 0, -1);
        public static readonly VInt3 Right = new(1, 0, 0);
        public static readonly VInt3 Left = new(-1, 0, 0);
        public static readonly VInt3 Up = new(0, 1, 0);
        public static readonly VInt3 Down = new(0, -1, 0);

        // Diagonals
        public static readonly VInt3 ForwardRight = new(1, 0, 1);
        public static readonly VInt3 ForwardLeft = new(-1, 0, 1);
        public static readonly VInt3 BackRight = new(1, 0, -1);
        public static readonly VInt3 BackLeft = new(-1, 0, -1);
    }

    private static readonly int _batchWorkCount = 10;

    #region PathFindingAlgorithms

    public static IEnumerator FloodFillPath(Node start, Action<List<Node>, HashSet<Node>> callback)
    {
        if (start == null)
        {
            callback?.Invoke(null, null);
            yield break;
        }

        Queue<Node> openNodes = new Queue<Node>(); // the ones we need to check
        HashSet<Node> closedNodes = new HashSet<Node>(); // this one is the ones we have checked

        openNodes.Enqueue(start);

        while (openNodes.Count > 0)
        {
            for (int i = 0; i < _batchWorkCount && openNodes.Count > 0; i++) // Batch work per frame
            {
                Node currentNode = openNodes.Dequeue();
                closedNodes.Add(currentNode);

                foreach (Node neighbor in GetNeighbours(currentNode))
                {
                    if (neighbor == null) continue; // node can be out of bounds/wall/idk
                    if (closedNodes.Contains(neighbor)) continue; // it's already been marked
                    if (openNodes.Contains(neighbor)) continue; // makes sure we are ignoring dupe connections

                    openNodes.Enqueue(neighbor);
                }
            }

            yield return null;
        }


        callback?.Invoke(closedNodes.ToList(), null);
    }

    public static IEnumerator BreadthFirstSearch(Node start, Node goal,
        Action<List<Node>, HashSet<Node>> callback, bool includeVisited = true)
    {
        if (start == null || goal == null)
        {
            callback?.Invoke(null, null);
            yield break;
        }

        Queue<Node> openNodes = new Queue<Node>(); // the ones we need to check
        HashSet<Node> closedNodes = new HashSet<Node>(); // this one is the ones we have checked

        openNodes.Enqueue(start);

        while (openNodes.Count > 0)
        {
            for (int i = 0; i < _batchWorkCount && openNodes.Count > 0; i++) // Batch work per frame
            {
                Node currentNode = openNodes.Dequeue();
                closedNodes.Add(currentNode);

                if (currentNode == goal)
                {
                    List<Node> path = CreatePath(currentNode, start);
                    // Include search area if we want it
                    callback?.Invoke(path, includeVisited ? closedNodes.Except(path).ToHashSet() : null);
                    yield break;
                }

                foreach (Node neighbor in GetNeighbours(currentNode))
                {
                    if (neighbor == null) continue; // node can be out of bounds/wall/idk
                    if (closedNodes.Contains(neighbor)) continue; // it's already been marked
                    if (openNodes.Contains(neighbor)) continue; // makes sure we are ignoring dupe connections

                    openNodes.Enqueue(neighbor);
                    neighbor.Parent = currentNode;
                }
            }

            yield return null;
        }

        callback?.Invoke(null, null);
    }

    public static IEnumerator DijkstraPath(Node start, Node goal, Heuristics heuristic,
        Action<List<Node>, HashSet<Node>> callback, bool includeVisited = true)
    {
        if (start == null || goal == null)
        {
            callback?.Invoke(null, null);
            yield break;
        }

        // SortedSet with custom TileComparer. Basically Temu PriorityQueue
        SortedSet<Node> openNodes = new SortedSet<Node>(new TileComparer());
        HashSet<Node> closedNodes = new HashSet<Node>(); // this one is the ones we have checked

        foreach (Node node in PathFinder.Instance.pathNodes)
        {
            if (node == null) continue;
            node.gScore = float.MaxValue;
            node.hScore = 0;
        }

        start.gScore = 0;
        openNodes.Add(start);

        while (openNodes.Count > 0)
        {
            for (int i = 0; i < _batchWorkCount && openNodes.Count > 0; i++) // Batch work per frame
            {
                Node currentNode = openNodes.Min;
                openNodes.Remove(currentNode);
                closedNodes.Add(currentNode);

                if (currentNode == goal)
                {
                    List<Node> path = CreatePath(currentNode, start);
                    // Include search area if we want it
                    callback?.Invoke(path, includeVisited ? closedNodes.Except(path).ToHashSet() : null);
                    yield break;
                }

                foreach (Node neighbor in GetNeighbours(currentNode))
                {
                    if (neighbor == null) continue; // node can be out of bounds/wall/idk
                    if (closedNodes.Contains(neighbor)) continue; // it's already been marked

                    float heldGScore =
                        currentNode.gScore + CalculateHeuristic(currentNode, neighbor, heuristic);

                    if (heldGScore < neighbor.gScore)
                    {
                        if (openNodes.Contains(neighbor)) openNodes.Remove(neighbor);

                        neighbor.Parent = currentNode;
                        neighbor.gScore = heldGScore;

                        openNodes.Add(neighbor);
                    }
                }
            }

            yield return null;
        }

        callback?.Invoke(null, null);
    }

    public static IEnumerator AstarPath(Node start, Node goal, Heuristics heuristic,
        Action<List<Node>, HashSet<Node>> callback, bool includeVisited = true)
    {
        if (start == null || goal == null)
        {
            callback?.Invoke(null, null);
            yield break;
        }

        // SortedSet with custom TileComparer. Basically Temu PriorityQueue
        SortedSet<Node> openNodes = new SortedSet<Node>(new TileComparer());
        HashSet<Node> closedNodes = new HashSet<Node>();


        foreach (Node pathNode in PathFinder.Instance.pathNodes)
        {
            if (pathNode == null) continue;
            pathNode.gScore = float.MaxValue;
        }

        start.gScore = 0;
        start.hScore = CalculateHeuristic(start, goal, heuristic);

        openNodes.Add(start);

        while (openNodes.Count > 0)
        {
            for (int i = 0; i < _batchWorkCount && openNodes.Count > 0; i++) // Batch work per frame
            {
                Node currentNode = openNodes.Min;
                openNodes.Remove(currentNode);
                closedNodes.Add(currentNode);

                if (currentNode == goal)
                {
                    List<Node> path = CreatePath(currentNode, start);
                    // Include search area if we want it
                    callback?.Invoke(path, includeVisited ? closedNodes.Except(path).ToHashSet() : null);
                    yield break;
                }

                foreach (Node neighbor in GetNeighbours(currentNode))
                {
                    if (neighbor == null) continue;
                    if (closedNodes.Contains(neighbor)) continue;

                    float heldGScore = currentNode.gScore + CalculateHeuristic(currentNode, neighbor, heuristic);

                    if (heldGScore < neighbor.gScore)
                    {
                        // Remove since we are changing values
                        if (openNodes.Contains(neighbor)) openNodes.Remove(neighbor);

                        neighbor.Parent = currentNode;
                        neighbor.gScore = heldGScore;
                        neighbor.hScore = CalculateHeuristic(neighbor, goal, heuristic);

                        // Add back so it's sorted
                        openNodes.Add(neighbor);
                    }
                }
            }

            yield return null;
        }

        callback?.Invoke(null, null);
    }

    #endregion

    #region Utility

    private static List<Node> GetNeighbours(Node node)
    {
        VInt3 currentCoord = new VInt3(node.Coordinates.x, node.Coordinates.y, node.Coordinates.z);
        HashSet<Node> directions = new()
        {
            // Cardinal Direction
            PathFinder.Instance[currentCoord + VInt3.Forward], // North
            PathFinder.Instance[currentCoord + VInt3.Right], // east
            PathFinder.Instance[currentCoord + VInt3.Back], // south
            PathFinder.Instance[currentCoord + VInt3.Left], // west
        };

        if (PathFinder.Instance.canMoveSideways)
        {
            directions.UnionWith(new[]
            {
                // Diagonal
                PathFinder.Instance[currentCoord + VInt3.ForwardRight], // NorthEast
                PathFinder.Instance[currentCoord + VInt3.ForwardLeft], // NorthWest
                PathFinder.Instance[currentCoord + VInt3.BackRight], // southEast
                PathFinder.Instance[currentCoord + VInt3.BackLeft], // SouthWest
            });
        }

        // Vertical neighbors
        if (PathFinder.Instance.canMoveVertical)
        {
            // up & down
            List<Node> verticalNodes = new List<Node>
            {
                PathFinder.Instance[currentCoord + VInt3.Up],
                PathFinder.Instance[currentCoord + VInt3.Down]
            };

            if (PathFinder.Instance.canMoveSideways)
            {
                // Sideways stuff
                foreach (Node n in directions)
                {
                    if (n == null) continue;

                    VInt3 cc = new VInt3(n.Coordinates.x, n.Coordinates.y, n.Coordinates.z);

                    verticalNodes.Add(PathFinder.Instance[cc + VInt3.Up]);
                    verticalNodes.Add(PathFinder.Instance[cc + VInt3.Down]);
                }
            }

            directions.UnionWith(verticalNodes);
        }

        return directions.ToList();
    }

    private static List<Node> CreatePath(Node start, Node goal)
    {
        List<Node> path = new List<Node>() { goal };

        while (start != goal)
        {
            path.Add(start);
            start = start.Parent;
        }

        path.Reverse();
        return path;
    }

    private static float CalculateHeuristic(Node start, Node goal, Heuristics heuristic)
    {
        return heuristic switch
        {
            Heuristics.Euclidean => Euclidean(start, goal),
            Heuristics.Manhattan => Manhattan(start, goal),
            Heuristics.Chebsyshev => Chebsyshev(start, goal),
            Heuristics.Octile => Octile(start, goal),
            _ => 0
        };
    }

    #endregion

    #region Heuristics

    private static float Euclidean(Node start, Node goal)
    {
        int dx = Math.Abs(start.Coordinates.x - goal.Coordinates.x);
        int dy = Math.Abs(start.Coordinates.y - goal.Coordinates.y);
        int dz = Math.Abs(start.Coordinates.z - goal.Coordinates.z);

        int cardinal = 10;

        return (float)(cardinal * Math.Sqrt(dx * dx + dy * dy + dz * dz));
    }

    private static float Manhattan(Node start, Node goal)
    {
        int dx = Math.Abs(start.Coordinates.x - goal.Coordinates.x);
        int dy = Math.Abs(start.Coordinates.y - goal.Coordinates.y);
        int dz = Math.Abs(start.Coordinates.z - goal.Coordinates.z);

        int cardinal = 10;

        return cardinal * (dx + dy + dz);
    }

    private static float Chebsyshev(Node start, Node goal)
    {
        int dx = Math.Abs(start.Coordinates.x - goal.Coordinates.x);
        int dy = Math.Abs(start.Coordinates.y - goal.Coordinates.y);
        int dz = Math.Abs(start.Coordinates.z - goal.Coordinates.z);

        int cardinal = 10;
        int diagonal = 10;

        return cardinal * (dx + dy + dz) + (diagonal - 2 * cardinal) * Math.Min(dx, Math.Min(dy, dz));
    }

    private static float Octile(Node start, Node goal)
    {
        int dx = Math.Abs(start.Coordinates.x - goal.Coordinates.x);
        int dy = Math.Abs(start.Coordinates.y - goal.Coordinates.y);
        int dz = Math.Abs(start.Coordinates.z - goal.Coordinates.z);

        int cardinal = 10;
        int diagonal = 14;

        // In 3D, the Octile equivalent considers the smallest distance as "diagonal", rest as cardinal
        int min = Math.Min(dx, Math.Min(dy, dz));
        int max = Math.Max(dx, Math.Max(dy, dz));
        int mid = dx + dy + dz - min - max;

        return diagonal * min + cardinal * (mid + max - min);
    }

    #endregion
}