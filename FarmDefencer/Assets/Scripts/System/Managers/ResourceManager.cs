using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

/// <summary>
/// 게임에서 사용되는 자원(Gold, ...) 관리합니다.
/// </summary>
public class ResourceManager : JoonyleGameDevKit.Singleton<ResourceManager>, IFarmSerializable
{
    public List<string> SurvivedMonsters { get; private set; }

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

    public JObject Serialize()
    {
        var jsonObject = new JObject();
        jsonObject.Add("Gold", _gold);

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
        Gold = json.ParsePropertyOrAssign("Gold", 0);

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
        SetGold(400);
    }

    // gold
    public bool TrySpendGold(int amount)
    {
        if (Gold < amount)
        {
            Debug.Log($"You don't have enough gold to build this tower");
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
