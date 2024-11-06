using UnityEngine;

/// <summary>
/// ��������Ʈ�� ���� ���͸� �̵���Ű�� ������Ʈ
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

        // TODO: �ܺο��� Initialize�� ȣ���ϵ��� �����ϰ� ResetWayPoint�� �� �ȿ��� �Ⱦ��ϵ���
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
    /// �ܺο��� �� ������Ʈ�� �ʱ�ȭ�մϴ�.
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
