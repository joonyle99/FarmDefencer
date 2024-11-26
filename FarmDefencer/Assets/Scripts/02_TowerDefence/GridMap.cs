using JoonyleGameDevKit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// GridMap의 각 Cell은 이동할 수 있는 땅인지의 정보를 가지며,
/// 타워 건설, 몬스터의 이동 경로 설정 등에 사용된다.
/// </summary>
public class GridMap : JoonyleGameDevKit.Singleton<GridMap> // temp singleton
{
    [Header("──────── GridMap ────────")]
    [Space]

    private Tilemap _tilemap;
    private Vector2Int _cellSize;
    private Vector2Int _mapSize;
    private Vector2Int _origin;
    private int _height;
    private int _width;

    public int UnitCellSize => _cellSize.x;

    [SerializeField] private TileBase _startTile;
    private Vector2Int _startCellPoint;
    public Vector2Int StartCellPoint => _startCellPoint;
    public Vector3 StartWorldPoint => CellToWorld(StartCellPoint.ToVector3Int());
    [SerializeField] private TileBase _endTile;
    private Vector2Int _endCellPoint;
    public Vector2Int EndCellPoint => _endCellPoint;
    public Vector3 EndWorldPoint => CellToWorld(EndCellPoint.ToVector3Int());

    [Space]

    [SerializeField] private GridCell _gridCell;
    [SerializeField] private List<GridCell> _gridPath;
    public List<GridCell> GridPath => _gridPath;

    private GridCell[,] _gridMap;

    private int[] _dx = new int[4] { -1, 0, 1, 0 };
    private int[] _dy = new int[4] { 0, 1, 0, -1 };

    public LineRenderer debugLine;

    protected override void Awake()
    {
        base.Awake();

        _tilemap = GetComponent<Tilemap>();
    }

    private void Start()
    {
        _cellSize = _tilemap.cellSize.ToVector2Int();
        _mapSize = _tilemap.size.ToVector2Int();
        _origin = _tilemap.origin.ToVector2Int();

        _height = _mapSize.y;
        _width = _mapSize.x;

        _startCellPoint = new Vector2Int(1, 1);
        _endCellPoint = new Vector2Int(_width - 2, _height - 2);

        _gridMap = new GridCell[_height, _width];

        _tilemap.SetTile(_startCellPoint.ToVector3Int(), _startTile);
        _tilemap.SetTile(_endCellPoint.ToVector3Int(), _endTile);

        for (int h = 0; h < _height; h++)
        {
            for (int w = 0; w < _width; w++)
            {
                var cellPos = new Vector2Int(w, h);
                var worldPos = _tilemap.GetCellCenterWorld(cellPos.ToVector3Int());

                _gridMap[h, w] = Instantiate(_gridCell, worldPos, Quaternion.identity, transform);

                _gridMap[h, w].cellPosition = cellPos;
                _gridMap[h, w].isUsable = true;
                _gridMap[h, w].distanceCost = -1;

                if (cellPos == _startCellPoint || cellPos == _endCellPoint)
                {
                    _gridMap[h, w].UnUsable();
                }
            }
        }
    }
    private void Update()
    {
        /*
        if (Input.GetMouseButtonDown(0) == true)
        {
            var mousePos = Input.mousePosition;
            var worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            var cellPos = _tilemap.WorldToCell(worldPos);

            Debug.Log($"x: {cellPos.x}, y: {cellPos.y}");
        }
        */

        if (Input.GetKeyDown(KeyCode.A))
        {
            CalculateDistance(_startCellPoint);

            DebugDistanceMap();

            StartCoroutine(PingGridPathCoroutine());
        }
    }

    // path
    private void CalculateDistance(Vector2Int startPoint)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(startPoint);

        _gridMap[startPoint.y, startPoint.x].distanceCost = 0;
        // _gridMap[startPoint.y, startPoint.x].prevGridCell = _gridMap[startPoint.y, startPoint.x];

