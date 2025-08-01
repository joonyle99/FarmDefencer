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

    [Space]

    [SerializeField] private Button _settingButton;

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

        _pauseButton.gameObject.SetActive(false);
        _resumeButton.gameObject.SetActive(false);

        _playX1Button.gameObject.SetActive(false);
        _playX2Button.gameObject.SetActive(true);

        _settingButton.gameObject.SetActive(true);
    }

    public void RefreshButton()
    {
        var isBuildState = GameStateManager.Instance.IsBuildState;
        var isWaveState = GameStateManager.Instance.IsWaveState;

        if (_fightButton == null ||
            _pauseButton == null ||
            _resumeButton == null ||
            _playX1Button == null ||
            _playX2Button == null)
        {
            return;
        }

        _fightButton?.gameObject.SetActive(isBuildState);

        _resumeButton?.gameObject.SetActive(isWaveState && _isPaused);
        _pauseButton?.gameObject.SetActive(isWaveState && !_isPaused);

        _playX1Button?.gameObject.SetActive(_isPlayX2);
        _playX2Button?.gameObject.SetActive(!_isPlayX2);
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
            SoundManager.Instance.PauseAll();
        }
        else
        {
            Time.timeScale = _savedTimeScale;
            SoundManager.Instance.ResumeAll();
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

        if (_isPaused)
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
    public void Setting()
    {
        var isBuildState = GameStateManager.Instance.IsBuildState;
        var isWaveState = GameStateManager.Instance.IsWaveState;

        if (isBuildState == false || isWaveState == false)
        {
            return;
        }
    }
}
