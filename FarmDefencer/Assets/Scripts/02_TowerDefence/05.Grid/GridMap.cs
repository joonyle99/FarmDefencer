using JoonyleGameDevKit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// GridMap의 각 Cell은 이동할 수 있는 땅인지의 정보를 가지며,
/// 타워 건설, 몬스터의 이동경로 설정 등에 사용된다.
/// </summary>
public class GridMap : MonoBehaviour
{
    [Header("──────── GridMap ────────")]
    [Space]

    private Tilemap _tilemap;

    private Vector2Int _cellSize;
    public int UnitCellSize => _cellSize.x;

    private Vector2Int _mapSize;
    private Vector2Int _origin;

    private int _height;
    private int _width;

    // start / end point
    public Vector2Int StartCellPoint { get; private set; }
    public Vector3 StartWorldPoint => CellToWorld(StartCellPoint.ToVector3Int());
    public Vector2Int EndCellPoint { get; private set; }
    public Vector3 EndWorldPoint => CellToWorld(EndCellPoint.ToVector3Int());
    private Vector2Int[] _possiblePoints;
    private Vector2Int[] _oppositePoints;

    // temp for debug
    public GameObject debugStartPointObject;
    public GameObject debugEndPointObject;
    public LineRenderer debugLine;
    private float _debugLineDuration = 0.3f;

    [Space]

    [SerializeField] private GridCell _gridCellPrefab;
    [SerializeField] private List<GridCell> _gridPath;
    public List<GridCell> GridPath => _gridPath;

    private GridCell[,] _gridMap;

    private int[] _dx = new int[4] { -1, 0, 1, 0 };
    private int[] _dy = new int[4] { 0, 1, 0, -1 };

    private void Awake()
    {
        _tilemap = GetComponent<Tilemap>();

        _cellSize = _tilemap.cellSize.ToVector2Int();
        _mapSize = _tilemap.size.ToVector2Int();
        _origin = _tilemap.origin.ToVector2Int();

        _height = _mapSize.y;
        _width = _mapSize.x;

        var minX = 1;
        var minY = 1;
        var maxX = _width - 2;
        var maxY = _height - 2;
        var halfX = (minX + maxX) / 2;
        var halfY = (minY + maxY) / 2;

        _possiblePoints = new Vector2Int[]
        {
            new(minX, minY),
            new(minX, maxY),
            new(maxX, minY),
            new(maxX, maxY),
            new(minX, halfY),
            new(halfX, minY),
            new(maxX, halfY),
            new(halfX, maxY)
        };

        //for (int i = 0; i < 8; i++)
        //{
        //    Painter.DebugDrawPlus(CellToWorld(_possiblePoints[i].ToVector3Int()));
        //}

        _oppositePoints = new Vector2Int[]
        {
            new(maxX, maxY),
            new(maxX, minY),
            new(minX, maxY),
            new(minX, minY),
            new(maxX, halfY),
            new(halfX, maxY),
            new(minX, halfY),
            new(halfX, minY)
        };

        int pointIndex = Random.Range(0, _possiblePoints.Length);
        StartCellPoint = _possiblePoints[pointIndex];
        EndCellPoint = _oppositePoints[pointIndex];

        _gridMap = new GridCell[_height, _width];
    }

    private void Start()
    {
        CreateGridMap();
    }

    private void CreateGridMap()
    {
        var startObj = Instantiate(debugStartPointObject, StartWorldPoint, Quaternion.identity);
        var endObj = Instantiate(debugEndPointObject, EndWorldPoint, Quaternion.identity);

        for (int h = 0; h < _height; h++)
        {
            for (int w = 0; w < _width; w++)
            {
                var cellPos = new Vector2Int(w, h);
                var worldPos = _tilemap.GetCellCenterWorld(cellPos.ToVector3Int());

                _gridMap[h, w] = Instantiate(_gridCellPrefab, worldPos, Quaternion.identity, transform);

                _gridMap[h, w].cellPosition = cellPos;
                _gridMap[h, w].isUsable = true;
                _gridMap[h, w].distanceCost = -1;

                if (cellPos == StartCellPoint || cellPos == EndCellPoint)
                {
                    _gridMap[h, w].UnUsable();
                }
            }
        }
    }

    public IEnumerator FindPathRoutine()
    {
        CalculateDistance(StartCellPoint);

        if (ConstantConfig.DEBUG == true)
        {
            DebugDistanceMap();
        }

        yield return DebugPingGridPathRoutine();
    }

    // path
    private void CalculateDistance(Vector2Int startPoint)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(startPoint);

        _gridMap[startPoint.y, startPoint.x].distanceCost = 0;

        while (queue.Count > 0)
        {
            Vector2Int nowPos = queue.Dequeue();

            // endPoint: excute trace
            if (nowPos == EndCellPoint)
            {
                TracePath(_gridMap[nowPos.y, nowPos.x]);
                return;
            }

            // check 4 direction
            for (int i = 0; i < 4; i++)
            {
                Vector2Int nextPos = new Vector2Int(nowPos.x + _dx[i], nowPos.y + _dy[i]);

                if (nextPos.x < 0 || nextPos.x >= _width || nextPos.y < 0 || nextPos.y >= _height) continue;
                if (_gridMap[nextPos.y, nextPos.x].distanceCost != -1) continue;
                if (_gridMap[nextPos.y, nextPos.x].isUsable == false && nextPos != EndCellPoint) continue;

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
    private IEnumerator DebugPingGridMapRoutine()
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
    private IEnumerator DebugPingGridPathRoutine()
    {
        var lineObject = Instantiate(debugLine, Vector3.zero, Quaternion.identity);
        lineObject.positionCount = 0;

        for (int i = 0; i < _gridPath.Count; i++)
        {
            lineObject.positionCount = i + 1;
            lineObject.SetPosition(i, _gridPath[i].transform.position);

            var originScale = _gridPath[i].transform.localScale;

            _gridPath[i].transform.localScale *= 1.5f;
            yield return new WaitForSeconds(0.1f);
            _gridPath[i].transform.localScale = originScale;
        }

        // 디버그 모드가 켜져있지 않다면 라인 렌더러를 제거합니다.
        if (ConstantConfig.DEBUG == false)
        {
            yield return new WaitForSeconds(_debugLineDuration);
            Destroy(lineObject.gameObject);
        }
    }

    public bool IsValid(int w, int h)
    {
        if (w < 0 || w >= _width || h < 0 || h >= _height)
        {
            // Debug.LogWarning($"[{w}, {h}] is invalid pointIndex");
            return false;
        }

        return true;
    }
}
