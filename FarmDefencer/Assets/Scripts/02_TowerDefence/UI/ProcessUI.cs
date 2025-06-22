using UnityEngine;
using UnityEngine.UI;

public class ProcessUI : MonoBehaviour
{
    [Header("━━━━━━━━ Process UI ━━━━━━━━")]
    [Space]

    [SerializeField] private Button _fightButton;
    [SerializeField] private Button _pauseButton;
    [SerializeField] private Button _resumeButton;

    [Space]

    [SerializeField] private Button _playX1Button;
    [SerializeField] private Button _playX2Button;

    private WaveSystem _waveSystem;

    private float _savedTimeScale;

    private void Awake()
    {
        _waveSystem = FindFirstObjectByType<WaveSystem>();
    }
    private void Start()
    {
        Initialize();
    }

    public void ResetButton01()
    {
        _fightButton.gameObject.SetActive(true);
        _pauseButton.gameObject.SetActive(false);
        _resumeButton.gameObject.SetActive(false);
    }
    public void ResetButton02()
    {
        _playX2Button.gameObject.SetActive(true);
        _playX1Button.gameObject.SetActive(false);
    }
    public void Initialize()
    {
        ResetButton01();
        //ResetButton02();
    }

    // 1
    public void Fight()
    {
        if (GameStateManager.Instance.CurrentState is not GameState.Build)
            return;

        GameStateManager.Instance.ChangeState(GameState.Wave);

        _fightButton.gameObject.SetActive(false);
        _pauseButton.gameObject.SetActive(true);

        _playX2Button.gameObject.SetActive(true);
        _playX1Button.gameObject.SetActive(false);
    }
    public void Pause()
    {
        if (GameStateManager.Instance.CurrentState is not GameState.Wave
            && GameStateManager.Instance.CurrentState is not GameState.WaveAfter)
            return;

        // pause 시점의 timeScale을 저장
        _savedTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        _pauseButton.gameObject.SetActive(false);
        _resumeButton.gameObject.SetActive(true);
    }
    public void Resume()
    {
        if (GameStateManager.Instance.CurrentState is not GameState.Wave
            && GameStateManager.Instance.CurrentState is not GameState.WaveAfter)
            return;

        // pause 시점의 timeScale을 복원
        Time.timeScale = _savedTimeScale;

        _pauseButton.gameObject.SetActive(true);
        _resumeButton.gameObject.SetActive(false);
    }

    // 2
    public void PlayX1()
    {
        if (GameStateManager.Instance.CurrentState is not GameState.Wave
            && GameStateManager.Instance.CurrentState is not GameState.WaveAfter)
            return;

        Time.timeScale = 1f;
        _savedTimeScale = Time.timeScale;

        _playX2Button.gameObject.SetActive(true);
        _playX1Button.gameObject.SetActive(false);
    }
    public void PlayX2()
    {
        if (GameStateManager.Instance.CurrentState is not GameState.Wave
            && GameStateManager.Instance.CurrentState is not GameState.WaveAfter)
            return;

        Time.timeScale = 3f;
        _savedTimeScale = Time.timeScale;

        _playX2Button.gameObject.SetActive(false);
        _playX1Button.gameObject.SetActive(true);
    }
}
