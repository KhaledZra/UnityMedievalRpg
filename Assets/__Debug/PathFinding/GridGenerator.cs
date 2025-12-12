using UnityEngine;

// [ExecuteInEditMode]
public class GridGenerator : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Vector2Int size;
    [SerializeField] private float offset = 2;
    
    public static GridGenerator Instance { get; private set; }

    public GameObject[,] tiles;

    private void Awake()
    {
        Instance = this;
        
        tiles = new GameObject[size.x, size.y];
        for (int i = 0; i <= tiles.GetUpperBound(0); i++)
        {
            for (int j = 0; j <= tiles.GetUpperBound(1); j++)
            {
                Vector3 pos = new Vector3(i, 0, j) * offset;
                tiles[i, j] = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                tiles[i, j].GetComponent<Renderer>().material.color = Color.gray;
            }
        }
    }
    
    public GameObject this[Vector2Int v] => this[v.x, v.y];

    public GameObject this[int x, int y] => WithinBounds(x, y) ? tiles[x, y] : null;
    
    public bool WithinBounds(Vector2Int v)
    {
        return WithinBounds(v.x, v.y);
    }

    public bool WithinBounds(int x, int y)
    {
        return x >= 0 &&
               y >= 0 &&
               x < size.x &&
               y < size.y;
    }
}