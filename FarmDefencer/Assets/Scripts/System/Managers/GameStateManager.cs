using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    None,

    // Common
    Normal,
    Pause,

    // Tycoon
    Water,
    TycoonEnd,

    // Defence
    Build,
    Wave,
    WaveAfter,
    DefenceEnd,
}

public class GameStateManager : JoonyleGameDevKit.Singleton<GameStateManager>
{
    public GameState CurrentState { get; private set; } = GameState.None;

    public Action OnNormalState;
    public Action OnPauseState;
    public Action OnBuildState;
    /// <summary>
    /// 웨이브가 시작되기 직전에 호출되는 이벤트
    /// </summary>
    public Action OnWaveState;
    /// <summary>
    /// 모든 웨이브가 끝난 후 호출되는 이벤트
    /// </summary>
    public Action OnWaveAfterState;
    /// <summary>
    /// 몬스터가 모두 사라진 후 호출되는 이벤트
    /// </summary>
    public Action OnDefenceEndState;

    /// <summary>
    /// 메인메뉴에서 타이쿤 씬으로 이동하기 전 true로 설정하여 타이쿤 씬에서 ResourceManager 등의 값들을 JSON에서 불러오도록 하는 프로퍼티.
    /// 디펜스에서 이동하여 false인 경우에는 기존 ResourceManager 등에 설정된 값을 그대로 사용함.
    /// </summary>
    public bool IsTycoonInitialLoad { get; set; }

    private void Start()
    {
        ChangeState(GameState.Normal);
    }

    private void Update()
    {
        // CHEAT: Scene Reload
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);

            ResourceManager.Instance.Initialize();
        }
    }

    public void ChangeState(GameState nextState)
    {
        if (CurrentState == nextState)
        {
            return;
        }

        CurrentState = nextState;
        Debug.Log($"Current State: {CurrentState.ToString()}");

        switch (CurrentState)
        {
            case GameState.Normal:
                HandleNormalState();
                break;
            case GameState.Pause:
                HandlePauseState();
                break;
            case GameState.Build:
                HandleBuildState();
                break;
            case GameState.Wave:
                HandleWaveState();
                break;
            case GameState.WaveAfter:
                HandleWaveAfterState();
                break;
            case GameState.DefenceEnd:
                HandleDefenceEndState();
                break;
        }
    }

    // Common
    private void HandleNormalState()
    {
        OnNormalState?.Invoke();
    }
    private void HandlePauseState()
    {
        OnPauseState?.Invoke();
    }

    // Tycoon

    // Defence
    private void HandleBuildState()
    {
        Debug.Log("Enter Build State");

        OnBuildState?.Invoke();
        SoundManager.Instance.PlayMapAmb();
    }
    private void HandleWaveState()
    {
        Debug.Log("Enter Wave State");

        OnWaveState?.Invoke();
        SoundManager.Instance.PlayMapSong();
    }
    private void HandleWaveAfterState()
    {
        Debug.Log("Enter WaveAfter State");

        OnWaveAfterState?.Invoke();
    }
    private void HandleDefenceEndState()
    {
        Debug.Log("Enter DefenceEnd State");

        OnDefenceEndState?.Invoke();
        SoundManager.Instance.StopBgm();
    }
}
