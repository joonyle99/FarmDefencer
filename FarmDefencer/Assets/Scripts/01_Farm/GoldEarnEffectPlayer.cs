using UnityEngine;
using System.Collections.Generic;

public sealed class GoldEarnEffectPlayer : MonoBehaviour
{
    private GoldEarnEffect _effectToClone;
    private List<GoldEarnEffect> _effectPool;
    public void PlayEffectAt(Vector2 worldPosition, int gold)
    {
        if (_effectPool.Count == 0)
        {
            var countToClone = _effectPool.Capacity + 1;
            for (var i = 0; i < countToClone + 1; ++i)
            {
                var newEffectObject = Instantiate(_effectToClone.gameObject, transform);
                var newEffect = newEffectObject.GetComponent<GoldEarnEffect>();
                _effectPool.Add(newEffect);
            }
        }

        var effect = _effectPool[0];
        _effectPool.RemoveAt(0);
        effect.transform.position = worldPosition;
        effect.PlayEffect(gold, finishedEffect => _effectPool.Add(finishedEffect));
    }

    private void Start()
    {
        _effectPool = new List<GoldEarnEffect>();
        _effectToClone = transform.Find("GoldEarnEffect").GetComponent<GoldEarnEffect>();
        _effectPool.Add(_effectToClone);
    }
}
