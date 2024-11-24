using UnityEngine;

/// <summary>
/// 게임에서 전역적으로 사용되는 리소스를 관리합니다.
/// </summary>
/// <remarks>
/// <see cref="ManagerClassGuideline"/>
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

            EarnGold(amount);
            Debug.Log($"earn {amount} gold (<color=orange>current: {_gold}</color>)");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            var amount = 100;

            SpendGold(amount);
            Debug.Log($"spend {amount} gold (<color=orange>current: {_gold}</color>)");
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
