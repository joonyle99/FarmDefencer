using JoonyleGameDevKit;
using System;
using UnityEngine;

public enum GameState
{
    None,

    // Loading
    Loading,

    // Title
    Title,

    // Main
    Main,

    // World
    World,

    // Tycoon
    Tycoon,

    // Defence
    Build,
    Wave,
    WaveAfter,
    DefenceEnd,
    LeavingDefenceScene,
}

public class GameStateManager : JoonyleGameDevKit.Singleton<GameStateManager>
{
    private GameState _currentState = GameState.None;
    public GameState CurrentState => _currentState;

    public bool IsBuildState => CurrentState == GameState.Build;
    public bool IsWaveState => CurrentState == GameState.Wave || CurrentState == GameState.WaveAfter;
    public bool IsDefenceState => IsBuildState || IsWaveState || CurrentState == GameState.DefenceEnd;

    // Loading
    public event Action OnLoadingState;

    // Title
    public event Action OnTitleState;

    // Main
    public event Action OnMainState;

    // World
    public event Action OnWorldState;

    // Tycoon
    public event Action OnTycoonState;

    // Defence
    public event Action OnBuildState;
    public event Action OnWaveState;
    public event Action OnWaveAfterState;
    public event Action OnDefenceEndState;
    public event Action<EndingType> OnLeavingDefenceSceneState;

    //
    public event Action OnChangeState;

    public bool IsPause { get; private set; } = false;
    public bool IsPlayX2 { get; private set; } = false;
    private float _savedTimeScale = 1f;

    public void ChangeState(GameState nextState, params object[] args)
    {
        if (CurrentState == nextState)
        {
            return;
        }

        _currentState = nextState;

        Debug.Log($"<color=orange>Current State: {CurrentState.ToName()}</color>");

        switch (CurrentState)
        {
            case GameState.Loading:
                HandleLoadingState();
                break;
            case GameState.Title:
                HandleTitleState();
                break;
            case GameState.Main:
                HandleMainState();
                break;
            case GameState.World:
                HandleWorldState();
                break;
            case GameState.Tycoon:
                HandleTycoonState();
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
            case GameState.LeavingDefenceScene :
                EndingType endingType = (EndingType)args[0];
                HandleLeavingDefenceSceneState(endingType);
                break;
        }

        OnChangeState?.Invoke();
    }

    // Loading
    private void HandleLoadingState()
    {
        OnLoadingState?.Invoke();
    }

    // Title
    private void HandleTitleState()
    {
        OnTitleState?.Invoke();
    }
    
    // Main
    private void HandleMainState()
    {
        OnMainState?.Invoke();

        SoundManager.Instance.StopAmb();
        SoundManager.Instance.StopBgm();
    }
    
    // World
    private void HandleWorldState()
    {
        OnWorldState?.Invoke();

        SoundManager.Instance.StopAmb();
        SoundManager.Instance.StopBgm();
    }

    // Tycoon
    private void HandleTycoonState()
    {
        OnTycoonState?.Invoke();
    }

    // Defence
    private void HandleBuildState()
    {
        OnBuildState?.Invoke();

        SoundManager.Instance.PlayDefenceAmb(MapManager.Instance.CurrentMap);
        SoundManager.Instance.PlayDefenceBgm(MapManager.Instance.CurrentMap);
    }
    private void HandleWaveState()
    {
        OnWaveState?.Invoke();
    }
    private void HandleWaveAfterState()
    {
        OnWaveAfterState?.Invoke();
    }
    private void HandleDefenceEndState()
    {
        OnDefenceEndState?.Invoke();
        SoundManager.Instance.StopBgm();

        // TODO: Fight 버튼, 타워 설치, 등 불가능하게 막기
        DefenceContext.Current.DefenceUIController.ProcessUI.PauseBlocker.gameObject.SetActive(true);

        // TODO: 게임 배속 복구하기
        Time.timeScale = 1f;
    }
    private void HandleLeavingDefenceSceneState(EndingType endingType)
    {
        OnLeavingDefenceSceneState?.Invoke(endingType);
    }

    public void TogglePause()
    {
        // TODO: Pause를 하지 못하는 상황 체크

        IsPause = !IsPause;

        if (IsPause)
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
    }
    public void TogglePlaySpeed()
    {
        if (IsPause)
        {
            return;
        }

        IsPlayX2 = !IsPlayX2;

        if (IsPlayX2)
        {
            Time.timeScale = 2f;
        }
        else
        {
            Time.timeScale = 1f;
        }

        _savedTimeScale = Time.timeScale;
    }
}
