using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PathFinding.Nodes;

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
    private class TileComparer : IComparer<PathNodeData>
    {
        public int Compare(PathNodeData a, PathNodeData b)
        {
            float f = a.FScore() - b.FScore();
            if (f < 0) return -1;
            if (f > 0) return 1;

            // Dupe check
            return a.GetHashCode().CompareTo(b.GetHashCode());
        }
    }

    #region PathFindingAlgorithms

    public static async Task FloodFillPathAsync(BasePathNode start,
        Action<List<BasePathNode>, HashSet<BasePathNode>> callback)
    {
        if (start == null)
        {
            callback?.Invoke(null, null);
            return;
        }

        Queue<BasePathNode> openNodes = new(); // the ones we need to check
        HashSet<BasePathNode> closedNodes = new(); // this one is the ones we have checked

        openNodes.Enqueue(start);

        await Task.Run(() =>
        {
            while (openNodes.Count > 0)
            {
                BasePathNode currentBasePathNode = openNodes.Dequeue();
                closedNodes.Add(currentBasePathNode);

                foreach (BasePathNode neighbor in currentBasePathNode.Neighbors)
                {
                    if (neighbor == null) continue; // node can be out of bounds/wall/idk
                    if (closedNodes.Contains(neighbor)) continue; // it's already been marked
                    if (openNodes.Contains(neighbor)) continue; // makes sure we are ignoring dupe connections

                    openNodes.Enqueue(neighbor);
                }
            }
        });

        callback?.Invoke(closedNodes.ToList(), null);
    }

    public static async Task BreadthFirstSearch(BasePathNode start, BasePathNode goal,
        Action<List<BasePathNode>, HashSet<BasePathNode>> callback, bool includeVisited = true)
    {
        if (start == null || goal == null)
        {
            callback?.Invoke(null, null);
            return;
        }

        Queue<BasePathNode> openNodes = new(); // the ones we need to check
        HashSet<BasePathNode> closedNodes = new(); // this one is the ones we have checked
        List<BasePathNode> path = null;

        openNodes.Enqueue(start);

        await Task.Run(() =>
        {
            while (openNodes.Count > 0)
            {
                BasePathNode currentBasePathNode = openNodes.Dequeue();
                closedNodes.Add(currentBasePathNode);

                if (currentBasePathNode == goal)
                {
                    path = CreatePath(currentBasePathNode, start);
                    break;
                }

                foreach (BasePathNode neighbor in currentBasePathNode.Neighbors)
                {
                    if (neighbor == null) continue; // node can be out of bounds/wall/idk
                    if (closedNodes.Contains(neighbor)) continue; // it's already been marked
                    if (openNodes.Contains(neighbor)) continue; // makes sure we are ignoring dupe connections

                    openNodes.Enqueue(neighbor);
                    neighbor.Parent = currentBasePathNode;
                }
            }
        });

        if (path == null)
        {
            callback?.Invoke(null, null);
        }
        else
        {
            callback?.Invoke(path, includeVisited ? closedNodes.Except(path).ToHashSet() : null);
        }
    }

    public static async Task DijkstraPath(BasePathNode start, BasePathNode goal, Heuristics heuristic,
        Action<List<BasePathNode>, HashSet<BasePathNode>> callback, bool includeVisited = true)
    {
        if (start == null || goal == null)
        {
            callback?.Invoke(null, null);
            return;
        }

        // SortedSet with custom TileComparer. Basically Temu PriorityQueue
        SortedSet<PathNodeData> openNodes = new(new TileComparer());
        HashSet<BasePathNode> closedNodes = new(); // this one is the ones we have checked
        Dictionary<BasePathNode, PathNodeData> dataDict = new();
        List<BasePathNode> path = null;

        PathNodeData currentData = GetData(start, dataDict);
        currentData.GScore = 0;
        currentData.HScore = CalculateHeuristic(start, goal, heuristic);
        
        openNodes.Add(currentData);

        await Task.Run(() =>
        {
            while (openNodes.Count > 0)
            {
                currentData = openNodes.Min;
                openNodes.Remove(currentData);
                closedNodes.Add(currentData.Node);

                if (currentData.Node == goal)
                {
                    path = CreatePath(currentData.Node, start);
                    return;
                }

                foreach (BasePathNode neighbor in currentData.Node.Neighbors)
                {
                    if (neighbor == null) continue;
                    if (closedNodes.Contains(neighbor)) continue;
                    
                    PathNodeData newData = GetData(neighbor, dataDict);

                    float heldGScore = currentData.GScore +
                                       CalculateHeuristic(currentData.Node, neighbor, heuristic);
                    
                    if (heldGScore < newData.GScore)
                    {
                        // Remove since we are changing values
                        if (openNodes.Contains(newData)) openNodes.Remove(newData);

                        newData.Node.Parent = currentData.Node;
                        newData.GScore = heldGScore;

                        // Add back so it's sorted
                        openNodes.Add(newData);
                    }
                }
            }
        });


        if (path == null)
        {
            callback?.Invoke(null, null);
        }
        else
        {
            callback?.Invoke(path, includeVisited ? closedNodes.Except(path).ToHashSet() : null);
        }
    }

    public static async Task AstarPath(BasePathNode start, BasePathNode goal, Heuristics heuristic,
        Action<List<BasePathNode>, HashSet<BasePathNode>> callback, bool includeVisited = true)
    {
        if (start == null || goal == null)
        {
            callback?.Invoke(null, null);
            return;
        }

        // SortedSet with custom TileComparer. Basically Temu PriorityQueue
        SortedSet<PathNodeData> openNodes = new(new TileComparer());
        HashSet<BasePathNode> closedNodes = new();
        Dictionary<BasePathNode, PathNodeData> dataDict = new();
        List<BasePathNode> path = null;

        PathNodeData currentData = GetData(start, dataDict);
        currentData.GScore = 0;
        currentData.HScore = CalculateHeuristic(start, goal, heuristic);

        openNodes.Add(currentData);

        await Task.Run(() =>
        {
            while (openNodes.Count > 0)
            {
                currentData = openNodes.Min;
                openNodes.Remove(currentData);
                closedNodes.Add(currentData.Node);

                if (currentData.Node == goal)
                {
                    path = CreatePath(currentData.Node, start);
                    return;
                }

                foreach (BasePathNode neighbor in currentData.Node.Neighbors)
                {
                    if (neighbor == null) continue;
                    if (closedNodes.Contains(neighbor)) continue;
                    
                    PathNodeData newData = GetData(neighbor, dataDict);

                    float heldGScore = currentData.GScore +
                                       CalculateHeuristic(currentData.Node, neighbor, heuristic);

                    if (heldGScore < newData.GScore)
                    {
                        // Remove since we are changing values
                        if (openNodes.Contains(newData)) openNodes.Remove(newData);

                        newData.Node.Parent = currentData.Node;
                        newData.GScore = heldGScore;
                        newData.HScore = CalculateHeuristic(neighbor, goal, heuristic);

                        // Add back so it's sorted
                        openNodes.Add(newData);
                    }
                }
            }
        });

        if (path == null)
        {
            callback?.Invoke(null, null);
        }
        else
        {
            callback?.Invoke(path, includeVisited ? closedNodes.Except(path).ToHashSet() : null);
        }
    }

    #endregion

    #region Utility

    // Get data if it does not exist create and return that
    private static PathNodeData GetData(BasePathNode node, Dictionary<BasePathNode, PathNodeData> dataDict)
    {
        if (!dataDict.TryGetValue(node, out var data))
            dataDict[node] = data = new PathNodeData(node);
        return data;
    }

    // todo: might have to delete : - [
    // private static List<BasePathNode> GetNeighbours<BasePathNode>(BasePathNode node)
    // {
    //     VInt3 currentCoord = new VInt3(node.Coordinates.x, node.Coordinates.y, node.Coordinates.z);
    //     HashSet<BasePathNode> directions = new()
    //     {
    //         // Cardinal Direction
    //         PathFinder.Instance[currentCoord + VInt3.Forward], // North
    //         PathFinder.Instance[currentCoord + VInt3.Right], // east
    //         PathFinder.Instance[currentCoord + VInt3.Back], // south
    //         PathFinder.Instance[currentCoord + VInt3.Left], // west
    //     };
    //
    //     if (PathFinder.Instance.canMoveSideways)
    //     {
    //         directions.UnionWith(new[]
    //         {
    //             // Diagonal
    //             PathFinder.Instance[currentCoord + VInt3.ForwardRight], // NorthEast
    //             PathFinder.Instance[currentCoord + VInt3.ForwardLeft], // NorthWest
    //             PathFinder.Instance[currentCoord + VInt3.BackRight], // southEast
    //             PathFinder.Instance[currentCoord + VInt3.BackLeft], // SouthWest
    //         });
    //     }
    //
    //     // Vertical neighbors
    //     if (PathFinder.Instance.canMoveVertical)
    //     {
    //         // up & down
    //         List<BasePathNode> verticalNodes = new()
    //         {
    //             PathFinder.Instance[currentCoord + VInt3.Up],
    //             PathFinder.Instance[currentCoord + VInt3.Down]
    //         };
    //
    //         if (PathFinder.Instance.canMoveSideways)
    //         {
    //             // Sideways stuff
    //             foreach (BasePathNode n in directions)
    //             {
    //                 if (n == null) continue;
    //
    //                 VInt3 cc = new VInt3(n.Coordinates.x, n.Coordinates.y, n.Coordinates.z);
    //
    //                 verticalNodes.Add(PathFinder.Instance[cc + VInt3.Up]);
    //                 verticalNodes.Add(PathFinder.Instance[cc + VInt3.Down]);
    //             }
    //         }
    //
    //         directions.UnionWith(verticalNodes);
    //     }
    //
    //     return directions.ToList();
    // }

    private static List<BasePathNode> CreatePath(BasePathNode start, BasePathNode goal)
    {
        List<BasePathNode> path = new() { goal };

        while (start != goal)
        {
            path.Add(start);
            start = start.Parent;
        }

        path.Reverse();
        path.Insert(0, start);
        path.RemoveAt(path.Count - 1);
        return path;
    }

    private static float CalculateHeuristic(BasePathNode start, BasePathNode goal, Heuristics heuristic)
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

    private static float Euclidean(BasePathNode start, BasePathNode goal)
    {
        float dx = Math.Abs(start.Coordinates.X - goal.Coordinates.X);
        float dy = Math.Abs(start.Coordinates.Y - goal.Coordinates.Y);
        float dz = Math.Abs(start.Coordinates.Z - goal.Coordinates.Z);

        float cardinal = 1f;

        return (float)(cardinal * Math.Sqrt(dx * dx + dy * dy + dz * dz));
    }

    private static float Manhattan(BasePathNode start, BasePathNode goal)
    {
        float dx = Math.Abs(start.Coordinates.X - goal.Coordinates.X);
        float dy = Math.Abs(start.Coordinates.Y - goal.Coordinates.Y);
        float dz = Math.Abs(start.Coordinates.Z - goal.Coordinates.Z);

        float cardinal = 1f;

        return cardinal * (dx + dy + dz);
    }

    private static float Chebsyshev(BasePathNode start, BasePathNode goal)
    {
        float dx = Math.Abs(start.Coordinates.X - goal.Coordinates.X);
        float dy = Math.Abs(start.Coordinates.Y - goal.Coordinates.Y);
        float dz = Math.Abs(start.Coordinates.Z - goal.Coordinates.Z);

        float cardinal = 1f;
        float diagonal = 1f;

        return cardinal * (dx + dy + dz) + (diagonal - 2 * cardinal) * Math.Min(dx, Math.Min(dy, dz));
    }

    private static float Octile(BasePathNode start, BasePathNode goal)
    {
        float dx = Math.Abs(start.Coordinates.X - goal.Coordinates.X);
        float dy = Math.Abs(start.Coordinates.Y - goal.Coordinates.Y);
        float dz = Math.Abs(start.Coordinates.Z - goal.Coordinates.Z);

        float cardinal = 1f;
        float diagonal = 1.45f;

        // In 3D, the Octile equivalent considers the smallest distance as "diagonal", rest as cardinal
        float min = Math.Min(dx, Math.Min(dy, dz));
        float max = Math.Max(dx, Math.Max(dy, dz));
        float mid = dx + dy + dz - min - max;

        return diagonal * min + cardinal * (mid + max - min);
    }

    #endregion
}