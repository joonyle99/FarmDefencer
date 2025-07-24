using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public sealed class CropGuide : MonoBehaviour, IFarmInputLayer
{
	[SerializeField] private Sprite cropGuideImage_carrot;
	[SerializeField] private Sprite cropGuideImage_potato;
	[SerializeField] private Sprite cropGuideImage_corn;
	[SerializeField] private Sprite cropGuideImage_cabbage;
	[SerializeField] private Sprite cropGuideImage_cucumber;
	[SerializeField] private Sprite cropGuideImage_eggplant;
	[SerializeField] private Sprite cropGuideImage_sweetpotato;
	[SerializeField] private Sprite cropGuideImage_mushroom;

	public int InputPriority => IFarmInputLayer.Priority_CropGuide;
	
	private Image _image;

	public bool OnHold(Vector2 initialWorldPosition, Vector2 deltaWorldPosition, bool isEnd, float deltaHoldTime)
	{
		return false;
	}

	public bool OnTap(Vector2 worldPosition)
	{
		if (_image.sprite != null)
		{
			Close();
			return true;
		}
		else
		{
			return false;
		}
	}

	public void Close()
	{
		_image.sprite = null;
		gameObject.SetActive(false);
	}

	public void Show(ProductEntry productEntry)
	{
		var sprite = GetSpriteOf(productEntry);

		if (sprite == null)
		{
			Close();
		}
		else
		{
			_image.sprite = sprite;
			gameObject.SetActive(true);
		}
	}

	/// <summary>
	/// 이미 열려 있는 가이드가 있는 경우, Close()로 동작.
	/// 이외의 경우에는 해당 가이드를 띄움.
	/// </summary>
	/// <param name="productEntry"></param>
	public void Toggle(ProductEntry productEntry)
	{
		if (_image.sprite != null)
		{
			Close();
			return;
		}

		var sprite = GetSpriteOf(productEntry);

		if (sprite == null)
		{
			Close();
			return;
		}

		_image.sprite = sprite;
		gameObject.SetActive(true);
	}

	private void Awake()
	{
		_image = GetComponent<Image>();
		Close();
	}

	private Sprite GetSpriteOf(ProductEntry productEntry)
	{
		if (productEntry == null)
		{
			return null;
		}

		return productEntry.ProductName switch
		{
			"product_carrot" => cropGuideImage_carrot,
			"product_potato" => cropGuideImage_potato,
			"product_corn" => cropGuideImage_corn,
			"product_cabbage" => cropGuideImage_cabbage,
			"product_cucumber" => cropGuideImage_cucumber,
			"product_eggplant" => cropGuideImage_eggplant,
			"product_sweetpotato" => cropGuideImage_sweetpotato,
			"product_mushroom" => cropGuideImage_mushroom,
			_ => null
		};
	}
}
