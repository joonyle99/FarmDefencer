using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class HarvestBox : MonoBehaviour
{
	[SerializeField] private ProductEntry productEntry;
	private int _quota; // remaining count
	public int Quota
	{
		get => _quota;
		set
		{
			_quota = value;
			_cropQuotaText.text = _quota.ToString();
		}
	}
	public Vector2 ScreenPosition => _rectTransform.position;
	public Vector2 UISize => _rectTransform.sizeDelta;
	public bool IsAvailable
	{
		get
		{
			return _isAvailable;
		}
		set
		{
			_isAvailable = value;
			OnAvailabilityChanged();
		}
	}
	private bool _isAvailable;
	private Image _hotImage;
	private Image _specialImage;
	private Image _boxImage;
	private Image _productImage;
	private Image _lockImage;
	private Image _blinkImage;
	private TMP_Text _cropQuotaText;
	private RectTransform _rectTransform;
	public RectTransform RectTransform => _rectTransform;

	public void Blink(float duration) => StartCoroutine(DoBlink(duration));

	public void ClearSpecialOrHot()
	{
		_hotImage.enabled = false;
		_specialImage.enabled = false;
	}

	public void MarkSpecial() => _specialImage.enabled = true;
	public void MarkHot() => _hotImage.enabled = true;
	
	private void OnAvailabilityChanged()
	{
		var color = _isAvailable ? Color.white : new Color(0.4f, 0.4f, 0.4f, 1.0f);
		_boxImage.color = color;
		_productImage.color = color;
		_cropQuotaText.enabled = IsAvailable;
		
		_lockImage.enabled = !IsAvailable;
		
		_hotImage.enabled = false;
		_specialImage.enabled = false;
	}

	private void Awake()
	{
		_boxImage = transform.Find("BoxImage").GetComponent<Image>();
		_lockImage = transform.Find("LockImage").GetComponent<Image>();
		_productImage = transform.Find("ProductImage").GetComponent<Image>();
		_blinkImage = transform.Find("BlinkImage").GetComponent<Image>();
		_cropQuotaText = transform.Find("CropQuotaText").GetComponent<TMP_Text>();
		_hotImage = transform.Find("HotImage").GetComponent<Image>();
		_specialImage = transform.Find("SpecialImage").GetComponent<Image>();
		_rectTransform = GetComponent<RectTransform>();

		_lockImage.enabled = false;
		IsAvailable = false;
		_blinkImage.enabled = false;
		_specialImage.enabled = false;
		_hotImage.enabled = false;
	}

	private void Start()
	{
		_productImage.sprite = productEntry.ProductSprite;
		_cropQuotaText.text = _quota.ToString();
	}

	private IEnumerator DoBlink(float duration)
	{
		_blinkImage.enabled = true;
		var elapsed = 0.0f;
		while (elapsed < duration)
		{
			var x = elapsed / duration;
			var colorAlpha = -(2.0f * x - 1.0f) * (2.0f * x - 1.0f) + 1.0f; // 0->1->0 곡선

			elapsed += Time.deltaTime;
			var color = Color.white;
			color.a = colorAlpha;
			_blinkImage.color = color;
			
			yield return null;
		}
		_blinkImage.enabled = false;
	}
}
