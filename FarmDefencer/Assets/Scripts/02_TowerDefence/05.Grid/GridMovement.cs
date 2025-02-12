using JoonyleGameDevKit;
using System.Collections.Generic;
using UnityEngine;
using VInspector.Libs;

public class GridMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 1f;

    private GridCell _currentGridCell;
    private GridCell _targetGridCell;
    private int _currentPathIndex = 0;
    private bool _isFirst = true;

    private List<GridCell> _gridPath;

    // references
    private Monster _monster;
    private Rigidbody2D _rigidbody;

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

        if (_targetGridCell == null)
        {
            return;
        }

        // flip sprite
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

        // check distance
        if (Vector2.Distance(_targetGridCell.transform.position, transform.position) < 0.1f)
        {
            _currentPathIndex++;

            if (_currentPathIndex >= _gridPath.Count)
            {
                if (_isFirst == true)
                {
                    _isFirst = false;
                }

                _rigidbody.linearVelocity = Vector2.zero;
                _targetGridCell = null;

                // arrived
                _monster.Survive();

                return;
            }

            _currentGridCell = _targetGridCell;
            _targetGridCell = _gridPath[_currentPathIndex];
        }
    }
    private void FixedUpdate()
    {
        if (_monster.IsStun == true || _monster.IsDead == true)
        {
            _rigidbody.linearVelocity = Vector2.zero;
            return;
        }

        if (_targetGridCell == null)
        {
            return;
        }

        // move
        var dirVec = (_targetGridCell.transform.position - transform.position).normalized;
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
        _currentPathIndex = 0;

        var gridPath = DefenceContext.Current.GridMap.GridPath;
        if (gridPath == null || gridPath.Count < 2)
        {
            Debug.LogError("grid path is invalid");
            return;
        }

        _currentGridCell = gridPath[_currentPathIndex];
        if (_currentGridCell == null)
        {
            Debug.LogError("current grid cell is null");
            return;
        }

        _targetGridCell = gridPath[_currentPathIndex + 1];
        if (_targetGridCell == null)
        {
            Debug.LogError("target grid cell is null");
            return;
        }

        _gridPath = gridPath;

        // 걷기 애니메이션
        _monster.SpineController.SetAnimation(_monster.WalkAnimationName, true);

        // 위치 초기화
        transform.position = _currentGridCell.transform.position;
    }
    public void ReCalculatePath()
    {
        // TODO: _currentGridCell과 _targetGridCell을 이렇게 초기화 하면 출발지로 잠시 돌아갔다가 다시 목적지로 가는데,,
        // 어떻게 하면 최대한 자연스럽게 움직이게 할 수 있을까?

        // 다음 목적지를 기준으로 현재 위치 (Position) 에서 가깝다면 그대로 두고,
        // 현재 Cell 에서 가깝다면 현재 Cell로 이동했다가 가도록 한다.

        _currentPathIndex = 0;

        var gridMap = DefenceContext.Current.GridMap;
        var gridPath = gridMap.CalculatePath(_currentGridCell.cellPosition, gridMap.EndCellPoint);

        if (gridPath == null || gridPath.Count < 2)
        {
            Debug.LogError("grid path is invalid");
            return;
        }

        _currentGridCell = gridPath[_currentPathIndex];
        if (_currentGridCell == null)
        {
            Debug.LogError("current grid cell is null");
            return;
        }

        _targetGridCell = gridPath[_currentPathIndex + 1];
        if (_targetGridCell == null)
        {
            Debug.LogError("target grid cell is null");
            return;
        }

        // 가까운 곳으로 간다
        var currGridCellPosToTarget = Vector2.Distance(_currentGridCell.worldPosition, _targetGridCell.worldPosition);
        var currObjectPosToTarget = Vector2.Distance(transform.position, _targetGridCell.worldPosition);
        if (currGridCellPosToTarget < currObjectPosToTarget)
        {
            _targetGridCell = _currentGridCell;
        }

        _gridPath = gridPath;
    }
}
