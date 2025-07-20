using UnityEngine;

public static class ConstantConfig
{
    // 모드
    public const bool DEBUG = true;

    // 색상
    public static readonly Color WHITE = new Color(1f, 1f, 1f, 1f); // original
    public static readonly Color WHITE_GHOST = new Color(1f, 1f, 1f, 0.7f);

    public static readonly Color RED = new Color(0.7f, 0f, 0f, 1f);
    public static readonly Color RED_GHOST = new Color(1f, 0f, 0f, 0.7f);
    public static readonly Color RED_RANGE = new Color(1f, 0f, 0f, 0.8f);

    public static readonly Color GREEN = new Color(0f, 0.7f, 0f, 1f);
    public static readonly Color GREEN_GHOST = new Color(0f, 1f, 0f, 0.7f);
    public static readonly Color GREEN_RANGE = new Color(0f, 1f, 0f, 0.8f);

    public static readonly Color BLUE = new Color(0f, 0f, 0.7f, 1f);
    public static readonly Color BLUE_GHOST = new Color(0f, 0f, 1f, 0.7f);
    public static readonly Color BLUE_RANGE = new Color(0f, 0f, 1f, 0.8f);

    public static readonly Color YELLOW = new Color(0.7f, 0.7f, 0f, 1f);
    public static readonly Color YELLOW_GHOST = new Color(1f, 1f, 0f, 0.7f);
    public static readonly Color YELLOW_RANGE = new Color(1f, 1f, 0f, 0.8f);

    public static readonly Color PINK = new Color(0.7f, 0f, 0.7f, 1f);
    public static readonly Color PINK_GHOST = new Color(1f, 0f, 1f, 0.7f);
    public static readonly Color PINK_RANGE = new Color(1f, 0f, 1f, 0.8f);

    public static readonly Color CYAN = new Color(0f, 1f, 1f, 1f);
    public static readonly Color CYAN_GHOST = new Color(0f, 1f, 1f, 0.7f);
    public static readonly Color CYAN_RANGE = new Color(0f, 1f, 1f, 0.8f);

    // 방향
    public static readonly (int x, int y)[] DIRECTIONS = new (int x, int y)[]
    {
        (-1, 0),   // left
        (1, 0),    // right
        (0, -1),   // down
        (0, 1)     // up
    };
    public static readonly (int x, int y)[] DirectionsWithOrigin = new (int x, int y)[]
    {
        (0, 0),    // origin
        (-1, 0),   // left
        (1, 0),    // right
        (0, -1),   // down
        (0, 1)     // up
    };
}
