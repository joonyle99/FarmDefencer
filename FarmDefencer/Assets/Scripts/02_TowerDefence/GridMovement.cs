using JoonyleGameDevKit;
using UnityEngine;

public class GridMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 2f;

    private GridCell _targetGridCell;
    private int _pathIndex = 0;
    private bool _isFirst = true;

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
        if (_isFirst == false)
        {
            Initialize();
        }
    }
    private void Update()
    {
        if (_targetGridCell == null)
        {
            return;
        }

        // check distance
        if (Vector2.Distance(_targetGridCell.transform.position, transform.position) < 0.1f)
        {
            _pathIndex++;

            if (_pathIndex >= GridMap.Instance.GridPath.Count)
            {
                if (_isFirst == true) _isFirst = false;

                _rigidbody.linearVelocity = Vector2.zero;
                _targetGridCell = null;

                return;
            }

            _targetGridCell = GridMap.Instance.GridPath[_pathIndex];
        }
    }
    private void FixedUpdate()
    {
        if (_targetGridCell == null)
        {
            return;
        }

        // Move to the target waypoint
        var dirVec = (_targetGridCell.transform.position - transform.position).normalized;
        _rigidbody.linearVelocity = dirVec * _moveSpeed;
    }

    public void Initialize()
    {
        _pathIndex = 0;
        _targetGridCell = GridMap.Instance.GridPath[_pathIndex];

        transform.position = _targetGridCell.transform.position;
    }
}
