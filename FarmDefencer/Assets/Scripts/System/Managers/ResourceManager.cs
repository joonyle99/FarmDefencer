using Newtonsoft.Json.Linq;
using UnityEngine;

/// <summary>
/// 게임에서 사용되는 자원(Gold, ...) 관리합니다.
/// </summary>
public class ResourceManager : JoonyleGameDevKit.Singleton<ResourceManager>, IFarmSerializable
{
    private int _gold;
    public int Gold
    {
        get => _gold;
        set
        {
            _gold = value;

            if (_gold <= 0)
            {
                _gold = 0;
            }

            OnGoldChanged?.Invoke(_gold);
        }
    }

    public event System.Action<int> OnGoldChanged;

    public JObject Serialize() => new(new JProperty("Gold", _gold));

    public void Deserialize(JObject json) => _gold = json.Property("Gold")?.Value.Value<int>() ?? 0;

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
        SetGold(200);
    }

    // gold
    public bool TrySpendGold(int amount)
    {
        if (Gold < amount)
        {
            Debug.Log("You don't have enough gold to build this tower");
            return false;
        }
        SpendGold(amount);
        return true;
    }
    public void EarnGold(int amount)
    {
        Gold += amount;
    }
    public void SpendGold(int amount)
    {
        Gold -= amount;
    }
    public void SetGold(int amount)
    {
        Gold = amount;
    }
    public int GetGold()
    {
        return Gold;
    }
}
