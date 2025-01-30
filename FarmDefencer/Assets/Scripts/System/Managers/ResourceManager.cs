using UnityEngine;

/// <summary>
/// 게임에서 사용되는 자원(Gold, ...) 관리합니다.
/// </summary>
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

    private void Start()
    {
        Initialize();
    }
    private void Update()
    {
#if UNITY_EDITOR
        // CHEAT: Earn Gold
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            var amount = 100;

            EarnGold(amount);
            Debug.Log($"earn {amount} gold (<color=orange>current: {_gold}</color>)");
        }
        // CHEAT: Spend Gold
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            var amount = 100;

            SpendGold(amount);
            Debug.Log($"spend {amount} gold (<color=orange>current: {_gold}</color>)");
        }
#endif
    }

    public void Initialize()
    {
        EarnGold(300);
    }

    // gold
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
