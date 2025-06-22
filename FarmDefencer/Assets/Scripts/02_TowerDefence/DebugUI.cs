using TMPro;
using UnityEngine;

public class DebugUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _tmpInputField;

    public void SelectMap()
    {
        if (GameStateManager.Instance.CurrentState is not GameState.Normal)
            return;

        var text = _tmpInputField.text;
        if (int.TryParse(text, out int mapIndex))
        {
            MapManager.Instance.CurrentMapIndex = mapIndex;
            MapManager.Instance.LoadCurrentMap();
            GameStateManager.Instance.ChangeState(GameState.Build);
        }
        else
        {
            Debug.LogWarning($"입력한 '{text}'는 올바른 숫자가 아닙니다.");
        }
    }
}
