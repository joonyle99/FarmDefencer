using DG.Tweening;
using JoonyleGameDevKit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

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

    // start / end targetPos
    public Vector2Int StartCellPoint { get; private set; }
    public Vector3 StartWorldPoint => CellToWorld(StartCellPoint.ToVector3Int());
    public Vector2Int EndCellPoint { get; private set; }
    public Vector3 EndWorldPoint => CellToWorld(EndCellPoint.ToVector3Int());
    public Vector2Int DirectionToEnd => EndCellPoint - StartCellPoint;
    public int DirectionToEndX
    {
        get
        {
            if (DirectionToEnd.x >= 0)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
    }
    public int DirectionToEndY
    {
        get
        {
            if (DirectionToEnd.y > 0)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
    }
    private Vector2Int[] _possiblePoints;
    private Vector2Int[] _oppositePoints;

    [SerializeField] private LineRenderer _pathLineRenderer;
    [SerializeField] private GridCell _gridCellPrefab;
    private List<GridCell> _originGridPath;
    public List<GridCell> OriginGridPath => _originGridPath;

    private GridCell[,] _myGridMap;
    public GridCell[,] MyGridMap => _myGridMap;

    private int[,] _prevDistanceCost;

    private int[] _dx = new int[4] { -1, 0, 1, 0 };
    private int[] _dy = new int[4] { 0, 1, 0, -1 };

    public GridCell LastPlacedGridCell { get; set; }
    private bool _isFirstCalcPath = true;
    private Coroutine _drawPathCo;
    private LineRenderer _pathLineObject;
    private int _targetLayerIndex;
    private int _targetLayerMask;

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
        _prevDistanceCost = new int[_height, _width];

        _targetLayerIndex = LayerMask.NameToLayer("GridCell");
        _targetLayerMask = 1 << _targetLayerIndex;
    }
    private void Start()
    {
        GameStateManager.Instance.OnBuildState -= CreateGridMap;
        GameStateManager.Instance.OnBuildState += CreateGridMap;
        GameStateManager.Instance.OnBuildState -= FindPathOnStart;
        GameStateManager.Instance.OnBuildState += FindPathOnStart;
    }
    private void Update()
    {
        //distanceCostText.text = distanceCost.ToString($"D{2}");
        //isUsableText.text = isUsable.ToString();
        //isUsableText.color = (isUsable == true) ? Color.blue : Color.red;

        // gridCell touch input
        if (Touchscreen.current != null)
        {
            if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                Vector2 touchPos = Touchscreen.current.primaryTouch.position.ReadValue();
                HandleTouch(touchPos);
            }
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 clickPos = Mouse.current.position.ReadValue();
            HandleTouch(clickPos);
        }
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance is not null)
        {
            GameStateManager.Instance.OnBuildState -= CreateGridMap;
            GameStateManager.Instance.OnBuildState -= FindPathOnStart;
        }
    }

    private void OnDisable()
    {
        StopDrawPath();
    }

    public void CreateGridMap()
    {
        var departureResPath = $"Texture/GridMap/{MapManager.Instance.CurrentMap.MapCode}_departure";
        var departureSprite = Resources.Load<Sprite>(departureResPath);
        var arrivalResPath = $"Texture/GridMap/{MapManager.Instance.CurrentMap.MapCode}_arrival";
        var arrivalSprite = Resources.Load<Sprite>(arrivalResPath);

        for (int h = 0; h < _height; h++)
        {
            for (int w = 0; w < _width; w++)
            {
                var cellPos = new Vector2Int(w, h);
                var worldPos = _tilemap.GetCellCenterWorld(cellPos.ToVector3Int());

                _myGridMap[h, w] = Instantiate(_gridCellPrefab, worldPos, Quaternion.identity, transform);
                _myGridMap[h, w].name = $"{_myGridMap[h, w].name} [{w}, {h}]";
                _myGridMap[h, w].cellPosition = cellPos;
                _myGridMap[h, w].worldPosition = worldPos;
                _myGridMap[h, w].isUsable = true;
                _myGridMap[h, w].distanceCost = -1;
                _myGridMap[h, w].prevGridCell = null;

                //if (cellPos == EndCellPoint)
                if (cellPos == StartCellPoint || cellPos == EndCellPoint)
                {
                    _myGridMap[h, w].UnUsable();
                    _myGridMap[h, w].SetSprite(cellPos == StartCellPoint ? departureSprite : arrivalSprite);
                    Debug.Log($"start cell point: {StartCellPoint}, departureResPath: {departureResPath}");
                }
            }
        }
    }

    // shortest path finding by using bfs algorithm
    public void FindPathOnStart()
    {
        StartCoroutine(FindPathOnStartCo());
    }
    public IEnumerator FindPathOnStartCo()
    {
        var isFirst = _isFirstCalcPath;

        // 출발점 -> 도착점의 경로 계산을 딱 한 번만 한다
        if (_isFirstCalcPath == true)
        {
            _isFirstCalcPath = false;

            var originGridPath = CalculateOriginPath();
            if (originGridPath == null || originGridPath.Count < 2)
            {
                Debug.Log("origin grid path is invalid");
                LoadPrevDistanceCost();
                yield break;
            }

            _originGridPath = originGridPath;
        }

        if (_drawPathCo != null)
        {
            StopDrawPath();
        }

        _drawPathCo = StartCoroutine(DrawPathCo(_originGridPath, isFirst));
        yield return _drawPathCo;
    }
    public bool FindPathAll()
    {
        var newOriginGridPath = CalculateOriginPath();
        if (newOriginGridPath == null || newOriginGridPath.Count < 2)
        {
            Debug.Log("new origin grid path is invalid");
            LoadPrevDistanceCost();
            return false;
        }

        var result = DefenceContext.Current.WaveSystem.CalculateEachPaths();
        if (result == false)
        {
            Debug.Log("each grid paths are invalid");
            return false;
        }

        _originGridPath = newOriginGridPath;

        //StartCoroutine(DrawPathCo(_originGridPath));

        return true;
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

    // check
    public bool IsOutOfTileMap(Vector3 worldPos)
    {
        Vector3Int cellPos = _tilemap.WorldToCell(worldPos);
        return _tilemap.HasTile(cellPos) == false;
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

    // path finding
    public List<GridCell> CalculateOriginPath()
    {
        SavePrevDistanceCost();
        ResetGridMap();

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(StartCellPoint);
        _myGridMap[StartCellPoint.y, StartCellPoint.x].distanceCost = 0;

        while (queue.Count > 0)
        {
            Vector2Int nowPos = queue.Dequeue();

            // endCellPoint: excute trace
            if (nowPos == EndCellPoint)
            {
                var newGridPath = TracePath(_myGridMap[nowPos.y, nowPos.x]);
                return newGridPath;
            }

            // check 4 direction
            for (int i = 0; i < 4; i++)
            {
                Vector2Int nextPos = new Vector2Int(nowPos.x + _dx[i], nowPos.y + _dy[i]);

                if (nextPos.x < 0 || nextPos.x >= _width || nextPos.y < 0 || nextPos.y >= _height) continue;
                if (_myGridMap[nextPos.y, nextPos.x].distanceCost != -1) continue;
                if (_myGridMap[nextPos.y, nextPos.x].isUsable == false && nextPos != EndCellPoint) continue;

                queue.Enqueue(nextPos);

                _myGridMap[nextPos.y, nextPos.x].distanceCost = _myGridMap[nowPos.y, nowPos.x].distanceCost + 1;
                _myGridMap[nextPos.y, nextPos.x].prevGridCell = _myGridMap[nowPos.y, nowPos.x];
            }
        }

        return null;
    }
    public List<GridCell> CalculateEachPath(Vector2Int startCellPoint, Vector2Int endCellPoint)
    {
        if (_myGridMap[startCellPoint.y, startCellPoint.x].isUsable == false
            || startCellPoint == endCellPoint)
        {
            return null;
        }

        SavePrevDistanceCost();
        ResetGridMap();

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(startCellPoint);
        _myGridMap[startCellPoint.y, startCellPoint.x].distanceCost = 0;

        while (queue.Count > 0)
        {
            Vector2Int nowPos = queue.Dequeue();

            // endCellPoint: excute trace
            if (nowPos == endCellPoint)
            {
                var newGridPath = TracePath(_myGridMap[nowPos.y, nowPos.x]);
                return newGridPath;
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
        // Debug.Log($"Start Trace (end targetCellPos: {endCell.gameObject.name})", endCell.gameObject);

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

    // occupied
    public int CalculateAllOccupiedTowerCost()
    {
        var total = 0;

        // 그리드 셀을 순회
        foreach (var gridCell in _myGridMap)
        {
            // 점유되어 있는지 확인
            if (gridCell.occupiedTower != null)
            {
                var tower = gridCell.occupiedTower;
                var cost = tower.CurrentCost;
                total += cost;
            }
        }

        return total;
    }

    // etc
    public void SavePrevDistanceCost()
    {
        //Debug.Log("SavePrevDistanceCost");

        for (int h = 0; h < _height; h++)
        {
            for (int w = 0; w < _width; w++)
            {
                _prevDistanceCost[h, w] = _myGridMap[h, w].distanceCost;
            }
        }
    }
    public void LoadPrevDistanceCost()
    {
        //Debug.Log("LoadPrevDistanceCost");

        for (int h = 0; h < _height; h++)
        {
            for (int w = 0; w < _width; w++)
            {
                _myGridMap[h, w].distanceCost = _prevDistanceCost[h, w];
            }
        }
    }
    public void ResetGridMap()
    {
        //Debug.Log("ResetGridMap");

        // myGridMap 초기화
        for (int h = 0; h < _height; h++)
        {
            for (int w = 0; w < _width; w++)
            {
                _myGridMap[h, w].distanceCost = -1;
                _myGridMap[h, w].prevGridCell = null;
            }
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
    public bool IsTargetInCell(Vector3 targetPos, GridCell gridCell)
    {
        var targetCellPos = _tilemap.WorldToCell(targetPos).ToVector2Int();
        var thisCellPos = gridCell.cellPosition;
        return targetCellPos == thisCellPos;
    }
    public IEnumerator DrawPathCo(List<GridCell> gridPath, bool isFirst, Color? endColor = null)
    {
        var cellMoveTime = isFirst ? 0.15f : 0.15f;
        var delayTime = isFirst ? 0.1f : 0.1f;

        yield return new WaitForSeconds(delayTime);

        _pathLineObject = Instantiate(_pathLineRenderer, Vector3.zero, Quaternion.identity);
        _pathLineObject.positionCount = 0;

        if (endColor != null)
        {
            _pathLineObject.endColor = endColor.Value;
        }

        // gridPath를 복사해야 한다.
        // 전달 받은 gridPath는 참조형 변수이기 때문에 변경되면 Draw Path도 변경된다
        var copiedGridPath = new List<GridCell>(gridPath);

        for (int i = 0; i < copiedGridPath.Count; i++)
        {
            _pathLineObject.positionCount = i + 1;
            _pathLineObject.SetPosition(i, copiedGridPath[i].transform.position);

            var tf = copiedGridPath[i].transform;
            var originScale = tf.localScale;

            tf.DOScale(originScale * 1.5f, cellMoveTime * 0.5f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                tf.DOScale(originScale, cellMoveTime * 0.5f).SetEase(Ease.InQuad);
            });

            yield return new WaitForSeconds(cellMoveTime);
        }

        yield return new WaitForSeconds(0.3f); // delay for last point

        Destroy(_pathLineObject.gameObject);
        _pathLineObject = null;
    }

    private void StopDrawPath()
    {
        if (_drawPathCo != null)
        {
            StopCoroutine(_drawPathCo);
            _drawPathCo = null;

            if (_pathLineObject != null)
            {
                Destroy(_pathLineObject.gameObject);
                _pathLineObject = null;
            }
        }
    }

    // touch
    private void HandleTouch(Vector2 screenPos)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);
        RaycastHit2D hit = Physics2D.Raycast(worldPos2D, Vector2.zero, 0f, _targetLayerMask);

        if (hit.collider != null)
        {
            GridCell gridCell = hit.collider.GetComponent<GridCell>();
            if (gridCell != null)
            {
                gridCell.OnTouch();
            }
        }
    }
}
