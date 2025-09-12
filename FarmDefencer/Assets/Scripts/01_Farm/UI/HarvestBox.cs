using System.Collections;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class HarvestBox : MonoBehaviour, IFarmSerializable
{
	[SerializeField] private ProductEntry productEntry;
	public ProductEntry ProductEntry => productEntry;
	
	private int _quota; // remaining count
	public int Quota
	{
		get => _quota;
		set
		{
			if (_quota <= 0 && value > 0)
			{
				SoundManager.Instance.PlaySfx("SFX_T_order_reset", SoundManager.Instance.orderResetVolume);
				Blink();
			}

			IsAvailable = value > 0;
			_quota = value;
			_cropQuotaText.text = _quota.ToString();
		}
	}

	public bool IsAvailable { get; private set; }
	private float _blinkDuration;
	private Image _hotImage;
	private Image _specialImage;
	private Image _boxImage;
	private Image _productImage;
	private Image _lockImage;
	private Image _blinkImage;
	private TMP_Text _cropQuotaText;

	public JObject Serialize() => new(new JProperty("Quota", _quota));

	public void Deserialize(JObject json) => Quota = json["Quota"]?.Value<int?>() ?? 0;

	public void Init(float blinkDuration, bool isAvailable)
	{
		_blinkDuration = blinkDuration;
		IsAvailable = isAvailable;
		
		var color = IsAvailable ? Color.white : new Color(0.4f, 0.4f, 0.4f, 1.0f);
		_boxImage.color = color;
		_productImage.color = color;
		_cropQuotaText.enabled = IsAvailable;
		
		_lockImage.enabled = !IsAvailable;
		
		_hotImage.enabled = false;
		_specialImage.enabled = false;
	}

	public void Blink() => StartCoroutine(DoBlink());

	public void ClearSpecialOrHot()
	{
		_hotImage.enabled = false;
		_specialImage.enabled = false;
	}

	public void MarkSpecial() => _specialImage.enabled = true;
	public void MarkHot() => _hotImage.enabled = true;
	
	private void Awake()
	{
		_boxImage = transform.Find("BoxImage").GetComponent<Image>();
		_lockImage = transform.Find("LockImage").GetComponent<Image>();
		_productImage = transform.Find("ProductImage").GetComponent<Image>();
		_blinkImage = transform.Find("BlinkImage").GetComponent<Image>();
		_cropQuotaText = transform.Find("CropQuotaText").GetComponent<TMP_Text>();
		_hotImage = transform.Find("HotImage").GetComponent<Image>();
		_specialImage = transform.Find("SpecialImage").GetComponent<Image>();

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

	private IEnumerator DoBlink()
	{
		_blinkImage.enabled = true;
		var elapsed = 0.0f;
		while (elapsed < _blinkDuration)
		{
			var x = elapsed / _blinkDuration;
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
