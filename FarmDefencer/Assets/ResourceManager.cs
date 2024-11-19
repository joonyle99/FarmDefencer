using UnityEngine;

/// <summary>
/// ���ӿ��� ���������� ���Ǵ� ���ҽ��� �����մϴ�.
/// </summary>
/// <remarks>
/// Manager vs Supervisor �� ���� ������ <see cref="ManagerClassGuideline"/>�� �����ϼ���
/// </remarks>
public class ResourceManager : JoonyleGameDevKit.Singleton<ResourceManager>
{
    private int _gold;
    public int Gold
    {
        get => _gold;
        set
        {
            _gold = value;

            if (_gold < 0)
            {
                _gold = 0;
            }

            OnGoldChanged?.Invoke(_gold);
        }
    }

    public event System.Action<int> OnGoldChanged;

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            var amount = 100;

            Debug.Log($"earn {amount} gold (<color=orange>current: {_gold}</color>)");
            EarnGold(amount);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            var amount = 100;

            Debug.Log($"spend {amount} gold (<color=orange>current: {_gold}</color>)");
            SpendGold(amount);
        }
#endif
    }

    public void EarnGold(int amount)
    {
        Gold += amount;
    }
    public void SpendGold(int amount)
    {
        Gold -= amount;
    }
    public int GetGold()
    {
        return Gold;
    }
}
