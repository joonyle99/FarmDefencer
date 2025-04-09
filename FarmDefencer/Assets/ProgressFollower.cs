using UnityEngine;

public class ProgressFollower : MonoBehaviour
{
    [SerializeField] private ProgressBar _targetProgressBar;
    private Vector3 _startPos;
    private Vector3 _endPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _startPos = transform.position;
        _endPos = _startPos - new Vector3(_targetProgressBar.GetBarWidth(), 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        // 프로그레스의 진행도에 따라 위치를 이동시킨다
        var progress = _targetProgressBar.GetProgress(); // 0 ~ 1 (1에서 시작한다)
        progress = 1f - progress;
        transform.position = Vector3.Lerp(_startPos, _endPos, progress);
    }
}
