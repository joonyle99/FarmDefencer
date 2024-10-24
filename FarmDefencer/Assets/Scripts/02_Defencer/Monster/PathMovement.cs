using UnityEngine;

public class PathMovement : MonoBehaviour
{
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
        _targetWayPoint = PathManager.Instance.Path[_pathIndex];

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

            if (_pathIndex == PathManager.Instance.Path.Length)
            {
                Destroy(gameObject);
                return;
            }

            _targetWayPoint = PathManager.Instance.Path[_pathIndex];
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
