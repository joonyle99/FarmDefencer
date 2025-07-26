using TMPro;
using UnityEngine;

public class DebugSelectMapUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _mapIndex;
    [SerializeField] private TMP_InputField _stageIndex;

    [SerializeField] private int _maxUnlockedMapIndex = 1;
    [SerializeField] private int _maxUnlockedStageIndex = 1;

    public void SelectMap()
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
}
