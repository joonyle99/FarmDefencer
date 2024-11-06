using UnityEngine;

/// <summary>
/// 웨이포인트를 따라 몬스터를 이동시키는 컴포넌트
/// </summary>
public class PathMovement : MonoBehaviour
{
    [SerializeField] private Pathway _pathway;
    [SerializeField] private float _moveSpeed = 2f;

    private Transform _targetWayPoint;
    private int _pathIndex = 0;

    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        if (_pathway == null)
        {
            // Debug.LogWarning("_pathway is null");
            // return;
            throw new System.Exception("_pathway is null");
        }

        var startPoint = _pathway.StartPoint;

        if (startPoint == null)
        {
            // Debug.LogWarning("_targetWayPoint is null");
            // return;
            throw new System.Exception("_targetWayPoint is null");
        }

        transform.position = startPoint.position;

        // TODO: 외부에서 Initialize를 호출하도록 수정하고 ResetWayPoint는 그 안에서 싫애하도록
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
                Destroy(gameObject);

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

        ResetWayPoint();
    }
    private void ResetWayPoint()
    {
        _pathIndex = 0;
        _targetWayPoint = _pathway.Path[_pathIndex];

        if (_targetWayPoint == null)
        {
            Debug.LogWarning("TargetWayPoint is null, you should set \'_targetWayPoint\' variable");
            return;
        }
    }
}
