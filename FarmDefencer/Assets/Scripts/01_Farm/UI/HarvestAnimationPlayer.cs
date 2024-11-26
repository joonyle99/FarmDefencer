using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HarvestAnimationPlayer : MonoBehaviour
{
	public float AnimationDuration = 0.5f;
	public GameObject HarvestAnimationObjectPrefab;
	private Queue<HarvestAnimationObject> _availableObjects;

	public void PlayAnimation(ProductEntry productEntry, Vector2 screenFrom, Vector2 screenTo, UnityAction callback)
	{
		if (_availableObjects.Count == 0)
		{
			var newAnimationObject = Instantiate(HarvestAnimationObjectPrefab);
			newAnimationObject.transform.SetParent(transform);
			_availableObjects.Enqueue(newAnimationObject.GetComponent<HarvestAnimationObject>());
		}
		var animationObject = _availableObjects.Dequeue();
		animationObject.PlayAnimation(
			productEntry,
			AnimationDuration,
			screenFrom,
			screenTo, 
			() => 
			{
				_availableObjects.Enqueue(animationObject);
				callback();
			});
	}

	private void Awake()
	{
		_availableObjects = new Queue<HarvestAnimationObject>();
		if (!HarvestAnimationObjectPrefab.TryGetComponent<HarvestAnimationObject>(out var _))
		{
			throw new MissingComponentException("HarvestAnimation의 HarvestAnimationObjectPrefab은 HarvestAnimationObject 컴포넌트를 갖지 않습니다.");
		}
	}
}
