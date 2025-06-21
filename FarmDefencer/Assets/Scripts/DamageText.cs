using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    [SerializeField] private float floatSpeed; // 초당 픽셀 이동
    [SerializeField] private float fadeDuration;

    private TextMeshPro _text;

    private float _timer;
    private float _offset;
    private Color _originalColor;

    private Transform _origin;

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

        // 누적 오프셋 증가
        _offset += floatSpeed * delta;

        // origin을 따라가면서, 위로 떠오름
        // transform.position = _origin.position + Vector3.up * _offset;
        transform.position = new Vector3(transform.position.x, _origin.position.y, transform.position.z) + Vector3.up * _offset;

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

    public void Init(string message, DamageType type, Transform origin)
    {
        _text.text = message;
        _text.color = GetColorByType(type);
        _originalColor = _text.color;

        _timer = fadeDuration;

        _origin = origin;
        _offset = 0f;
        transform.position = _origin.position;

        _isTrigger = true;
    }
    private Color GetColorByType(DamageType type)
    {
        switch (type)
        {
            case DamageType.Fire:
                return ConstantConfig.RED;
            case DamageType.Poison:
                return ConstantConfig.GREEN;
            case DamageType.Electric:
                return ConstantConfig.YELLOW;
            case DamageType.Normal:
                return ConstantConfig.WHITE;
            default:
                return ConstantConfig.WHITE;
        }
    }
}