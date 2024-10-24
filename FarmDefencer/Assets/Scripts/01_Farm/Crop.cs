using UnityEngine;

/// <summary>
/// �۹��� �ǹ��ϴ� Ŭ����.<br/>
/// ��ü�����δ� Field�� �� ĭ�� �ǹ��մϴ�. �� ���� �ɾ����� ���� �� ĭ�� �ǹ��մϴ�.
/// <br/><br/>
/// ���������� ǥ�õ� �ʵ���� ���� �÷��� �帧�� ���� �ڵ����� ������ �ʴ� �ݵ�� ������ �����ؾ� �ϴ� �����Դϴ�.<br/>
/// ���°����� ǥ�õ� �ʵ���� ���� �÷��� �帧�� ���� ���ϴ� �����, �ʿ��� ��� ���Ƿ� ������ �� ������ ������ �������� �ʾƵ� �˴ϴ�.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Crop : MonoBehaviour
{
    public Sprite CropSprite;
    [Header("������ - �䱸 ���� �ð�(��)")]
    public float GrowthSecondsRequired;
    [Header("������ - �䱸 �� �ѷ�(L)")]
    public float WaterRequired;
    [Space]
    [Header("���°� - ����(��)")]
    public float AgeSeconds;
    [Header("���°� - �۹��� �ɾ��� �ִ���")]
    public bool IsPlanted;
    [Header("���°� - �� ���差(L)")]
    [Tooltip("���� �ƴ϶� �Ĺ��� �����մϴ�. ���� ���� ��� 0�� �˴ϴ�.")]
    public float WaterStored;

    public float WaterConsumptionPerSecond => WaterRequired / GrowthSecondsRequired;

    private SpriteRenderer _spriteRenderer;

    /// <summary>
    /// �� Crop�� �ش� ��ǥ�� �ش��ϴ��� Ȯ���մϴ�.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool IsLocatedAt(Vector2 position) => Mathf.Abs(position.x - transform.position.x) < 0.5f && Mathf.Abs(position.y - transform.position.y) < 0.5f;

    /// <summary>
    /// �۹��� �ɽ��ϴ�. ��� ���°����� �ʱ�ȭ�մϴ�.<br/>
    /// </summary>
    /// <returns>�۹��� ���� ��� true, �̹� �ɾ��� �ִ� ��� false</returns>
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
    /// �۹��� ��Ȯ�մϴ�.
    /// </summary>
    /// <returns>�۹��� �ɾ��� �ְ� �� �ڶ� ��� ���°����� �ʱ�ȭ�ϸ� true, �̿��� ��� false</returns>
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
    /// �۹��� ���� �ݴϴ�.
    /// </summary>
    /// <param name="amount"></param>
    /// <returns>�۹��� �ɾ��� �ְ� ���� �� ��� true, �̿��� ��� false</returns>
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