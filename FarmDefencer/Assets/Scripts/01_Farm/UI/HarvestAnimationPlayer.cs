using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public sealed class HarvestAnimationPlayer : MonoBehaviour
{
	[SerializeField] private float animationDuration = 0.5f;
	[SerializeField] private GameObject harvestAnimationObjectPrefab;
	private Queue<HarvestAnimationObject> _availableObjects;

	public void PlayAnimation(ProductEntry productEntry, Vector2 screenFrom, Vector2 screenTo, UnityAction callback)
	{
		if (_availableObjects.Count == 0)
		{
			var newAnimationObject = Instantiate(harvestAnimationObjectPrefab, transform);
			_availableObjects.Enqueue(newAnimationObject.GetComponent<HarvestAnimationObject>());
		}
		var animationObject = _availableObjects.Dequeue();
		animationObject.PlayAnimation(
			productEntry,
			animationDuration,
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
	}
}
