using System;
using UnityEngine;
using JoonyleGameDevKit;
using System.Collections.Generic;

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
    WaveInProgress,
    WaveCompleted,
    DefenceEnd,
}

public class GameStateManager : JoonyleGameDevKit.Singleton<GameStateManager>
{
    #region State

    private GameState _currentState = GameState.None;
    public GameState CurrentState => _currentState;

    public bool IsBuildState => CurrentState == GameState.Build;
    public bool IsWaveState => CurrentState == GameState.WaveInProgress || CurrentState == GameState.WaveCompleted;
    public bool IsDefenceState => IsBuildState || IsWaveState || CurrentState == GameState.DefenceEnd;

    private Dictionary<GameState, List<Delegate>> _stateCallbacks = new();

    #endregion

    #region Time

    public bool IsPause { get; private set; } = false;
    public bool IsPlayX2 { get; private set; } = false;
    private float _savedTimeScale = 1f;

    #endregion

    public void ChangeState(GameState nextState, params object[] args)
    {
        if (CurrentState == nextState)
        {
            return;
        }

        Debug.Log($"<color=orange>Current State: {nextState.ToName()}</color>");

        _currentState = nextState;

        if (_stateCallbacks.TryGetValue(nextState, out List<Delegate> callbacks))
        {
            foreach (Delegate callback in callbacks)
            {
                if (callback is Action)
                {
                    var action = (Action)callback;
                    action?.Invoke();
                }
                else if (callback is Action<EndingType>)
                {
                    var action = (Action<EndingType>)callback;
                    var endingType = (EndingType)args[0];
                    action?.Invoke(endingType);
                }
            }
        }
    }

    public void AddCallback(GameState state, Delegate callback)
    {
        if (_stateCallbacks.ContainsKey(state) == false)
        {
            _stateCallbacks.Add(state, new List<Delegate>());
        }

        if (_stateCallbacks[state].Contains(callback) == false)
        {
            _stateCallbacks[state].Add(callback);
        }
    }
    public void RemoveCallback(GameState state, Delegate callback)
    {
        if (_stateCallbacks.ContainsKey(state) == false)
        {
            return;
        }

        if (_stateCallbacks[state].Contains(callback) == true)
        {
            _stateCallbacks[state].Remove(callback);
        }
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
