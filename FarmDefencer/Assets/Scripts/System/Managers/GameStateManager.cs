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

    }
    private void HandlePauseState()
    {

    }

    // Tycoon

    // Defence
    private void HandleBuildState()
    {
        SoundManager.Instance.PlayMapAmb();
    }
    private void HandleWaveState()
    {
        SoundManager.Instance.PlayMapSong();
    }
    private void HandleWaveAfterState()
    {

    }
    private void HandleDefenceEndState()
    {
        SoundManager.Instance.StopBgm();
    }
}
