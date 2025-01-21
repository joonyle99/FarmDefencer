using JoonyleGameDevKit;
using UnityEngine;

public class GridMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 1f;

    // public Vector2Int currentLookDir;

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

            _targetGridCell = DefenceContext.Current.GridMap.GridPath[_pathIndex];

            /*
            // rotate
            var targetLookDir = (_targetGridCell.transform.position - transform.position).normalized.ToVector2Int();
            if (targetLookDir != currentLookDir)
            {
                var targetAngle = Vector2.SignedAngle(currentLookDir, targetLookDir);
                transform.rotation = Quaternion.Euler(0f, 0f, transform.rotation.eulerAngles.z + targetAngle);

                currentLookDir = targetLookDir;
            }
            */
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
        _pathIndex = 0;
        _targetGridCell = DefenceContext.Current.GridMap.GridPath[_pathIndex];

        // monster 이미지의 앞쪽은 right 방향이다
        // currentLookDir = transform.right.ToVector2Int();

        transform.position = _targetGridCell.transform.position;
        // _rigidbody.MovePosition(_targetGridCell.transform.position);

        _monster.SpineController.SetAnimation(_monster.WalkAnimationName, true);

        /*
        if (DefenceContext.Current.GridMap.GridPath.Count > 2)
        {
            var firstTarget = DefenceContext.Current.GridMap.GridPath[0];
            var secondTarget = DefenceContext.Current.GridMap.GridPath[1];

            var targetLookDir = (secondTarget.transform.position - firstTarget.transform.position).normalized.ToVector2Int();
            if (targetLookDir != currentLookDir)
            {
                var targetAngle = Vector2.SignedAngle(currentLookDir, targetLookDir);
                transform.rotation = Quaternion.Euler(0f, 0f, transform.rotation.eulerAngles.z + targetAngle);

                currentLookDir = targetLookDir;
            }
        }
        */
    }
}
