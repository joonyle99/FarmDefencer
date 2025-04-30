using UnityEngine;

public class ProgressFollower : MonoBehaviour
{
    [SerializeField] private ProgressBar _progressBar;
    private Vector3 _startPos;
    private Vector3 _endPos;

    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }
    private void Start()
    {
        _startPos = _rectTransform.anchoredPosition;
        _endPos = _startPos - new Vector3(_progressBar.GetBarWidth(), 0, 0);
    }

    private void Update()
    {
        // 프로그레스의 진행도에 따라 위치를 이동시킨다
        var progress = _progressBar.GetProgress();
        _rectTransform.anchoredPosition = Vector3.Lerp(_startPos, _endPos, progress);
    }
}
