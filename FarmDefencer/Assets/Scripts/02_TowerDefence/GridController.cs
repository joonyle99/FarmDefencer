using UnityEngine;
using UnityEngine.Tilemaps;

public class GridController : MonoBehaviour
{
    public GridMap GridMap;
    public Tilemap Tilemap;

    public TileBase TileBase1;
    public TileBase TileBase2;

    private void Awake()
    {
        GridMap = GetComponent<GridMap>();
        Tilemap = GetComponent<Tilemap>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GridMap.Initialize(10, 20);

        UpdateTile();
    }

    public void UpdateTile()
    {
        for (int h = 0; h < GridMap.Height; h++)
        {
            for (int w = 0; w < GridMap.Width; w++)
            {
                if (GridMap.Get(h, w))
                {

                    Tilemap.SetTile(new Vector3Int(w, h, 0), TileBase1);
                }
                else
                {
                    Tilemap.SetTile(new Vector3Int(w, h, 0), TileBase2);
                }
            }
        }
    }
}
