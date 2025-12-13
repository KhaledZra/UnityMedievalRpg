using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    private static int _batchWorkCount = 10;

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
        HashSet<Node> directions = new()
        {
            // Cardinal Direction
            PathFinder.Instance[node.Coordinates + Vector3Int.forward], // North
            PathFinder.Instance[node.Coordinates + Vector3Int.right], // east
            PathFinder.Instance[node.Coordinates + Vector3Int.back], // south
            PathFinder.Instance[node.Coordinates + Vector3Int.left], // west
        };

        if (PathFinder.Instance.canMoveSideways)
        {
            directions.UnionWith(new[]
            {
                // Diagonal
                PathFinder.Instance[node.Coordinates + Vector3Int.forward + Vector3Int.right], // NorthEast
                PathFinder.Instance[node.Coordinates + Vector3Int.forward + Vector3Int.left], // NorthWest
                PathFinder.Instance[node.Coordinates + Vector3Int.back + Vector3Int.right], // southEast
                PathFinder.Instance[node.Coordinates + Vector3Int.back + Vector3Int.left], // SouthWest
            });
        }

        // Vertical neighbors
        if (PathFinder.Instance.canMoveVertical)
        {
            // up & down
            List<Node> verticalNodes = new List<Node>
            {
                PathFinder.Instance[node.Coordinates + Vector3Int.up],
                PathFinder.Instance[node.Coordinates + Vector3Int.down]
            };

            if (PathFinder.Instance.canMoveSideways)
            {
                // Sideways stuff
                foreach (Node n in directions)
                {
                    if (n == null) continue;

                    verticalNodes.Add(PathFinder.Instance[n.Coordinates + Vector3Int.up]);
                    verticalNodes.Add(PathFinder.Instance[n.Coordinates + Vector3Int.down]);
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
        int dx = Mathf.Abs(start.Coordinates.x - goal.Coordinates.x);
        int dy = Mathf.Abs(start.Coordinates.y - goal.Coordinates.y);
        int dz = Mathf.Abs(start.Coordinates.z - goal.Coordinates.z);

        int cardinal = 10;

        return cardinal * Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    private static float Manhattan(Node start, Node goal)
    {
        int dx = Mathf.Abs(start.Coordinates.x - goal.Coordinates.x);
        int dy = Mathf.Abs(start.Coordinates.y - goal.Coordinates.y);
        int dz = Mathf.Abs(start.Coordinates.z - goal.Coordinates.z);

        int cardinal = 10;

        return cardinal * (dx + dy + dz);
    }

    private static float Chebsyshev(Node start, Node goal)
    {
        int dx = Mathf.Abs(start.Coordinates.x - goal.Coordinates.x);
        int dy = Mathf.Abs(start.Coordinates.y - goal.Coordinates.y);
        int dz = Mathf.Abs(start.Coordinates.z - goal.Coordinates.z);

        int cardinal = 10;
        int diagonal = 10;

        return cardinal * (dx + dy + dz) + (diagonal - 2 * cardinal) * Mathf.Min(dx, Mathf.Min(dy, dz));
    }

    private static float Octile(Node start, Node goal)
    {
        int dx = Mathf.Abs(start.Coordinates.x - goal.Coordinates.x);
        int dy = Mathf.Abs(start.Coordinates.y - goal.Coordinates.y);
        int dz = Mathf.Abs(start.Coordinates.z - goal.Coordinates.z);

        int cardinal = 10;
        int diagonal = 14;

        // In 3D, the Octile equivalent considers the smallest distance as "diagonal", rest as cardinal
        int min = Mathf.Min(dx, Mathf.Min(dy, dz));
        int max = Mathf.Max(dx, Mathf.Max(dy, dz));
        int mid = dx + dy + dz - min - max;

        return diagonal * min + cardinal * (mid + max - min);
    }

    #endregion
}