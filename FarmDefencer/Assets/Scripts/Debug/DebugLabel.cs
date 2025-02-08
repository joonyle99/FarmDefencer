using UnityEngine;

public class DebugLabel : MonoBehaviour
{
    private string _message;
    private float _timer;
    private Vector2 _position;
    private Color _textColor;

    private void OnGUI()
    {
        if (_timer > 0)
        {
            // GUIStyle 설정
            GUIStyle style = new GUIStyle();
            style.fontSize = 24;
            style.normal.textColor = _textColor;

            // 메시지를 화면에 표시
            GUI.Label(new Rect(_position.x, _position.y, 200, 50), _message, style);

            // 시간에 따라 위치와 투명도 변경
            //float elapsed = Time.deltaTime;
            //_timer -= elapsed;

            // 서서히 위로 이동
            //_position.y -= elapsed * 50f; // 초당 50픽셀씩 이동

            // 서서히 투명해짐
            //_textColor.a = Mathf.Clamp01(_timer / 2.0f); // 2초 동안 점점 투명해짐
        }
    }

    public void SetLabel(string newMessage, float duration, Vector3 spawnPos, Color color)
    {
        _message = newMessage;
        _timer = duration;
        // 원하는 위치에 뜨지 않는다..
        _position = Camera.main.WorldToScreenPoint(spawnPos);
        _textColor = color;
    }
}
