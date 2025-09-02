using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 해당 맵에 존재하는 모든 스테이지 버튼을 초기화하고,
/// 버튼 클릭 시 해당 스테이지로 이동하는 역할을 담당
/// </summary>
public class StageInitializer : MonoBehaviour
{
    /// <summary>
    /// 해당 맵에 존재하는 모든 스테이지 버튼을 자식으로 가지고 있는 오브젝트
    /// </summary>
    [SerializeField] private GameObject _stageButtons;

    private void Start()
    {
        var maxUnlockedMapIdx = MapManager.Instance.MaximumUnlockedMapIndex;
        var maxUnlockedStageIdx = MapManager.Instance.MaximumUnlockedStageIndex;

        // 스테이지 버튼 초기화
        foreach (Transform child in _stageButtons.transform)
        {
            var stageButton = child.GetComponent<StageButton>();

            // 지금 맵이 이미 해금된 맵보다 이전 맵인지
            var isUnlockedMap = stageButton.MapIndex < maxUnlockedMapIdx;
            // 지금 맵이 현재 해금된 맵이고, 그 안에서 해금된 스테이지 이하인지
            var isUnlockedStage = stageButton.MapIndex == maxUnlockedMapIdx && stageButton.StageIndex <= maxUnlockedStageIdx;

            stageButton.SetStageButtonEnabled(isUnlockedMap || isUnlockedStage);
            stageButton.OnClicked += OnStageButtonClicked;
        }
    }

    private void OnStageButtonClicked(int mapIndex, int stageIndex)
    {
        // 현재 맵 / 스테이지 설정
        MapManager.Instance.CurrentMapIndex = mapIndex;
        MapManager.Instance.CurrentStageIndex = stageIndex;

        // 로딩 씬 설정
        SceneLoadContext.NextSceneName = "Defence Scene";
        //SceneLoadContext.OnSceneChanged = null;
        SceneLoadContext.OnSceneChanged += () =>
        {
            SceneLoadContext.OnSceneChanged = null;

            var defenceSceneOpenContext = new GameObject("DefenceSceneOpenContext");
            defenceSceneOpenContext.AddComponent<DefenceSceneOpenContext>();
            DontDestroyOnLoad(defenceSceneOpenContext);
        };

        SceneManager.LoadScene("Loading Scene");
    }
}
