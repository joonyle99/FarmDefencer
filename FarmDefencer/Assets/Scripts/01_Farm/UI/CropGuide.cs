using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CropGuide : MonoBehaviour, IFarmInputLayer
{
	[SerializeField] private Sprite _cropGuideImage_carrot;
	[SerializeField] private Sprite _cropGuideImage_potato;
	[SerializeField] private Sprite _cropGuideImage_corn;
	[SerializeField] private Sprite _cropGuideImage_cabbage;
	[SerializeField] private Sprite _cropGuideImage_cucumber;
	[SerializeField] private Sprite _cropGuideImage_eggplant;
	[SerializeField] private Sprite _cropGuideImage_sweetpotato;
	[SerializeField] private Sprite _cropGuideImage_mushroom;

	private Image _image;

	public bool OnSingleHolding(Vector2 initialWorldPosition, Vector2 deltaWorldPosition, bool isEnd, float deltaHoldTime)
	{
		return false;
	}

	public bool OnSingleTap(Vector2 worldPosition)
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

		return productEntry.UniqueId switch
		{
			"product_carrot" => _cropGuideImage_carrot,
			"product_potato" => _cropGuideImage_potato,
			"product_corn" => _cropGuideImage_corn,
			"product_cabbage" => _cropGuideImage_cabbage,
			"product_cucumber" => _cropGuideImage_cucumber,
			"product_eggplant" => _cropGuideImage_eggplant,
			"product_sweetpotato" => _cropGuideImage_sweetpotato,
			"product_mushroom" => _cropGuideImage_mushroom,
			_ => null
		};
	}
}
