using UnityEngine;
using JoonyleGameDevKit;
using UnityEngine.SceneManagement;
using System.Collections;

public enum SceneType
{
    None,

    Loading,
    Title,
    Main,
    Tycoon,
    World,
    Defence,
}

public class SceneChangeManager : JoonyleGameDevKit.Singleton<SceneChangeManager>
{
    protected override void Awake()
    {
        base.Awake();

        SceneManager.sceneLoaded += InitLoadedScene;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= InitLoadedScene;
    }

    private void InitLoadedScene(Scene scene, LoadSceneMode mode)
    {
        // init sound
        SoundManager.Instance.StopBgm();
        SoundManager.Instance.StopAmb();
        SoundManager.Instance.StopSfx();

        // init game state
        StartCoroutine(InitDefaultGameState(scene));
    }
    private IEnumerator InitDefaultGameState(Scene scene)
    {
        yield return null;

        var sceneName = scene.name;
        var sceneType = sceneName.ToSceneType();
        var defaultState = GetDefaultGameState(sceneType);

        GameStateManager.Instance.ChangeState(defaultState);
    }

    private GameState GetDefaultGameState(SceneType sceneType)
    {
        switch (sceneType)
        {
            case SceneType.Loading:
                return GameState.Loading;
            case SceneType.Title:
                return GameState.Title;
            case SceneType.Main:
                return GameState.Main;
            case SceneType.Tycoon:
                return GameState.Tycoon;
            case SceneType.World:
                return GameState.World;
            case SceneType.Defence:
                return GameState.Build;

            default:
                return GameState.None;
        }
    }

    public void ChangeScene(SceneType sceneType)
    {
        var sceneName = sceneType.ToSceneName();

        ChangeScene(sceneName);
    }
    public void ChangeScene(int sceneIdx)
    {
        var scene = SceneManager.GetSceneByBuildIndex(sceneIdx);
        var sceneName = scene.name;

        ChangeScene(sceneName);
    }
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
