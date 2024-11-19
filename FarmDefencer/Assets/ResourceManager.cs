using UnityEngine;

/// <summary>
/// 게임에서 전역적으로 사용되는 리소스를 관리합니다.
/// </summary>
/// <remarks>
/// Manager vs Supervisor 에 대한 설명은 <see cref="ManagerClassGuideline"/>를 참조하세요
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
