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
    public int Height => _height;
    private int _width;
    public int Width => _width;

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

    private GridCell[,] _myGridMap;
    public GridCell[,] MyGridMap => _myGridMap;

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

        _myGridMap = new GridCell[_height, _width];
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

                _myGridMap[h, w] = Instantiate(_gridCellPrefab, worldPos, Quaternion.identity, transform);

                _myGridMap[h, w].cellPosition = cellPos;
                _myGridMap[h, w].worldPosition = worldPos;
                _myGridMap[h, w].isUsable = true;
                _myGridMap[h, w].distanceCost = -1;
                _myGridMap[h, w].prevGridCell = null;

                if (cellPos == StartCellPoint || cellPos == EndCellPoint)
                {
                    _myGridMap[h, w].UnUsable();
                }
            }
        }
    }

    // shortest path finding by using bfs algorithm
    public IEnumerator FindPathRoutine()
    {
        // TODO: 경로 다시 찾을 때마다 같은 타워 배치 상태임에도 최단 거리가 계속 변경되는 이유 찾기

        _gridPath = CalculatePath(StartCellPoint, EndCellPoint);

        // 현재 필드 위 몬스터들의 경로를 재계산한다
        if (DefenceContext.Current.WaveSystem.FieldCount > 0)
        {
            //Debug.Log(DefenceContext.Current.WaveSystem.FieldCount);
            DefenceContext.Current.WaveSystem.ReCalculatePath();
        }

        //yield return null;

        if (ConstantConfig.DEBUG == true)
        {
            DebugDistanceMap();
        }

        yield return DebugPingGridPathRoutine();
    }

    // convert
    public void CellToWorld(GridCell cell)
    {

    }
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

        return _myGridMap[h, w];
    }

    // debug
    private void DebugDistanceMap()
    {
        for (int h = 0; h < _height; h++)
        {
            for (int w = 0; w < _width; w++)
            {
                var number = _myGridMap[h, w].distanceCost;
                var stringNumber = number.ToString($"D{2}");

                _myGridMap[h, w].textMeshPro.text = stringNumber;
            }
        }
    }
    private IEnumerator DebugPingGridMapRoutine()
    {
        for (int h = 0; h < _height; h++)
        {
            for (int w = 0; w < _width; w++)
            {
                var originScale = _myGridMap[h, w].transform.localScale;

                _myGridMap[h, w].transform.localScale *= 1.7f;
                yield return new WaitForSeconds(0.15f);
                _myGridMap[h, w].transform.localScale = originScale;
            }
        }
    }
    private IEnumerator DebugPingGridPathRoutine()
    {
        var lineObject = Instantiate(debugLine, Vector3.zero, Quaternion.identity);
        lineObject.positionCount = 0;

        // _gridPath를 복사
        var copiedGridPath = new List<GridCell>(_gridPath);

        for (int i = 0; i < copiedGridPath.Count; i++)
        {
            //Debug.Log(copiedGridPath.Count);
            lineObject.positionCount = i + 1;
            lineObject.SetPosition(i, copiedGridPath[i].transform.position);

            var originScale = copiedGridPath[i].transform.localScale;

            copiedGridPath[i].transform.localScale = originScale * 1.5f;
            yield return new WaitForSeconds(0.1f);
            copiedGridPath[i].transform.localScale = originScale;
        }

        yield return new WaitForSeconds(_debugLineDuration);
        Destroy(lineObject.gameObject);
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

    // 
    public void ResetPath()
    {
        // _myGridMap 초기화
        for (int h = 0; h < _height; h++)
        {
            for (int w = 0; w < _width; w++)
            {
                _myGridMap[h, w].distanceCost = -1;
                _myGridMap[h, w].prevGridCell = null;
            }
        }
    }
    public List<GridCell> CalculatePath(Vector2Int startCellPoint, Vector2Int endCellPoint)
    {
        ResetPath();

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(startCellPoint);
        _myGridMap[startCellPoint.y, startCellPoint.x].distanceCost = 0;

        while (queue.Count > 0)
        {
            Vector2Int nowPos = queue.Dequeue();

            // endCellPoint: excute trace
            if (nowPos == endCellPoint)
            {
                var gridPath = TracePath(_myGridMap[nowPos.y, nowPos.x]);
                return gridPath;
            }

            // check 4 direction
            for (int i = 0; i < 4; i++)
            {
                Vector2Int nextPos = new Vector2Int(nowPos.x + _dx[i], nowPos.y + _dy[i]);

                if (nextPos.x < 0 || nextPos.x >= _width || nextPos.y < 0 || nextPos.y >= _height) continue;
                if (_myGridMap[nextPos.y, nextPos.x].distanceCost != -1) continue;
                if (_myGridMap[nextPos.y, nextPos.x].isUsable == false && nextPos != endCellPoint) continue;

                queue.Enqueue(nextPos);

                _myGridMap[nextPos.y, nextPos.x].distanceCost = _myGridMap[nowPos.y, nowPos.x].distanceCost + 1;
                _myGridMap[nextPos.y, nextPos.x].prevGridCell = _myGridMap[nowPos.y, nowPos.x];
            }
        }

        return null;
    }
    public List<GridCell> TracePath(GridCell endCell)
    {
        // Debug.Log($"Start Trace (end cell: {endCell.gameObject.name})", endCell.gameObject);

        var gridPath = new List<GridCell>();

        var wayPoint = endCell;
        gridPath.Add(wayPoint);

        while (true)
        {
            wayPoint = wayPoint.prevGridCell;
            if (wayPoint == null) break;
            gridPath.Add(wayPoint);
        }

        gridPath.Reverse();

        return gridPath;
    }
}
