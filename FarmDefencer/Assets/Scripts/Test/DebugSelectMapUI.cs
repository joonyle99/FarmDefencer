using TMPro;
using UnityEngine;

public class DebugSelectMapUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _mapIndex;
    [SerializeField] private TMP_InputField _stageIndex;

    public void SelectMaximumUnlockedMap()
    {
        if (GameStateManager.Instance.CurrentState is not GameState.Normal)
        {
            return;
        }

        var mapIndexStr = _mapIndex.text;
        var stageIndexStr = _stageIndex.text;

        if (int.TryParse(mapIndexStr, out int mapIndexInt) && int.TryParse(stageIndexStr, out int stageIndexInt))
        {
            MapManager.Instance.Debug_SetMaximumUnlockedMap(mapIndexInt, stageIndexInt);
        }
        else
        {
            Debug.LogWarning($"입력한 '{mapIndexStr}' 또는 '{stageIndexStr}' 는 올바른 숫자가 아닙니다.");
        }
    }

    public void SelectCurrentMap()
    {
        if (GameStateManager.Instance.CurrentState is not GameState.Normal)
        {
            return;
        }

        var mapIndexStr = _mapIndex.text;
        var stageIndexStr = _stageIndex.text;

        if (int.TryParse(mapIndexStr, out int mapIndexInt) && int.TryParse(stageIndexStr, out int stageIndexInt))
        {
            MapManager.Instance.Debug_SetCurrentMap(mapIndexInt, stageIndexInt);
        }
        else
        {
            Debug.LogWarning($"입력한 '{mapIndexStr}' 또는 '{stageIndexStr}' 는 올바른 숫자가 아닙니다.");
        }
    }

    public void DefenceStarter()
    {
        var defenceSceneOpenContextObject = new GameObject("DefenceSceneOpenContext");
        defenceSceneOpenContextObject.AddComponent<DefenceSceneOpenContext>();
        DontDestroyOnLoad(defenceSceneOpenContextObject);
    }
}
