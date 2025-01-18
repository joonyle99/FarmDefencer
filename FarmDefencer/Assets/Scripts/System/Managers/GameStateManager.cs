using UnityEngine;

public enum GameState
{
    None,

    // Common
    Normal,
    Pause,
    End,

    // Tycoon
    Water,

    // Defence
    Build,
    Wave,
}

public class GameStateManager : JoonyleGameDevKit.Singleton<GameStateManager>
{
    public GameState CurrentState { get; private set; } = GameState.None;

    private void Start()
    {
        ChangeState(GameState.Normal);
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
            case GameState.End:
                HandleEndState();
                break;
            case GameState.Build:
                HandleBuildState();
                break;
            case GameState.Wave:
                HandleWaveState();
                break;
        }

    }

    private void HandleNormalState()
    {

    }
    private void HandlePauseState()
    {

    }
    private void HandleEndState()
    {

    }
    private void HandleBuildState()
    {

    }
    private void HandleWaveState()
    {

    }
}
