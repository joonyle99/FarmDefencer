using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

public abstract class HarvestAnimationObject : MonoBehaviour
{
	public abstract void Play(ProductEntry productEntry, float duration, Vector2 from, Vector2 yo, Action callback);
}
public sealed class ScreenHarvestAnimationObject : HarvestAnimationObject
{
	private RectTransform _rectTransform;
	private Image _image;

	public override void Play(ProductEntry productEntry, float duration, Vector2 screenFrom, Vector2 screenTo, Action callback)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform.parent, screenFrom, Camera.main,
			out var anchoredScreenFrom);
		
		_image.sprite = productEntry.ProductSprite;
		_rectTransform.anchoredPosition = anchoredScreenFrom;
		_rectTransform.localScale = Vector3.one;
		_rectTransform
			.DOMove(screenTo, duration)
			.SetEase(Ease.OutCirc)
			.OnComplete(
			() =>
			{
				Destroy(gameObject);
				callback();
			});
	}

	private void Awake()
	{
		_rectTransform = GetComponent<RectTransform>();
		_image = GetComponent<Image>();
	}
}

public sealed class WorldHarvestAnimationObject : HarvestAnimationObject
{
	private SpriteRenderer _spriteRenderer;

	public override void Play(ProductEntry productEntry, float duration, Vector2 worldFrom, Vector2 worldTo, Action callback)
	{
		_spriteRenderer.sprite = productEntry.ProductSprite;
		transform.position = worldFrom;
		transform.localScale = Vector3.one;
		transform
			.DOMove(worldTo, duration)
			.SetEase(Ease.OutCirc)
			.OnComplete(
				() =>
				{
					Destroy(gameObject);
					callback();
				});
	}

	private void Awake()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();
	}
}