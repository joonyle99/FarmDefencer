using UnityEngine;

public class Pathway : MonoBehaviour
{
    [SerializeField] private Transform _startPoint;
    [SerializeField] private Transform[] _path;

    public Transform StartPoint => _startPoint;
    public Transform[] Path => _path;
}
