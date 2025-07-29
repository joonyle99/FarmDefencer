using UnityEngine;
using System;
using UnityEngine.UI;

public sealed class HarvestAnimationPlayer : MonoBehaviour
{
	private GameObject _screenAnimationObjectsRoot;
	private GameObject _worldAnimationObjectsRoot;

	public void PlayScreenAnimation(ProductEntry productEntry, float duration, Vector2 screenFrom, Vector2 screenTo, Action callback)
	{
		var newAnimationObject = new GameObject("ScreenHarvestAnimationObject");
		newAnimationObject.transform.parent = _screenAnimationObjectsRoot.transform;

		newAnimationObject.AddComponent<Image>();
		var animationComponent = newAnimationObject.AddComponent<ScreenHarvestAnimationObject>();
		animationComponent.Play(
			productEntry,
			duration,
			screenFrom,
			screenTo, 
			callback);
	}
	
	public void PlayWorldAnimation(ProductEntry productEntry, float duration, Vector2 worldFrom, Vector2 worldTo, Action callback)
	{
		var newAnimationObject = new GameObject("WorldHarvestAnimationObject");
		newAnimationObject.transform.parent = _worldAnimationObjectsRoot.transform;

		newAnimationObject.AddComponent<SpriteRenderer>();
		var animationComponent = newAnimationObject.AddComponent<WorldHarvestAnimationObject>();
		animationComponent.Play(
			productEntry,
			duration,
			worldFrom,
			worldTo, 
			callback);
	}

	private void Awake()
	{
		_screenAnimationObjectsRoot = transform.Find("ScreenAnimationObjects").gameObject;
		_worldAnimationObjectsRoot = transform.Find("WorldAnimationObjects").gameObject;
	}
}
