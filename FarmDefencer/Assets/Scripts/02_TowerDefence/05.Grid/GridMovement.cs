using JoonyleGameDevKit;
using UnityEngine;
using UnityEngine.UIElements;

public class GridMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 1f;

    private GridCell _currentGridCell;
    private GridCell _targetGridCell;
    private int _pathIndex = 0;
    private bool _isFirst = true;

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
            _pathIndex++;

            if (_pathIndex >= DefenceContext.Current.GridMap.GridPath.Count)
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
            _targetGridCell = DefenceContext.Current.GridMap.GridPath[_pathIndex];
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
        // 이것도 무조건 0번째 _pathIndex면 안되고, 예를 들어 17개의 way point 중 5번째 way point에 현재 있다면 _targetGridCell를 6번째 way point로 설정해야함
        // 만약 몬스터가 grid path 위를 이동 중이었다면, 중간 지점에서 이동할 수 있도록 해야하는데,,

        // DefenceContext.Current.GridMap.GridPath 중 현재 위치에서 가장 가까운 grid cell을 찾아서 그 cell을 _currentGridCell로 설정

        var gridPath = DefenceContext.Current.GridMap.GridPath;
        //var closestDistance = float.MaxValue;
        //var closestIndex = -1;
        //for (int i = gridPath.Count - 1; i > 0; i--)
        //{
        //    var gridCell = gridPath[i];
        //    var distance = Vector2.Distance(gridCell.transform.position, transform.position);
        //    if (distance <= closestDistance)
        //    {
        //        closestDistance = distance;
        //        closestIndex = i;
        //    }
        //}

        //if (closestIndex == -1)
        //{
        //    Debug.LogError("closest is invalid");
        //    return;
        //}

        //_pathIndex = closestIndex;
        //_currentGridCell = gridPath[_pathIndex];

        _pathIndex = 0;
        _targetGridCell = gridPath[_pathIndex];

        _monster.SpineController.SetAnimation(_monster.WalkAnimationName, true);
        transform.position = _targetGridCell.transform.position;
    }
}
