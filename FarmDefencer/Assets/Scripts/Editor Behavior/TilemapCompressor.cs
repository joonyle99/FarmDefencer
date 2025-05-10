using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapCompressor : MonoBehaviour
{
    private Tilemap _tilemap;

    public void CompressTilemap()
    {
        Debug.Log("Compressing tilemap...");

        _tilemap = GetComponent<Tilemap>();
        _tilemap.CompressBounds();
    }
}
