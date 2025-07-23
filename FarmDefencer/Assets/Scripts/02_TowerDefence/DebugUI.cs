using TMPro;
using UnityEngine;

public class DebugUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _tmpInputField;

    public void SelectMap()
    {
        if (GameStateManager.Instance.CurrentState is not GameState.Normal)
            return;

        var mapIndexStr = _tmpInputField.text;
        if (int.TryParse(mapIndexStr, out int mapIndexInt))
        {
            MapManager.Instance.Debug_SetCurrentMap(mapIndexInt);
            GameStateManager.Instance.ChangeState(GameState.Build);
        }
        else
        {
            Debug.LogWarning($"입력한 '{mapIndexStr}'는 올바른 숫자가 아닙니다.");
        }
    }
}
