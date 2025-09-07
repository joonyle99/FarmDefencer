using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

/// <summary>
/// 게임에서 사용되는 자원(Coin, ...) 관리합니다.
/// </summary>
public class ResourceManager : JoonyleGameDevKit.Singleton<ResourceManager>, IFarmSerializable
{
    public List<string> SurvivedMonsters { get; private set; }

    private int _coin;
    public int Coin
    {
        get => _coin;
        set
        {
            _coin = value;

            if (_coin <= 0)
            {
                _coin = 0;
            }

            OnCoinChanged?.Invoke(_coin);
        }
    }

    public event System.Action<int> OnCoinChanged;

    public JObject Serialize()
    {
        var jsonObject = new JObject();
        jsonObject.Add("Gold", _coin);

        var jsonSurvivedMonsters = new JArray();
        foreach (var survivedMonster in SurvivedMonsters)
        {
            jsonSurvivedMonsters.Add(survivedMonster);
        }
        jsonObject.Add("SurvivedMonsters", jsonSurvivedMonsters);

        return jsonObject;
    }

    public void Deserialize(JObject json)
    {
        Coin = json.ParsePropertyOrAssign("Gold", 0);

        SurvivedMonsters.Clear();
        if (json["SurvivedMonsters"] is JArray jsonSurvivedMonsters)
        {
            foreach (var jsonSurvivedMonster in jsonSurvivedMonsters)
            {
                var survivedMonster = jsonSurvivedMonster.Value<string>();
                if (survivedMonster is null)
                {
                    continue;
                }

                SurvivedMonsters.Add(survivedMonster);
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        
        SurvivedMonsters = new List<string>();
    }

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        SetGold(999999999);
    }

    // gold
    public bool TrySpendGold(int amount)
    {
        if (Coin < amount)
        {
            Debug.Log($"You don't have enough gold to build this tower");
            return false;
        }
        SpendGold(amount);
        return true;
    }
    public void EarnGold(int amount)
    {
        Coin += amount;
    }
    public void SpendGold(int amount)
    {
        Coin -= amount;
    }
    public void SetGold(int amount)
    {
        Coin = amount;
    }
    public int GetGold()
    {
        return Coin;
    }
}
