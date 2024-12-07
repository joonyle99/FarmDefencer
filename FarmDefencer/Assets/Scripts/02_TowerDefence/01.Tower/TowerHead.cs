using UnityEngine;

public class TowerHead : MonoBehaviour
{
    [SerializeField] private Transform _muzzle;
    public Transform Muzzle => _muzzle;

    private float _startAngle;

    private void Start()
    {
        _startAngle = transform.rotation.eulerAngles.z;
    }

    public void LookAt(Vector3 targetPosition)
    {
        var dirVec = (targetPosition - transform.position).normalized;
        var targetAngle = Mathf.Atan2(dirVec.y, dirVec.x) * Mathf.Rad2Deg;
        var targetRotation = Quaternion.Euler(0f, 0f, targetAngle + _startAngle);
        transform.rotation = targetRotation;
    }
}
