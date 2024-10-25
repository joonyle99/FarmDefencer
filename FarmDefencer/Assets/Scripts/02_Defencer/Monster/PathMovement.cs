using UnityEngine;

/// <summary>
/// PathSupervisor에 의해 정의된 웨이포인트를 따라 게임오브젝트를 이동시키는 컴포넌트
/// </summary>
public class PathMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 2f;

    private Transform _targetWayPoint;
    private int _pathIndex = 0;

    private Monster _monster;           // TEMP
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _monster = GetComponent<Monster>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        _targetWayPoint = PathSupervisor.Instance.Path[_pathIndex];

        if (_targetWayPoint == null)
        {
            Debug.LogWarning("TargetWayPoint is null, you should set \'_targetWayPoint\' variable");
            return;
        }

    }
    private void Update()
    {
        if (_targetWayPoint == null)
            return;

        if (Vector2.Distance(transform.position, _targetWayPoint.position) <= 0.1f)
        {
            _pathIndex++;

            if (_pathIndex >= PathSupervisor.Instance.Path.Length)
            {
                // 해당 객체의 사망
                Destroy(gameObject);
                // _monster.Die();
                return;
            }

            _targetWayPoint = PathSupervisor.Instance.Path[_pathIndex];
        }
    }
    private void FixedUpdate()
    {
        if (_targetWayPoint == null)
            return;

        /*
        if ((Vector2.Distance(transform.position, _targetWayPoint.position) <= 0.1f))
            return;
        */

        var dirVec = (_targetWayPoint.position - transform.position).normalized;
        _rigidbody.linearVelocity = dirVec * _moveSpeed;
    }
}