        while (queue.Count > 0)
        {
            Vector2Int nowPos = queue.Dequeue();

            if (nowPos == _endCellPoint)
            {
                // 도착했으므로 어느 Cell에서 왔는지 경로를 추적한다
                TracePath(_gridMap[nowPos.y, nowPos.x]);
                return;
            }

            for (int i = 0; i < 4; i++)
            {
                Vector2Int nextPos = new Vector2Int(nowPos.x + _dx[i], nowPos.y + _dy[i]);

                if (nextPos.x < 0 || nextPos.x >= _width || nextPos.y < 0 || nextPos.y >= _height) continue;
                if (_gridMap[nextPos.y, nextPos.x].distanceCost != -1) continue;
                if (_gridMap[nextPos.y, nextPos.x].isUsable == false && nextPos != _endCellPoint) continue;

                queue.Enqueue(nextPos);

                _gridMap[nextPos.y, nextPos.x].distanceCost = _gridMap[nowPos.y, nowPos.x].distanceCost + 1;
                _gridMap[nextPos.y, nextPos.x].prevGridCell = _gridMap[nowPos.y, nowPos.x];
            }
        }
    }
    private void TracePath(GridCell endCell)
    {
        // Debug.Log($"Start Trace (end cell: {endCell.gameObject.name})", endCell.gameObject);

        _gridPath = new List<GridCell>();

        var wayPoint = endCell;
        _gridPath.Add(wayPoint);

        while (true)
        {
            wayPoint = wayPoint.prevGridCell;
            if (wayPoint == null) break;
            _gridPath.Add(wayPoint);
        }

        _gridPath.Reverse();
    }

    // convert
    public Vector3 CellToWorld(Vector3Int cellPos)
    {
        return _tilemap.GetCellCenterWorld(cellPos);
    }
    public Vector3Int WorldToCell(Vector3 worldPos)
    {
        return _tilemap.WorldToCell(worldPos);
    }

    // getter
    public GridCell GetCell(int w, int h)
    {
        if (IsValid(w, h) == false)
        {
            return null;
        }

        return _gridMap[h, w];
    }

    // editor
    public void CompressTilemap()
    {
        _tilemap = GetComponent<Tilemap>();
        _tilemap.CompressBounds();
    }

    // debug
    private void DebugDistanceMap()
    {
        for (int h = 0; h < _height; h++)
        {
            for (int w = 0; w < _width; w++)
            {
                var number = _gridMap[h, w].distanceCost;
                var stringNumber = number.ToString($"D{2}");

                _gridMap[h, w].textMeshPro.text = stringNumber;
            }
        }
    }
    private IEnumerator PingGridMapCoroutine()
    {
        for (int h = 0; h < _height; h++)
        {
            for (int w = 0; w < _width; w++)
            {
                var originScale = _gridMap[h, w].transform.localScale;

                _gridMap[h, w].transform.localScale *= 1.7f;
                yield return new WaitForSeconds(0.15f);
                _gridMap[h, w].transform.localScale = originScale;
            }
        }
    }
    private IEnumerator PingGridPathCoroutine()
    {
        var lineObject = Instantiate(debugLine, Vector3.zero, Quaternion.identity);
        lineObject.positionCount = 0;

        for (int i = 0; i < _gridPath.Count; i++)
        {
            lineObject.positionCount = i + 1;
            lineObject.SetPosition(i, _gridPath[i].transform.position);

            var originScale = _gridPath[i].transform.localScale;

            _gridPath[i].transform.localScale *= 1.7f;
            yield return new WaitForSeconds(0.15f);
            _gridPath[i].transform.localScale = originScale;
        }
    }

    public bool IsValid(int w, int h)
    {
        if (w < 0 || w >= _width || h < 0 || h >= _height)
        {
            Debug.LogWarning($"[{w}, {h}] is invalid index");
            return false;
        }

        return true;
    }
}
