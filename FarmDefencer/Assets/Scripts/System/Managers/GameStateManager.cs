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

    }
    private void HandlePauseState()
    {

    }

    // Tycoon

    // Defence
    private void HandleBuildState()
    {

    }
    private void HandleWaveState()
    {

    }
    private void HandleWaveAfterState()
    {

    }
    private void HandleDefenceEndState()
    {

    }
}
