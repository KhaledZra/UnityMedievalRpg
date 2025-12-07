using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Node Parent = null;
    public List<Node> Neihbours = new();

    public float gScore;
    public float hScore;

    public float FScore()
    {
        return gScore + hScore;
    }
    
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (Node node in Neihbours)
        {
            Gizmos.DrawLine(transform.position, node.transform.position);
        }
    }
}