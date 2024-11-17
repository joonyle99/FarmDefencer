using UnityEngine;

/// <summary>
/// 웨이포인트를 따라 몬스터를 이동시키는 컴포넌트
/// </summary>
public class PathMovement : MonoBehaviour
{
    [SerializeField] private Pathway _pathway;
    [SerializeField] private float _moveSpeed = 2f;

    private Transform _startWayPoint;
    private Transform _targetWayPoint;
    private int _pathIndex = 0;

    // references
    private Monster _monster;
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _monster = GetComponent<Monster>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }
    private void OnEnable()
    {
        if (_pathway == null)
        {
            return;
        }

        ResetWayPoint();
    }
    private void Update()
    {
        if (_targetWayPoint == null)
        {
            return;
        }

        // Arrived at the target waypoint
        if (Vector2.Distance(transform.position, _targetWayPoint.position) <= 0.1f)
        {
            _pathIndex++;

            // Arrived at the last waypoint
            if (_pathIndex >= _pathway.Path.Length)
            {
                ResetWayPoint();
                _monster.OriginFactory.ReturnProduct(_monster);

                return;
            }

            _targetWayPoint = _pathway.Path[_pathIndex];
        }
    }
    private void FixedUpdate()
    {
        if (_targetWayPoint == null)
        {
            return;
        }

        // Move to the target waypoint
        var dirVec = (_targetWayPoint.position - transform.position).normalized;
        _rigidbody.linearVelocity = dirVec * _moveSpeed;
    }

    /// <summary>
    /// 외부에서 이 컴포넌트를 초기화합니다.
    /// e.g) _pathway.Initialize(pathway);
    /// </summary>
    public void Initialize(Pathway pathway)
    {
        _pathway = pathway;

        if (_pathway == null)
        {
            throw new System.NullReferenceException("_pathway is null");
        }

        ResetWayPoint();
    }
    /// <summary>
    /// 
    /// </summary>
    private void ResetWayPoint()
    {
        // start waypoint
        _startWayPoint = _pathway.StartPoint;

        if (_startWayPoint == null)
        {
            throw new System.NullReferenceException("_startWayPoint is null");
        }

        transform.position = _startWayPoint.position;

        // target waypoint
        _pathIndex = 0;
        _targetWayPoint = _pathway.Path[_pathIndex];

        if (_targetWayPoint == null)
        {
            throw new System.NullReferenceException("_targetWayPoint is null");
        }
    }
}
