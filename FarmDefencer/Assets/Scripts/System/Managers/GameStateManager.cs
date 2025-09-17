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
    LeavingDefenceScene,
}

public class GameStateManager : JoonyleGameDevKit.Singleton<GameStateManager>
{
    public GameState CurrentState { get; private set; } = GameState.None;

    public bool IsBuildState => CurrentState == GameState.Build;
    public bool IsWaveState => CurrentState == GameState.Wave || CurrentState == GameState.WaveAfter;
    public bool IsDefenceState => IsBuildState || IsWaveState || CurrentState == GameState.DefenceEnd;

    public event Action OnNormalState;
    public event Action OnBuildState;
    /// <summary>
    /// 웨이브가 시작되기 직전에 호출되는 이벤트
    /// </summary>
    public event Action OnWaveState;
    /// <summary>
    /// 모든 웨이브가 끝난 후 호출되는 이벤트
    /// </summary>
    public event Action OnWaveAfterState;
    /// <summary>
    /// 몬스터가 모두 사라진 후 호출되는 이벤트
    /// </summary>
    public event Action OnDefenceEndState;
    /// <summary>
    /// EndingUI까지 모두 보여준 후 다른 씬으로 이동해야 할 때 호출되는 이벤트
    /// </summary>
    public event Action<EndingType> OnLeavingDefenceSceneState;

    public event Action OnChangeState;

    public bool IsPause { get; private set; } = false;
    public bool IsPlayX2 { get; private set; } = false;
    private float _savedTimeScale = 1f;

    private void Start()
    {
        ChangeState(GameState.Normal);
    }

    public void ChangeState(GameState nextState, params object[] args)
    {
        if (CurrentState == nextState)
        {
            return;
        }

        CurrentState = nextState;
        Debug.Log($"<color=orange>Current State: {CurrentState.ToString()}</color>");

        switch (CurrentState)
        {
            case GameState.Normal:
                HandleNormalState();
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

    // Common
    private void HandleNormalState()
    {
        OnNormalState?.Invoke();
    }

    // Tycoon

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

        // build state로 옮김
        //SoundManager.Instance.PlayDefenceMapSong();
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
