using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    [SerializeField] private Node nodePrefab;
    [SerializeField] private Vector2Int start;
    [SerializeField] private Vector2Int target;
    [SerializeField] private Color startColor;
    [SerializeField] private Color targetColor;
    [SerializeField] private Color pathColor;

    public Node[,] pathNodes;

    private GameObject[,] tiles;
    private Node lastAdded = null;

    private void Start()
    {
        tiles = GridGenerator.Instance.tiles;

        tiles[start.x, start.y].GetComponent<Renderer>().material.color = startColor;
        tiles[target.x, target.y].GetComponent<Renderer>().material.color = targetColor;

        CreatePathNodes();
        UpdatePathNeighbors();
    }

    private void CreatePathNodes()
    {
        pathNodes = new Node[tiles.GetUpperBound(0) + 1, tiles.GetUpperBound(1) + 1];

        for (int i = 0; i <= tiles.GetUpperBound(0); i++)
        {
            for (int j = 0; j <= tiles.GetUpperBound(1); j++)
            {
                pathNodes[i, j] =
                    Instantiate(nodePrefab,
                        GridGenerator.Instance.tiles[i, j].transform.position,
                        Quaternion.identity,
                        transform);

                pathNodes[i, j].Parent = lastAdded;
                lastAdded = pathNodes[i, j];
            }
        }
    }

    private void UpdatePathNeighbors()
    {
        for (int i = 0; i <= tiles.GetUpperBound(0); i++)
        {
            for (int j = 0; j <= tiles.GetUpperBound(1); j++)
            {
                if (j + 1 <= tiles.GetUpperBound(1)) pathNodes[i, j].Neihbours.Add(pathNodes[i, j + 1]); // North
                if (j - 1 >= 0) pathNodes[i, j].Neihbours.Add(pathNodes[i, j - 1]); // South
                if (i + 1 <= tiles.GetUpperBound(0)) pathNodes[i, j].Neihbours.Add(pathNodes[i + 1, j]); // East
                if (i - 1 >= 0) pathNodes[i, j].Neihbours.Add(pathNodes[i - 1, j]); // West
            }
        }
    }
}