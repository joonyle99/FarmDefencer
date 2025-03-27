using System;
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
			if (!IsAvailable)
			{
				throw new InvalidOperationException($"HarvestBox({productEntry.ProductName})가 잠긴 상태에서 개수를 조절하려 시도했습니다.");
			}
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException($"HarvestBox({productEntry.ProductName})의 개수를 0 미만으로 줄이려 시도했습니다.");
			}
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
	private Image _boxImage;
	private Image _productImage;
	private Image _lockImage;
	private TMP_Text _cropQuotaText;
	private RectTransform _rectTransform;
	public RectTransform RectTransform => _rectTransform;
	
	private void OnAvailabilityChanged()
	{
		var color = _isAvailable ? Color.white : new Color(0.4f, 0.4f, 0.4f, 1.0f);
		_boxImage.color = color;
		_productImage.color = color;
		_cropQuotaText.enabled = IsAvailable;
		_lockImage.enabled = !IsAvailable;
	}

	private void Awake()
	{
		_boxImage = transform.Find("BoxImage").GetComponent<Image>();
		_lockImage = transform.Find("LockImage").GetComponent<Image>();
		_productImage = transform.Find("ProductImage").GetComponent<Image>();
		_cropQuotaText = transform.Find("CropQuotaText").GetComponent<TMP_Text>();
		_rectTransform = GetComponent<RectTransform>();

		_lockImage.enabled = false;
		IsAvailable = false;
	}

	private void Start()
	{
		_productImage.sprite = productEntry.ProductSprite;
		_cropQuotaText.text = _quota.ToString();
	}
}
