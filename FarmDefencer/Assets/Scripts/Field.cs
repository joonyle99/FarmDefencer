using UnityEngine;
using UnityEngine.Tilemaps;

public class Field : MonoBehaviour
{
    public Vector2Int FieldPosition;
    public Vector2Int FieldSize;
    public TileBase FlowedTile;
    public TileBase CropTile;

    private Tilemap _tilemap;

    private void Awake()
    {
        _tilemap = GetComponentInChildren<Tilemap>();
    }

    private void Start()
    {
        for (int y=0; y<FieldSize.y; y++)
        {
            for (int x=0; x<FieldSize.x; x++)
            {
                _tilemap.SetTile(new Vector3Int(x, y), FlowedTile);
            }
        }
    }
}
