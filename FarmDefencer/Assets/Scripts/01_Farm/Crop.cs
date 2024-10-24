using UnityEngine;

/// <summary>
/// 작물을 의미하는 클래스.<br/>
/// 구체적으로는 Field의 한 칸을 의미합니다. 즉 아직 심어지지 않은 한 칸도 의미합니다.
/// <br/><br/>
/// 설정값으로 표시된 필드들은 게임 플레이 흐름에 의해 자동으로 변하지 않는 반드시 사전에 설정해야 하는 값들입니다.<br/>
/// 상태값으로 표시된 필드들은 게임 플레이 흐름에 따라 변하는 값들로, 필요할 경우 임의로 설정할 수 있으며 별도로 조작하지 않아도 됩니다.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Crop : MonoBehaviour
{
    public Sprite CropSprite;
    [Header("설정값 - 요구 성장 시간(초)")]
    public float GrowthSecondsRequired;
    [Header("설정값 - 요구 물 총량(L)")]
    public float WaterRequired;
    [Space]
    [Header("상태값 - 나이(초)")]
    public float AgeSeconds;
    [Header("상태값 - 작물이 심어져 있는지")]
    public bool IsPlanted;
    [Header("상태값 - 물 저장량(L)")]
    [Tooltip("땅이 아니라 식물이 저장합니다. 새로 심을 경우 0이 됩니다.")]
    public float WaterStored;

    public float WaterConsumptionPerSecond => WaterRequired / GrowthSecondsRequired;

    private SpriteRenderer _spriteRenderer;

    /// <summary>
    /// 이 Crop이 해당 좌표에 해당하는지 확인합니다.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool IsLocatedAt(Vector2 position) => Mathf.Abs(position.x - transform.position.x) < 0.5f && Mathf.Abs(position.y - transform.position.y) < 0.5f;

    /// <summary>
    /// 작물을 심습니다. 모든 상태값들을 초기화합니다.<br/>
    /// </summary>
    /// <returns>작물을 심은 경우 true, 이미 심어져 있는 경우 false</returns>
    public bool TryPlant()
    {
        if (IsPlanted)
        {
            return false;
        }

        AgeSeconds = 0.0f;
        WaterStored = 0.0f;
        IsPlanted = true;
        _spriteRenderer.sprite = CropSprite;
        return true;
    }

    /// <summary>
    /// 작물을 수확합니다.
    /// </summary>
    /// <returns>작물이 심어져 있고 다 자란 경우 상태값들을 초기화하며 true, 이외의 경우 false</returns>
    public bool TryHarvest()
    {
        if (IsPlanted
            && AgeSeconds >= GrowthSecondsRequired)
        {
            AgeSeconds = 0.0f;
            WaterStored = 0.0f;
            IsPlanted = false;
            _spriteRenderer.sprite = null;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 작물에 물을 줍니다.
    /// </summary>
    /// <param name="amount"></param>
    /// <returns>작물이 심어져 있고 물을 준 경우 true, 이외의 경우 false</returns>
    public bool TryWatering(float amount)
    {
        if (!IsPlanted)
        {
            return false;
        }

        WaterStored += amount;
        return true;
    }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        var deltaTime = Time.deltaTime;
        if (IsPlanted)
        {
            if (WaterStored >= WaterConsumptionPerSecond * deltaTime)
            {
                AgeSeconds += deltaTime;
                WaterStored -= WaterConsumptionPerSecond * deltaTime;
            }
        }
    }
}