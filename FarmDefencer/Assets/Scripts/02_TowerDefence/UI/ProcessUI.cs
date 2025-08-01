using UnityEngine;
using UnityEngine.UI;

public class ProcessUI : MonoBehaviour
{
    [Header("━━━━━━━━ Process UI ━━━━━━━━")]
    [Space]

    [SerializeField] private Button _fightButton; // build state

    [SerializeField] private Button _pauseButton; // wave state / wave after state
    [SerializeField] private Button _resumeButton; // wave state / wave after state

    [Space]

    [SerializeField] private Button _playX1Button;
    [SerializeField] private Button _playX2Button;

    private bool _isPaused = false;
    private bool _isPlayX2 = false;

    private float _savedTimeScale = 1f;

    private void Awake()
    {
        GameStateManager.Instance.OnChangeState -= RefreshButton;
        GameStateManager.Instance.OnChangeState += RefreshButton;
    }
    private void Start()
    {
        _isPaused = false;
        _isPlayX2 = false;

        _fightButton.gameObject.SetActive(true);

        _resumeButton.gameObject.SetActive(false);
        _pauseButton.gameObject.SetActive(false);

        _playX1Button.gameObject.SetActive(false);
        _playX2Button.gameObject.SetActive(true);
    }

    public void RefreshButton()
    {
        var isBuildState = GameStateManager.Instance.CurrentState is GameState.Build;
        var isWaveState = GameStateManager.Instance.CurrentState is GameState.Wave || GameStateManager.Instance.CurrentState is GameState.WaveAfter;

        // TODO: 임시 코드.. 이벤트 리스너 등록 타이밍을 조절해야 한다
        if (_fightButton == null || _pauseButton == null || _resumeButton == null || _playX1Button == null || _playX2Button == null)
        {
            return;
        }

        _fightButton.gameObject.SetActive(isBuildState);

        _resumeButton.gameObject.SetActive(isWaveState && _isPaused);
        _pauseButton.gameObject.SetActive(isWaveState && !_isPaused);

        _playX1Button.gameObject.SetActive(_isPlayX2);
        _playX2Button.gameObject.SetActive(!_isPlayX2);
    }

    public void Fight()
    {
        if (GameStateManager.Instance.IsBuildState == false)
        {
            return;
        }

        GameStateManager.Instance.ChangeState(GameState.Wave);
    }

    public void TogglePause()
    {
        if (GameStateManager.Instance.IsWaveState == false)
        {
            return;
        }

        _isPaused = !_isPaused;

        if (_isPaused)
        {
            // pause 시점의 timeScale을 저장
            _savedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = _savedTimeScale;
        }

        _resumeButton.gameObject.SetActive(_isPaused);
        _pauseButton.gameObject.SetActive(!_isPaused);
    }
    public void TogglePlaySpeed()
    {
        if (GameStateManager.Instance.IsWaveState == false)
        {
            return;
        }

        _isPlayX2 = !_isPlayX2;

        if (_isPlayX2)
        {
            Time.timeScale = 2f;
        }
        else
        {
            Time.timeScale = 1f;
        }

        _savedTimeScale = Time.timeScale;

        _playX1Button.gameObject.SetActive(_isPlayX2);
        _playX2Button.gameObject.SetActive(!_isPlayX2);
    }
}
