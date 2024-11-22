using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HarvestBox : MonoBehaviour
{
	[SerializeField]
	public ProductEntry ProductEntry;
	[SerializeField]
	private int _quota; // remaining count
	public int Quota
	{
		get => _quota;
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException($"HarvestBox({ProductEntry.UniqueId})의 개수를 0 미만으로 줄이려 시도했습니다.");
			}
			_quota = value;
			_cropQuotaText.text = _quota.ToString();
		}
	}
	public Vector2 ScreenPosition => _rectTransform.position;
	public Vector2 UISize => _rectTransform.sizeDelta;
	private Image _boxImage;
	private Image _productImage;
	private TMP_Text _cropQuotaText;
	private RectTransform _rectTransform;
	public RectTransform RectTransform => _rectTransform;

	private void Awake()
	{
		_boxImage = transform.Find("BoxImage").GetComponent<Image>();
		_productImage = transform.Find("ProductImage").GetComponent<Image>();
		_cropQuotaText = transform.Find("CropQuotaText").GetComponent<TMP_Text>();
		_rectTransform = GetComponent<RectTransform>();
	}

	private void Start()
	{
		_productImage.sprite = ProductEntry.ProductSprite;
		_cropQuotaText.text = _quota.ToString();
	}
}
