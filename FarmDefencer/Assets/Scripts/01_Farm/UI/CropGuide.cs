using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CropGuide : MonoBehaviour
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
			Debug.LogError($"알 수 없는 ProductEntry.UniqueId: {productEntry.UniqueId}");
			Close();
		}
		else
		{
			_image.sprite = sprite;
			gameObject.SetActive(true);
		}
	}

	public void Toggle(ProductEntry productEntry)
	{
		var sprite = GetSpriteOf(productEntry);

		if (sprite == null)
		{
			Debug.LogError($"알 수 없는 ProductEntry.UniqueId: {productEntry.UniqueId}");
			Close();
			return;
		}

		if (_image.sprite == sprite)
		{
			Close();
		}
		else
		{
			_image.sprite = sprite;
			gameObject.SetActive(true);
		}
	}

	private void Awake()
	{
		_image = GetComponent<Image>();
		Close();
	}

	private Sprite GetSpriteOf(ProductEntry productEntry) =>
		productEntry.UniqueId switch
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
