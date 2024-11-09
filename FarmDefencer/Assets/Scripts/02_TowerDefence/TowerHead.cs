using UnityEngine;

public class TowerHead : MonoBehaviour
{
    [SerializeField] private Transform _muzzle;
    public Transform Muzzle => _muzzle;

    private float _startAngle;
    public float StartAngle => _startAngle;

    private void Start()
    {
        _startAngle = transform.rotation.eulerAngles.z;
    }
}
