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

    private void OnLeavingDefenceSceneState(EndingType endingType)
    {
        // 여기 Json 접근에서 발생할 수 있는 NullReferenceException은 기본적으로 그냥 두는게 맞음(타이쿤에서 저장이 잘 되면 절대 뜨지 않는 예외고 그 외의 경우에는 떠야 함)
        // 그래도 디펜스 씬 디버그 플레이 시를 감안해서 catch
        try
        {
            // TODO: 분기별 데이터 처리 - 이 메소드가 호출된 원인이 중도 포기인지 디펜스 정상 종료인지 분기해야 함.
            // 각 분기에서 남은 타이쿤 시간을 초기화하거나, 클리어 스테이지를 1 증가시키는 등의 처리를 함.
            if (endingType == EndingType.Success)
            {
                OnSuccess();
            }
            else if (endingType == EndingType.Failure)
            {
                OnFailure();
            }
            else if (endingType == EndingType.GiveUp)
            {
                OnGiveUp();
            }
            
            var loadedSave = SaveManager.Instance.LoadedSave;
            loadedSave["ResourceManager"] = ResourceManager.Instance.Serialize();
            loadedSave["MapManager"] = MapManager.Instance.Serialize();
            SaveManager.Instance.FlushSave();
        }
        catch (NullReferenceException)
        {
            Debug.LogError("NullReferenceException");
        }

        SceneManager.LoadScene("Tycoon Scene");
    }

    private void OnSuccess()
    {
        MapManager.Instance.ClearCurrentStage(); // TODO 여기서 호출되는 OnMapChanged때문에 갑자기 디펜스 배경이 바뀔 경우가 예상됨.

        // TODO: 스테이지 1개 증가시키기..
        //MapManager.Instance.IncreaseClearedStageCount();

        SaveManager.Instance.LoadedSave["FarmClock"]["CurrentDaytime"] = 0.0f;
    }
    private void OnFailure()
    {
        foreach (var survivedMonster in DefenceContext.Current.WaveSystem.SurvivedMonsters)
        {
            ResourceManager.Instance.SurvivedMonsters.Add(survivedMonster);
        }

        SaveManager.Instance.LoadedSave["FarmClock"]["CurrentDaytime"] = 0.0f;
    }
    private void OnGiveUp()
    {
        var isBeforeWave = (int)GameStateManager.Instance.CurrentState < (int)GameState.Wave;

        // 웨이브 이전이라면 그냥 아무것도 안하면 됨
        // 어차피 5분 다 썼는지 안썼는지는 타이쿤에서 넘어왔을 때 이미 저장되어 있음. -> 메인화면이나 타이쿤 씬에서 자동으로 처리 됨
        if (isBeforeWave)
        {
            return;
        }

        // 웨이브 이후라면 페널티와 타이쿤 시간만 저장하면 됨

        // TODO: 페널티 적용할 몬스터들 다 여기 넣기
        foreach (var survivedMonster in DefenceContext.Current.WaveSystem.SurvivedMonsters)
        {
            ResourceManager.Instance.SurvivedMonsters.Add(survivedMonster);
        }

        SaveManager.Instance.LoadedSave["FarmClock"]["CurrentDaytime"] = 0.0f;

        // TODO: 사용한 코인들 되돌려놓기
        var totalCost = DefenceContext.Current.GridMap.CalculateAllOccupiedTowerCost();
        ResourceManager.Instance.Gold += totalCost;
    }
}