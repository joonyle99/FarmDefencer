using UnityEngine;

public class GridMap : MonoBehaviour
{
    public int Height;  // 세로
    public int Width;   // 가로

    private bool[,] _grid;

    public void Initialize(int height, int width)
    {
        _grid = new bool[height, width];

        this.Height = height;
        this.Width = width;

        for (int h = 0; h < Height; h++)
        {
            for (int w = 0; w < Width; w++)
            {
                if ((h + w) % 2 == 0)
                {
                    _grid[h, w] = true;
                }
                else
                {
                    _grid[h, w] = false;
                }
            }
        }
    }

    public void Set(int h, int w, bool to)
    {
        if (IsValid(h, w) == true)
        {
            return;
        }

        _grid[h, w] = to;
    }
    public bool Get(int h, int w)
    {
        if (IsValid(h, w) == false)
        {
            return false;
        }

        return _grid[h, w];
    }

    public bool IsValid(int h, int w)
    {
        if (h < 0 || h >= Height || w < 0 || w >= Width)
        {
            Debug.LogWarning($"[{h}, {w}] is invalid index");
            return false;
        }

        return _grid[h, w];
    }
}
