using UnityEngine;

public class ProgressFollower : MonoBehaviour
{
    [SerializeField] private ProgressBar _progressBar;
    private Vector3 _startPos;
    private Vector3 _endPos;

    private Animator _animator;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rectTransform = GetComponent<RectTransform>();
    }
    private void Start()
    {
        _progressBar.OnStart -= PlayAnimator;
        _progressBar.OnStart += PlayAnimator;

        _progressBar.OnFinished -= StopAnimator;
        _progressBar.OnFinished += StopAnimator;

        _startPos = _rectTransform.anchoredPosition;
        var barWidth = _progressBar.GetBarWidth();
        _endPos = _startPos - new Vector3(barWidth - barWidth * 0.06f, 0, 0);
    }

    private void OnDestroy()
    {
        _progressBar.OnStart -= PlayAnimator;
        _progressBar.OnFinished -= StopAnimator;
    }

    private void Update()
    {
        // 프로그레스의 진행도에 따라 위치를 이동시킨다
        var progress = _progressBar.GetProgress();
        _rectTransform.anchoredPosition = Vector3.Lerp(_startPos, _endPos, progress);
    }

    private void PlayAnimator()
    {
        //_animator.gameObject.SetActive(true);
        _animator.Play("Play");
    }
    private void StopAnimator()
    {
        //_animator.Play("Stop");
        //_animator.gameObject.SetActive(false);
    }
}
