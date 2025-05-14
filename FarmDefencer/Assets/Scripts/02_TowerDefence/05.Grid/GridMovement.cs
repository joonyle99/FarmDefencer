using JoonyleGameDevKit;
using System.Collections.Generic;
using UnityEngine;

public class GridMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 1f;
    /// <summary>
    /// 몬스터가 이동하는 속도입니다.
    /// 기본 값은 1입니다. (1초에 1칸 이동)
    /// 값이 2인 경우 -> 1초에 2칸 이동합니다.
    /// 값이 0.5인 경우 -> 1초에 0.5칸 이동합니다.
    /// </summary>
    public float MoveSpeed
    {
        get => _moveSpeed;
        set
        {
            _moveSpeed = value;

            // 몬스터의 움직임 애니메이션 속도 조절
            if (_monster != null)
            {
               _monster.SpineController.SpineAnimationState.TimeScale = _moveSpeed;
            }
        }
    }
    [SerializeField] private float _arrivalThreshold = 0.05f;

    private GridCell _currGridCell;
    private GridCell _nextGridCell;
    private int _pathIndex = 0;
    private bool _isFirst = true;

    // references
    private Monster _monster;
    private Rigidbody2D _rigidbody;

    private List<GridCell> _eachGridPath;

    public GridCell CurrGridCell => _currGridCell;
    public GridCell NextGridCell => _nextGridCell;

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
        // for object pooling
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
        var sqrtThreshold = _arrivalThreshold * _arrivalThreshold;
        if ((_nextGridCell.worldPosition - transform.position).sqrMagnitude <= sqrtThreshold)
        {
            _pathIndex++;

            _currGridCell.monstersInCell.Remove(_monster);

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

            _nextGridCell.monstersInCell.Add(_monster);

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

        // 몬스터 리스트에 추가
        _currGridCell.monstersInCell.Add(_monster);
    }
    /// <summary> 원래 경로(출발 지점에서 도착 지점의 경로)를 사용합니다. </summary>
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
    /// <summary> 몬스터의 현재 지점에서 도착 지점까지의 경로를 사용합니다. </summary>
    public bool CalcEachGridPath()
    {
        var gridMap = DefenceContext.Current.GridMap;

        // 다음 목적지에는 타워를 설치할 수 없다
        if (gridMap.LastGridCell == _nextGridCell)
        {
            Debug.Log("cannot be installed in the next grid cell.");
            return false;
        }

        // _nextGridCell을 기준으로 EndCellPoint까지의 경로를 계산합니다
        var eachGridPath = gridMap.CalculateEachPath(_nextGridCell.cellPosition, gridMap.EndCellPoint);
        if (eachGridPath == null || eachGridPath.Count < 2)
        {
            Debug.Log("each grid path is invalid");
            return false;
        }

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

        return true;
    }

    private void OnDrawGizmos()
    {
        if (_currGridCell != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, _currGridCell.worldPosition);
        }

        if (_nextGridCell != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _nextGridCell.worldPosition);
        }
    }
}
