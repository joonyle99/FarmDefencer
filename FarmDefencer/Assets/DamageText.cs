using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    [SerializeField] private float floatSpeed; // 초당 픽셀 이동
    [SerializeField] private float fadeDuration;

    private TextMeshPro _text;

    private float _timer;
    private Color _originalColor;

    private bool _isTrigger = false;

    private void Awake()
    {
        _text = GetComponent<TextMeshPro>();
    }

    private void Update()
    {
        if (_isTrigger == false)
        {
            return;
        }

        float delta = Time.deltaTime;
        _timer -= delta;

        // 위로 떠오르기
        transform.position += Vector3.up * floatSpeed * delta;

        // 투명도 조정
        float alpha = Mathf.Clamp01(_timer / fadeDuration);
        Color newColor = _originalColor;
        newColor.a = alpha;
        _text.color = newColor;

        // 종료 시 삭제
        if (_timer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    public void Init(string message, DamageType type, Vector3 worldPos)
    {
        _text.text = message;
        _text.color = GetColorByType(type);
        _originalColor = _text.color;

        _timer = fadeDuration;

        transform.position = worldPos;

        _isTrigger = true;
    }
    private Color GetColorByType(DamageType type)
    {
        switch (type)
        {
            case DamageType.Fire:
                return Color.red;
            case DamageType.Poison:
                return Color.green;
            case DamageType.Electric:
                return Color.yellow;
            case DamageType.Normal:
                return Color.white;
            default:
                return Color.white;
        }
    }
}