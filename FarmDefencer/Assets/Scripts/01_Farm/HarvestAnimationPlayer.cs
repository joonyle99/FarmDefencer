using UnityEngine;
using System;
using System.Collections.Generic;

public sealed class HarvestAnimationPlayer : MonoBehaviour
{
	private HarvestAnimationObject _animationObjectToClone;
	private List<HarvestAnimationObject> _animationObjectPool;
	
	public void PlayAnimation(ProductEntry productEntry, float duration, Vector2 screenFrom, Vector2 screenTo, Action callback)
	{
		if (_animationObjectPool.Count == 0)
		{
			var countToExtend = _animationObjectPool.Count + 1;
			for (var i = 0; i < countToExtend; ++i)
			{
				var newAnimationObject = Instantiate(_animationObjectToClone, transform);
				_animationObjectPool.Add(newAnimationObject.GetComponent<HarvestAnimationObject>());
			}
		}

		var animationComponent = _animationObjectPool[0];
		_animationObjectPool.RemoveAt(0);
		animationComponent.Play(
			productEntry,
			duration,
			screenFrom,
			screenTo,
			() =>
			{
				_animationObjectPool.Add(animationComponent);
				callback();
			});
	}

	private void Awake()
	{
		_animationObjectToClone = transform.Find("HarvestAnimationObject").GetComponent<HarvestAnimationObject>();
		_animationObjectPool = new List<HarvestAnimationObject>();
	}
}
