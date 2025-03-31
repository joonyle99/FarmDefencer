using TMPro;
using UnityEngine;

/// <summary>
/// 0번 자식은 SpriteRenderer를 가진 LockSprite 오브젝트,
/// 1번 자식은 TMP_Text를 가진 RemainingTimeText 오브젝트
/// </summary>
public sealed class CropLock : MonoBehaviour
{
    private SpriteRenderer _lockSpriteRenderer;
    private TMP_Text _remainingTimeText;

    public void UpdateTime(float time)
    {
        _remainingTimeText.SetText(((int)time).ToString());
    }
    
    private void Awake()
    {
        _lockSpriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        _remainingTimeText = transform.GetChild(1).GetComponent<TMP_Text>();
    }
}
