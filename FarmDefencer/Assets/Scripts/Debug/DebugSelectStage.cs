using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugSelectStage : MonoBehaviour
{
    [SerializeField] private TMP_InputField _maxUnlockedMapIndex;
    [SerializeField] private TMP_InputField _maxUnlockedStageIndex;

    [Space]

    [SerializeField] private TMP_InputField _currentMapIndex;
    [SerializeField] private TMP_InputField _currentStageIndex;

    public void SelectMap()
    {
        if (GameStateManager.Instance.CurrentState is not GameState.Main)
        {
            return;
        }

        var maxUnlockedMapIndexStr = _maxUnlockedMapIndex.text;
        var maxUnlockedStageIndexStr = _maxUnlockedStageIndex.text;

        var currentMapIndexStr = _currentMapIndex.text;
        var currentStageIndexStr = _currentStageIndex.text;

        if (int.TryParse(maxUnlockedMapIndexStr, out int mapIndexInt_1) && int.TryParse(maxUnlockedStageIndexStr, out int stageIndexInt_1) && int.TryParse(currentMapIndexStr, out int mapIndexInt_2) && int.TryParse(currentStageIndexStr, out int stageIndexInt_2))
        {
            if (mapIndexInt_2 > mapIndexInt_1 || (mapIndexInt_1 == mapIndexInt_2 && stageIndexInt_2 > stageIndexInt_1))
            {
                Debug.LogWarning($"현재 맵 인덱스({mapIndexInt_2})와 스테이지 인덱스({stageIndexInt_2})는 최대 해금 맵 인덱스({mapIndexInt_1})와 스테이지 인덱스({stageIndexInt_1})보다 작아야 합니다.");
                return;
            }

            if (mapIndexInt_1 < 1 || stageIndexInt_1 < 1 || mapIndexInt_2 < 1 || stageIndexInt_2 < 1)
            {
                Debug.LogWarning($"모든 인덱스는 1 이상이어야 합니다. - mapIndexInt_1({mapIndexInt_1}), stageIndexInt_1({stageIndexInt_1}), mapIndexInt_2({mapIndexInt_2}), stageIndexInt_2({stageIndexInt_2})");
                return;
            }

            if (mapIndexInt_1 > 10 || stageIndexInt_1 > 10 || mapIndexInt_2 > 10 || stageIndexInt_2 > 10)
            {
                Debug.LogWarning($"모든 인덱스는 10 이하이어야 합니다. - mapIndexInt_1({mapIndexInt_1}), stageIndexInt_1({stageIndexInt_1}), mapIndexInt_2({mapIndexInt_2}), stageIndexInt_2({stageIndexInt_2})");
                return;
            }

            MapManager.Instance.Debug_SetMaximumUnlockedMap(mapIndexInt_1, stageIndexInt_1);
            MapManager.Instance.Debug_SetCurrentMap(mapIndexInt_2, stageIndexInt_2);

            ApplyMapManagerToSave();
        }
        else
        {
            Debug.LogWarning($"올바른 숫자를 입력해주세요.");
        }
    }

    public void DefenceStarter()
    {
        var defenceSceneOpenContextObject = new GameObject("DefenceSceneOpenContext");
        defenceSceneOpenContextObject.AddComponent<DefenceSceneOpenContext>();
        DontDestroyOnLoad(defenceSceneOpenContextObject);
    }

    private void ApplyMapManagerToSave()
    {
        var loadedSave = SaveManager.Instance.LoadedSave;
        loadedSave["MapManager"] = MapManager.Instance.Serialize();
        SaveManager.Instance.FlushSave();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
