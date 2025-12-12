using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PathFindingManagers
{
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
    
    public static List<Node> AstarPath(Node start, Node goal)
    {
        if (start == null || goal == null) return null;
        
        // SortedSet with custom TileComparer. Basically Temu PriorityQueue
        SortedSet<Node> openNodes = new SortedSet<Node>(new TileComparer());
        HashSet<Node> closedNodes = new HashSet<Node>();


        foreach (Node pathNode in PathFinder.Instance.pathNodes)
        {
            if (pathNode == null) continue;
            pathNode.gScore = float.MaxValue;
        }

        start.gScore = 0;
        start.hScore = Vector3.Distance(start.transform.position, goal.transform.position);

        openNodes.Add(start);

        int iteration = 0;

        while (openNodes.Count > 0)
        {
            iteration++;
            if (iteration > 1000) break; // limit so i don't crash unity
            Debug.Log("Iteration: " + openNodes.Count);

            Node currentNode = openNodes.Min;
            openNodes.Remove(currentNode);
            closedNodes.Add(currentNode);

            if (currentNode == goal)
            {
                List<Node> path = new List<Node> { goal };

                while (currentNode != start)
                {
                    currentNode = currentNode.Parent;
                    path.Add(currentNode);
                }

                path.Reverse();
                path.AddRange(closedNodes);
                return path;
            }

            foreach (Node neighbor in GetNeighbours(currentNode))
            {
                if (neighbor == null) continue;
                if (closedNodes.Contains(neighbor)) continue;

                float heldGScore = currentNode.gScore +
                                   Vector3.Distance(currentNode.transform.position, neighbor.transform.position);

                if (heldGScore < neighbor.gScore)
                {
                    // Remove since we are changing values
                    if (openNodes.Contains(neighbor)) openNodes.Remove(neighbor);
                    
                    neighbor.Parent = currentNode;
                    neighbor.gScore = heldGScore;
                    neighbor.hScore = Vector3.Distance(neighbor.transform.position, goal.transform.position);
                    
                    // Add back so it's sorted
                    openNodes.Add(neighbor);
                }
            }
        }

        return null; // No path found
    }

    // FloodFill - BFS/Breadth First Search

    public static List<Node> FloodFillPath(Node start)
    {
        if (start == null) return new List<Node>();

        Queue<Node> openNodes = new Queue<Node>(); // the ones we need to check
        HashSet<Node> closedNodes = new HashSet<Node>(); // this one is the ones we have checked

        openNodes.Enqueue(start);

        while (openNodes.Count > 0)
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


        return closedNodes.ToList();
    }

    public static List<Node> BreadthFirstSearch(Node start, Node goal)
    {
        if (start == null || goal == null) return new List<Node>();

        Queue<Node> openNodes = new Queue<Node>(); // the ones we need to check
        HashSet<Node> closedNodes = new HashSet<Node>(); // this one is the ones we have checked

        openNodes.Enqueue(start);

        while (openNodes.Count > 0)
        {
            Node currentNode = openNodes.Dequeue();
            closedNodes.Add(currentNode);

            if (currentNode == goal)
            {
                List<Node> path = CreatePath(currentNode, start);
                path.AddRange(closedNodes);
                return path;
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


        return new List<Node>();
    }

    public static List<Node> DijkstraPath(Node start, Node goal)
    {
        if (start == null || goal == null) return new List<Node>();

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
            Node currentNode = openNodes.Min;
            openNodes.Remove(currentNode);
            closedNodes.Add(currentNode);

            if (currentNode == goal)
            {
                List<Node> path = CreatePath(currentNode, start);
                path.AddRange(closedNodes);
                return path;
            }

            foreach (Node neighbor in GetNeighbours(currentNode))
            {
                if (neighbor == null) continue; // node can be out of bounds/wall/idk
                if (closedNodes.Contains(neighbor)) continue; // it's already been marked
                
                float heldGScore = currentNode.gScore +
                                   Vector3.Distance(currentNode.transform.position, neighbor.transform.position);

                if (heldGScore < neighbor.gScore)
                {
                    if (openNodes.Contains(neighbor))  openNodes.Remove(neighbor);
                    
                    neighbor.Parent = currentNode;
                    neighbor.gScore = heldGScore;
                    
                    openNodes.Add(neighbor);
                }
            }
        }


        return new List<Node>();
    }

    private static List<Node> GetNeighbours(Node node)
    {
        List<Node> directions = new List<Node>
        {
            // Cardinal Direction
            PathFinder.Instance[node.Coordinates + Vector2Int.up], // North
            PathFinder.Instance[node.Coordinates + Vector2Int.right], // east
            PathFinder.Instance[node.Coordinates + Vector2Int.down], // south
            PathFinder.Instance[node.Coordinates + Vector2Int.left], // west
        };

        if (PathFinder.Instance.canMoveSideways)
        {
            directions.AddRange(new []
            {
                // Diagonal
                PathFinder.Instance[node.Coordinates + Vector2Int.up + Vector2Int.right], // NorthEast
                PathFinder.Instance[node.Coordinates + Vector2Int.up + Vector2Int.left], // NorthWest
                PathFinder.Instance[node.Coordinates + Vector2Int.down + Vector2Int.right], // southEast
                PathFinder.Instance[node.Coordinates + Vector2Int.down + Vector2Int.left], // SouthWest
            });
        }
        
        return directions;
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
}