using System.Collections.Generic;
using UnityEngine;

public class AStarManager : MonoBehaviour
{
    // Singleton
    public static AStarManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public List<Node> GeneratePath(Node start, Node goal)
    {
        List<Node> openNodes = new List<Node>();
        HashSet<Node> closedNodes = new HashSet<Node>();

        foreach (Node pathNode in PathFinder.Instance.pathNodes)
        {
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

            int lowestF = 0;

            for (int i = 1; i < openNodes.Count; i++)
            {
                if (openNodes[i].FScore() < openNodes[lowestF].FScore())
                {
                    lowestF = i;
                }
            }

            Node currentNode = openNodes[lowestF];
            openNodes.RemoveAt(lowestF);
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
                return path;
            }

            foreach (Node neighbor in currentNode.Neihbours)
            {
                if (closedNodes.Contains(neighbor)) continue;
                
                float heldGScore = currentNode.gScore +
                                   Vector3.Distance(currentNode.transform.position, neighbor.transform.position);

                if (heldGScore < neighbor.gScore)
                {
                    neighbor.Parent = currentNode;
                    neighbor.gScore = heldGScore;
                    neighbor.hScore = Vector3.Distance(neighbor.transform.position, goal.transform.position);
                }

                if (openNodes.Contains(neighbor) is false)
                {
                    openNodes.Add(neighbor);
                }
            }
        }

        return null; // No path found
    }
}