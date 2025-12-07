using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Node Parent;
    public List<Node> Neihbours;

    public float gScore;
    public float hScore;

    public float FScore()
    {
        return gScore + hScore;
    }
}