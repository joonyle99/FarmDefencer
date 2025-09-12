using System;
using TMPro;
using UnityEngine;

public sealed class CropSign : MonoBehaviour
{
	public static readonly Vector2 SignClickSize = new Vector2 { x = 1.0f, y = 1.0f };
	
	private ProductEntry _productEntry;

	private SpriteRenderer _originalCoinImage;
	private SpriteRenderer _currentCoinImage;
	private SpriteRenderer _differentPriceMarkImage;

	private TMP_Text _originalPriceText;
	private TMP_Text _currentPriceText;
	
	private Func<(int, int)> _getPrice;

	public void Init(ProductEntry productEntry, Func<(int, int)> getPrice)
	{
		_productEntry = productEntry;
		_getPrice = getPrice;
		if (_getPrice().Item1 > 0) // 작물 열려 있음  
		{
			return;
		}

		_originalPriceText.gameObject.SetActive(false);
		_currentPriceText.gameObject.SetActive(false);
		_originalCoinImage.gameObject.SetActive(false);
		_currentCoinImage.gameObject.SetActive(false);
		_differentPriceMarkImage.gameObject.SetActive(false);
		enabled = false;
	}
	
	private void Start()
	{
		_originalCoinImage = transform.Find("OriginalCoinImage").GetComponent<SpriteRenderer>();
		_currentCoinImage = transform.Find("CurrentCoinImage").GetComponent<SpriteRenderer>();
		_differentPriceMarkImage = transform.Find("DifferentPriceMarkImage").GetComponent<SpriteRenderer>();
		
		_originalPriceText = transform.Find("OriginalPriceText").GetComponent<TMP_Text>();
		_originalPriceText.color = new Color(0.6f, 0.3f, 0.3f);
		_currentPriceText = transform.Find("CurrentPriceText").GetComponent<TMP_Text>();
		
		var currentPriceTextMeshRenderer = _currentPriceText.GetComponent<MeshRenderer>();
		currentPriceTextMeshRenderer.sortingLayerName = "OverDefault2";
		currentPriceTextMeshRenderer.sortingOrder = 32;
		
		var originalPriceTextMeshRenderer = _originalPriceText.GetComponent<MeshRenderer>();
		originalPriceTextMeshRenderer.sortingLayerName = "OverDefault2";
		originalPriceTextMeshRenderer.sortingOrder = 32;
	}

	private void Update()
	{
		var (originalPrice, currentPrice) = _getPrice();

		_originalPriceText.text = $"{originalPrice}";
		_currentPriceText.text = $"{currentPrice}";
		
		_currentPriceText.gameObject.SetActive(originalPrice != currentPrice);
		_currentPriceText.color = currentPrice > originalPrice ? Color.blue : Color.red;
		
		_currentCoinImage.gameObject.SetActive(originalPrice != currentPrice);
		
		_differentPriceMarkImage.gameObject.SetActive(originalPrice != currentPrice);
		_differentPriceMarkImage.color = currentPrice > originalPrice ? Color.blue : Color.red;
	}
}
