using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 디펜스 씬의 종료 이후 다른 씬으로의 전환을 담당하고,
/// 중도 종료 또는 정상 종료시의 저장을 돕는 컴포넌트.
/// GameStateManager.CurrentState를 LeavingDefenceScene으로 바꾸면 자동으로 처리됨.
/// </summary>
public sealed class DefenceSceneTransitioner : MonoBehaviour
{
    private void Awake()
    {
        GameStateManager.Instance.OnLeavingDefenceSceneState += OnLeavingDefenceSceneState;
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance is not null)
        {
            GameStateManager.Instance.OnLeavingDefenceSceneState -= OnLeavingDefenceSceneState;
        }
    }

    private void OnLeavingDefenceSceneState()
    {
        ResourceManager.Instance.SurvivedMonsters.Clear();
        
        var cleared = DefenceContext.Current.WaveSystem.SurvivedCount == 0; // WaveSystem.CompleteStageProcess() 의 EndingType 판정과 동일한 로직 사용.
        if (cleared)
        {
            // TODO 여기서 호출되는 OnMapChanged때문에 갑자기 디펜스 배경이 바뀔 경우가 예상됨.
            MapManager.Instance.ClearCurrentStage();
        }
        else
        {
            foreach (var survivedMonster in DefenceContext.Current.WaveSystem.SurvivedMonsters)
            {
                ResourceManager.Instance.SurvivedMonsters.Add(survivedMonster);
            }
        }

        // 여기 NullReferenceException은 기본적으로 그냥 두는게 맞음(타이쿤에서 저장이 잘 되면 절대 뜨지 않는 예외고 그 외의 경우에는 떠야 함)
        // 그래도 디펜스 씬 디버그 플레이 시를 감안해서 catch
        try
        {
            var loadedSave = SaveManager.Instance.LoadedSave;
            loadedSave["ResourceManager"] = ResourceManager.Instance.Serialize();
            loadedSave["MapManager"] = MapManager.Instance.Serialize();
            loadedSave["FarmClock"]["CurrentDaytime"] = 0.0f;
            SaveManager.Instance.FlushSave();
        }
        catch (NullReferenceException)
        {
            Debug.LogError("NullReferenceException");
        }

        SceneManager.LoadScene("Tycoon Scene");
    }

    // TODO 중도포기시 이거 호출되게 하기. (강제종료, 농장버튼 클릭, 설정 UI 나가기)
    private void OnDropOut()
    {
        var isBeforeWave = (int)GameStateManager.Instance.CurrentState < (int)GameState.Wave;
        
        // 웨이브 이전이라면 그냥 아무것도 안하면 됨.
        // 어차피 5분 다 썼는지 안썼는지는 타이쿤에서 넘어왔을 때 이미 저장되어 있음. -> 메인화면이나 타이쿤 씬에서 자동으로 처리 됨
        if (isBeforeWave)
        {
            return;
        }
        
        // 웨이브 이후라면 타이쿤 시간과 페널티만 저장하면 됨
        var loadedSave = SaveManager.Instance.LoadedSave;
        
        // TODO 페널티 적용할 몬스터들 다 여기 넣기.
        // foreach (var survivedMonster in DefenceContext.Current.WaveSystem.SurvivedMonsters)
        // {
        //     ResourceManager.Instance.SurvivedMonsters.Add(survivedMonster);
        // }
        
        // TODO 사용한 코인들 되돌려놓기.
        // ResourceManager.Instance.Gold += usedGold;
        
        try
        {
            loadedSave["ResourceManager"] = ResourceManager.Instance.Serialize();
            loadedSave["FarmClock"]["CurrentDaytime"] = 0.0f;
            SaveManager.Instance.FlushSave();
        }
        catch (NullReferenceException)
        {
            Debug.LogError("NullReferenceException");
        }
    }
}