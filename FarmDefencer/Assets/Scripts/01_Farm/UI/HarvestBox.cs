using System.Collections;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum HotSpacialState
{
	None,
	Hot,
	Special
}

public sealed class HarvestBox : MonoBehaviour, IFarmSerializable
{
	private enum QuotaCycleState
	{
		NeverHarvested,
		Normal,
		PriceRaised,
	}
	
	[SerializeField] private ProductEntry productEntry;
	public ProductEntry ProductEntry => productEntry;
	
	private int _quota; // remaining count
	public int Quota
	{
		get => _quota;
		private set
		{
			_quota = value;
			_cropQuotaText.text = _quota.ToString();
		}
	}

	private HotSpacialState _hotSpacialState;

	public HotSpacialState HotSpacialState
	{
		get => _hotSpacialState;
		set
		{
			_hotImage.enabled = value == HotSpacialState.Hot;
			_specialImage.enabled = value == HotSpacialState.Special;
			_hotSpacialState = value;
		}
	}
	
	private Image _hotImage;
	private Image _specialImage;
	private Image _boxImage;
	private Image _productImage;
	private Image _lockImage;
	private Image _blinkImage;
	private TMP_Text _cropQuotaText;
	
	public bool IsAvailable { get; private set; }
	
	private float _blinkDuration;
	private (int, int) _quotaRange;
	private int _specialBonus;
	private QuotaCycleState _cycleState;
	public int Cycle { get; private set; }

	/// <summary>
	/// (originalPrice, currentPrice)
	/// </summary>
	public (int, int) Price
	{
		get
		{
			var originalPrice = productEntry.Price;
			var currentPrice = Mathf.RoundToInt(_cycleState == QuotaCycleState.PriceRaised ? originalPrice * 1.2f : originalPrice * Mathf.Pow(0.8f, Cycle));

			return (originalPrice, currentPrice);
		}
	}
	
	public JObject Serialize()
	{
		var json = new JObject();
		json["Quota"] = _quota;
		json["HotSpacialState"] = (int)_hotSpacialState;
		json["Cycle"] = Cycle;
		json["CycleState"] = (int)_cycleState;
		return json;
	}

	public void Deserialize(JObject json)
	{
		_cycleState = (QuotaCycleState)(json["CycleState"]?.Value<int>() ?? 0);
		Cycle = json["Cycle"]?.Value<int>() ?? 0;
		HotSpacialState = (HotSpacialState)(json["HotSpacialState"]?.Value<int>() ?? 0);
		Quota = json["Quota"]?.Value<int>() ?? 0;
	}

	public void Init(float blinkDuration, bool isAvailable, int minQuota, int maxQuota, int specialBonus)
	{
		_blinkDuration = blinkDuration;
		
		var color = isAvailable ? Color.white : new Color(0.4f, 0.4f, 0.4f, 1.0f);
		_boxImage.color = color;
		_productImage.color = color;
		_cropQuotaText.enabled = isAvailable;
		
		_lockImage.enabled = !isAvailable;
		
		_hotImage.enabled = false;
		_specialImage.enabled = false;
		_quotaRange = (minQuota, maxQuota);
		_specialBonus = specialBonus;
		
		IsAvailable = isAvailable;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns>새로 주문량 할당된 경우 true.</returns>
	public void FillQuota(out int price, out bool reAssigned)
	{
		Quota -= 1;
		price = productEntry.Price;
		if (_cycleState == QuotaCycleState.PriceRaised)
		{
			price = Mathf.RoundToInt(price * 1.2f);
		}
		
		if (HotSpacialState == HotSpacialState.Hot)
		{
			price *= 2;
		}
		else if (HotSpacialState == HotSpacialState.Special && Quota == 0)
		{
			price += _specialBonus;
		}
		
		reAssigned = Quota <= 0;
		if (reAssigned)
		{
			AssignQuota();
		}
	}

	public void ResetCycle()
	{
		Cycle = 0;
	}

	public void RaisePriceIfNeverFilled()
	{
		if (_cycleState == QuotaCycleState.NeverHarvested)
		{
			_cycleState = QuotaCycleState.PriceRaised;
		}
	}
	
	/// <summary>
	/// 낮 시작할 때(0.0초) 호출될 것을 기대하는 메소드.
	/// </summary>
    public void ResetQuota()
    {
	    HotSpacialState = HotSpacialState.None;
        Quota = Random.Range(_quotaRange.Item1, _quotaRange.Item2 + 1) / 10 * 10;
        SoundManager.Instance.PlaySfx("SFX_T_order_reset", SoundManager.Instance.orderResetVolume);
        Blink();
        
        Cycle = 0;
        _cycleState = QuotaCycleState.NeverHarvested;
    }
    
    private void AssignQuota()
    {
	    HotSpacialState = HotSpacialState.None;
	    Quota = Random.Range(_quotaRange.Item1, _quotaRange.Item2 + 1) / 10 * 10;
	    SoundManager.Instance.PlaySfx("SFX_T_order_reset", SoundManager.Instance.orderResetVolume);
	    Blink();
	    
	    Cycle += 1;
	    _cycleState = QuotaCycleState.Normal;
    }

	private void Blink() => StartCoroutine(DoBlink());
	
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
