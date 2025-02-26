using JoonyleGameDevKit;
using System.Collections.Generic;
using UnityEngine;
using VInspector.Libs;

public class GridMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 1f;

    private GridCell _currGridCell;
    private GridCell _nextGridCell;
    private int _pathIndex = 0;
    private bool _isFirst = true;

    // references
    private Monster _monster;
    private Rigidbody2D _rigidbody;

    private List<GridCell> _eachGridPath;

#if UNITY_EDITOR
    public float eTime;
    public float distance;
#endif

    private void Awake()
    {
        _monster = GetComponent<Monster>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }
    private void OnEnable()
    {
        if (_isFirst == false)
        {
            Initialize();
        }
    }
    private void Update()
    {
        if (_monster.IsStun == true || _monster.IsDead == true)
        {
            return;
        }

        if (_nextGridCell == null)
        {
            return;
        }

        // TODO: Update에서 방향 설정을 매번 해주는 게 맞는 지는 모르겠다..

        float horizontalVelocity = _rigidbody.linearVelocityX;
        float threshold = 0.1f; // Define a threshold for near-zero horizontal movement

        // 1. flip sprite based on linear velocity
        if (Mathf.Abs(horizontalVelocity) > threshold)
        {
            if (_rigidbody.linearVelocityX < 0 && transform.localScale.x > 0)
            {
                var scale = transform.localScale;
                scale.x *= -1; // x축 스케일 반전
                transform.localScale = scale;
            }
            else if (_rigidbody.linearVelocityX > 0 && transform.localScale.x < 0)
            {
                var scale = transform.localScale;
                scale.x *= -1; // x축 스케일 반전
                transform.localScale = scale;
            }
            else
            {
                // nothing
            }
        }
        // 2. flip sprite based on target direction
        else
        {
            GridCell endGridCell = _eachGridPath[^1];
            Vector3 targetPosition = endGridCell.worldPosition;
            Vector3 direction = targetPosition - transform.position;

            if (direction.x < 0 && transform.localScale.x > 0)
            {
                var scale = transform.localScale;
                scale.x *= -1;
                transform.localScale = scale;
            }
            else if (direction.x > 0 && transform.localScale.x < 0)
            {
                var scale = transform.localScale;
                scale.x *= -1;
                transform.localScale = scale;
            }
            else
            {
                // nothing
            }
        }

        // check distance
        if (Vector2.Distance(transform.position, _nextGridCell.worldPosition) <= 0.05f)
        {
            _pathIndex++;

            if (_pathIndex >= _eachGridPath.Count)
            {
                if (_isFirst == true)
                {
                    _isFirst = false;
                }

                _rigidbody.linearVelocity = Vector2.zero;
                _nextGridCell = null;

                // arrived
                _monster.Survive();

                return;
            }

            _currGridCell = _nextGridCell;
            _nextGridCell = _eachGridPath[_pathIndex];
        }
    }
    private void FixedUpdate()
    {
        if (_monster.IsStun == true || _monster.IsDead == true)
        {
            _rigidbody.linearVelocity = Vector2.zero;
            return;
        }

        if (_nextGridCell == null)
        {
            return;
        }

        // move
        var dirVec = (_nextGridCell.worldPosition - transform.position).normalized;
        _rigidbody.linearVelocity = dirVec * DefenceContext.Current.GridMap.UnitCellSize * _moveSpeed;

#if UNITY_EDITOR
        // 몬스터 1초당 이동거리 계산
        eTime += Time.deltaTime;
        distance += _rigidbody.linearVelocity.magnitude * Time.deltaTime;

        if (eTime >= 1f)
        {
            eTime = 0f;
            distance = 0f;
        }
#endif
    }

    public void Initialize()
    {
        bool result = UseOriginGridPath();
        if (result == false)
        {
            Debug.LogError("failed to calculate");
            return;
        }

        // 걷기 애니메이션
        _monster.SpineController.SetAnimation(_monster.WalkAnimationName, true);

        // 위치 초기화
        transform.position = _currGridCell.transform.position;
    }
    public bool UseOriginGridPath()
    {
        var originGridPath = DefenceContext.Current.GridMap.OriginGridPath;
        if (originGridPath == null || originGridPath.Count < 2)
        {
            Debug.LogError("origin grid path is invalid");
            return false;
        }

        int tempPathIndex = 0;
        GridCell tempCurrGridCell = null;
        GridCell tempNextGridCell = null;

        tempCurrGridCell = originGridPath[tempPathIndex++]; // index = 0
        if (tempCurrGridCell == null)
        {
            Debug.Log("curr grid cell is null");
            return false;
        }

        tempNextGridCell = originGridPath[tempPathIndex]; // index = 1
        if (tempNextGridCell == null)
        {
            Debug.Log("next grid cell is null");
            return false;
        }

        _pathIndex = tempPathIndex;
        _currGridCell = tempCurrGridCell;
        _nextGridCell = tempNextGridCell;
        _eachGridPath = originGridPath;

        return true;
    }
    public bool CalcEachGridPath()
    {
        var gridMap = DefenceContext.Current.GridMap;

        // 다음 목적지에는 타워를 설치할 수 없다
        if (gridMap.ClickedCell == _nextGridCell)
        {
            Debug.Log("cannot be installed in the next grid cell.");
            return false;
        }

        var eachGridPath = gridMap.CalculateEachPath(_nextGridCell.cellPosition, gridMap.EndCellPoint);
        if (eachGridPath == null || eachGridPath.Count < 2)
        {
            Debug.Log("each grid path is invalid");
            //DefenceContext.Current.GridMap.LoadPrevDistanceCost();
            return false;
        }

        // 굳이 경로를 바꿀 필요가 없는 경우
        // _eachGridPath에서 남아있는 경로의 개수를 구해야지,,
        // if (eachGridPath.Count >= _eachGridPath.Count - _pathIndex)
        // {
        //     //Debug.Log($"eachGridPath.Count: {eachGridPath.Count} / _eachGridPath.Count: {_eachGridPath.Count}");
        //     //StartCoroutine(gridMap.DrawPathRoutine(eachGridPath, Color.red));
        //     //StartCoroutine(gridMap.DrawPathRoutine(_eachGridPath, Color.blue));
           
        //     Debug.Log("no need to change path");
        //     return true;
        // }

        GridCell tempCurrGridCell = null;
        GridCell tempNextGridCell = null;

        // 방향 전환이 필요한 경우
        var newNextGridCell = eachGridPath[0];
        if (newNextGridCell != _nextGridCell)
        {
            tempCurrGridCell = _currGridCell;
            tempNextGridCell = newNextGridCell;
        }
        // 방향 전환이 필요하지 않은 경우
        else
        {
            tempCurrGridCell = _currGridCell;
            if (tempCurrGridCell == null)
            {
                Debug.Log("curr grid cell is null");
                return false;
            }

            tempNextGridCell = newNextGridCell;
            if (tempNextGridCell == null)
            {
                Debug.Log("next grid cell is null");
                return false;
            }
        }

        _pathIndex = 0;
        _currGridCell = tempCurrGridCell;
        _nextGridCell = tempNextGridCell;
        _eachGridPath = eachGridPath;

        //StartCoroutine(gridMap.DrawPathRoutine(_eachGridPath, Color.blue));

        return true;
    }

    public bool ContainCellInPath(List<GridCell> gridPath, GridCell gridCell)
    {
        if (gridPath == null || gridPath.Count == 0)
        {
            return false;
        }

        return _eachGridPath.Contains(gridCell);
    }

    private void OnDrawGizmos()
    {
        if (_currGridCell != null)
        {
            //var dir = _currGridCell.worldPosition - transform.position;
            //JoonyleGameDevKit.Painter.GizmosDrawArrow(transform.position, dir, bodyColor: Color.blue, headColor: Color.blue);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, _currGridCell.worldPosition);
        }

        if (_nextGridCell != null)
        {
            //var dir = _nextGridCell.worldPosition - transform.position;
            //JoonyleGameDevKit.Painter.GizmosDrawArrow(transform.position, dir, bodyColor: Color.red, headColor: Color.red);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _nextGridCell.worldPosition);
        }
    }
}
