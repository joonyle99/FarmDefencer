using UnityEngine;
using JoonyleGameDevKit;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 팜디펜서의 모든 씬을 관리한다
/// </summary>
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

        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
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

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(SceneLoadedCo(scene));
    }
    private IEnumerator SceneLoadedCo(Scene scene)
    {
        // wait 1 frame to rigister game state handler
        yield return null;

        var sceneName = scene.name;
        var sceneType = sceneName.ToSceneType();
        var defaultState = GetDefaultGameState(sceneType);

        GameStateManager.Instance.ChangeState(defaultState);
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
