using TMPro;
using UnityEngine;

/// <summary>
/// 0번 자식은 SpriteRenderer를 가진 LockSprite 오브젝트,
/// 1번 자식은 TMP_Text를 가진 RemainingTimeText 오브젝트
/// </summary>
public sealed class CropLock : MonoBehaviour
{
    private SpriteRenderer _remainingGauge;

    public void UpdateGauge(float ratio)
    {
        var clamped = Mathf.Clamp01(ratio);
        _remainingGauge.transform.localScale = new Vector3(clamped, 1.0f, 1.0f);
    }
    
    private void Awake()
    {
        _remainingGauge = transform.Find("Gauge/Remaining").GetComponent<SpriteRenderer>();
    }
}
