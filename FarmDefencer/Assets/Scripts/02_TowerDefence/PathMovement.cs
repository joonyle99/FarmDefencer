using UnityEngine;

/// <summary>
/// 웨이포인트를 따라 몬스터를 이동시키는 컴포넌트
/// </summary>
public class PathMovement : MonoBehaviour
{
    [Header("Attributes")]
    [Space]

    [SerializeField] private float _moveSpeed = 2f;

    private Transform _targetWayPoint;
    private int _pathIndex = 0;

    private Monster _monster;
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _monster = GetComponent<Monster>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        var startPoint = PathSupervisor.Instance.StartPoint;

        if (startPoint == null)
        {
            Debug.LogWarning("TargetWayPoint is null");
            return;
        }

        transform.position = startPoint.position;

        ResetWayPoint();
    }
    private void Update()
    {
        if (_targetWayPoint == null)
            return;

        // Arrived at the target waypoint
        if (Vector2.Distance(transform.position, _targetWayPoint.position) <= 0.1f)
        {
            _pathIndex++;

            // Arrived at the last waypoint
            if (_pathIndex >= PathSupervisor.Instance.Path.Length)
            {
                ResetWayPoint();
                _monster.Kill();

                return;
            }

            _targetWayPoint = PathSupervisor.Instance.Path[_pathIndex];
        }
    }
    private void FixedUpdate()
    {
        if (_targetWayPoint == null)
            return;

        // Move to the target waypoint
        var dirVec = (_targetWayPoint.position - transform.position).normalized;
        _rigidbody.linearVelocity = dirVec * _moveSpeed;
    }

    private void ResetWayPoint()
    {
        _pathIndex = 0;
        _targetWayPoint = PathSupervisor.Instance.Path[_pathIndex];

        if (_targetWayPoint == null)
        {
            Debug.LogWarning("TargetWayPoint is null, you should set \'_targetWayPoint\' variable");
            return;
        }
    }
}
