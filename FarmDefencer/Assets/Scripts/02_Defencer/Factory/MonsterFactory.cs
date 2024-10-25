using System.Collections.Generic;
using UnityEngine;

public sealed class MonsterFactory : Factory
{
    [Header("收收收收收收收收 Monster Factory 收收收收收收收收")]
    [Space]

    [SerializeField] private List<GameObject> _monsterPool = new List<GameObject>();
    public List<GameObject> MonsterPool => _monsterPool;

    protected override void AddObject(GameObject obj)
    {
        MonsterPool.Add(obj);
    }
    protected override GameObject RemoveLast()
    {
        var obj = MonsterPool[^1];
        MonsterPool.RemoveAt(MonsterPool.Count - 1);
        return obj;
    }

    public override bool IsEmptyPool()
    {
        return MonsterPool.Count == 0;
    }
}
