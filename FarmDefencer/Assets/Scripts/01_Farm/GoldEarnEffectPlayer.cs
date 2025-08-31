using UnityEngine;
using System.Collections.Generic;

public sealed class GoldEarnEffectPlayer : MonoBehaviour
{
    private GoldEarnEffect _effectToClone;
    private List<GoldEarnEffect> _effectPool;
    
    public GoldEarnEffect PlayEffectAt(Vector2 worldPosition, int gold)
    {
        var effect = GetEffectObject(worldPosition);
        effect.PlayEffect(gold);
        return effect;
    }

    public GoldEarnEffect GetEffectObject(Vector2 worldPosition)
    {
        if (_effectPool.Count == 0)
        {
            var countToClone = _effectPool.Capacity + 1;
            for (var i = 0; i < countToClone + 1; ++i)
            {
                var newEffectObject = Instantiate(_effectToClone.gameObject, transform);
                var newEffect = newEffectObject.GetComponent<GoldEarnEffect>();
                newEffect.Init(finishedEffect => _effectPool.Add(finishedEffect));
                _effectPool.Add(newEffect);
            }
        }
        
        var effect = _effectPool[0];
        _effectPool.RemoveAt(0);
        effect.transform.position = worldPosition;
        effect.GoldDisplaying = 0;
        return effect;
    }

    private void Start()
    {
        _effectPool = new List<GoldEarnEffect>();
        _effectToClone = transform.Find("GoldEarnEffect").GetComponent<GoldEarnEffect>();
    }
}
