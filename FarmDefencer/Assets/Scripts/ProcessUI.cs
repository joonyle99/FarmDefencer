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
        _playX1Button.gameObject.SetActive(true);
        _playX2Button.gameObject.SetActive(false);
    }
    public void Initialize()
    {
        ResetButton01();
        ResetButton02();
    }

    // 1
    public void Fight()
    {
        Debug.Log("Fight");
        _waveSystem.StartWaveProcess();

        _fightButton.gameObject.SetActive(false);
        _pauseButton.gameObject.SetActive(true);
    }
    public void Pause()
    {
        Debug.Log("Pause");
        Time.timeScale = 0f;

        _pauseButton.gameObject.SetActive(false);
        _resumeButton.gameObject.SetActive(true);
    }
    public void Resume()
    {
        Debug.Log("Resume");
        Time.timeScale = 1f;

        _pauseButton.gameObject.SetActive(true);
        _resumeButton.gameObject.SetActive(false);
    }

    // 2
    public void PlayX1()
    {
        Debug.Log("PlayX1");
        Time.timeScale = 3f;

        _playX1Button.gameObject.SetActive(false);
        _playX2Button.gameObject.SetActive(true);
    }
    public void PlayX2()
    {
        Debug.Log("PlayX2");
        Time.timeScale = 1f;

        _playX1Button.gameObject.SetActive(true);
        _playX2Button.gameObject.SetActive(false);
    }
}
