using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class HarvestAnimationObject : MonoBehaviour
{
	private RectTransform _rectTransform;
	private Image _image;

	public void PlayAnimation(ProductEntry productEntry, float duration, Vector2 screenFrom, Vector2 screenTo, UnityAction callback)
	{
		_image.sprite = productEntry.ProductSprite;
		gameObject.SetActive(true);
		_rectTransform.position = screenFrom;
		_rectTransform
			.DOMove(screenTo, duration)
			.SetEase(Ease.OutCirc)
			.OnComplete(
			() =>
			{
				gameObject.SetActive(false);
				callback();
			});
	}

	private void Awake()
	{
		_rectTransform = GetComponent<RectTransform>();
		_image = GetComponent<Image>();
		gameObject.SetActive(false);
	}
}
