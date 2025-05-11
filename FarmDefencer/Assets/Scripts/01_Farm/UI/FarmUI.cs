using System;
using UnityEngine;

public sealed class FarmUI : MonoBehaviour
{
	public bool IsPaused => _farmDebugUI.IsPaused;
	
	private CoinsUI _coinsUI;
	private WateringCan _wateringCan;
	private HarvestInventory _harvestInventory;
	private TimerUI _timerUI;
	private CropGuide _cropGuide;
	private FarmDebugUI _farmDebugUI;

	public bool WateringCanAvailable
	{
		get => _wateringCan.gameObject.activeSelf;
		set => _wateringCan.gameObject.SetActive(value);
	}

	public void Init(FarmInput farmInput, ProductDatabase productDatabase, Action<Vector2> onWatering, Action<float> setDaytime, Func<float> getDaytime, Func<bool> isFarmPaused)
	{
		_wateringCan.Init(() => !isFarmPaused(), onWatering);
		farmInput.RegisterInputLayer(_wateringCan);
		farmInput.RegisterInputLayer(_cropGuide);
		
		_harvestInventory.Init(productDatabase);
		
		_farmDebugUI.Init(setDaytime, getDaytime);
	}

	public void SetTimerClockHand(float ratio) => _timerUI.SetClockhand(ratio);

	public void PlayProductFillAnimation(ProductEntry entry, Vector2 cropWorldPosition, int count, QuotaContext context)
	{
		_harvestInventory.PlayProductFillAnimation(entry, cropWorldPosition, count);
		_harvestInventory.UpdateInventory(context);
	}

	public void ToggleCropGuide(ProductEntry entry) => _cropGuide.Toggle(entry);

	public void UpdateHarvestInventory(QuotaContext context) => _harvestInventory.UpdateInventory(context);
	
	private void Awake()
	{
		_coinsUI = GetComponentInChildren<CoinsUI>();
		_wateringCan = GetComponentInChildren<WateringCan>();
		_harvestInventory = GetComponentInChildren<HarvestInventory>();
		_timerUI = GetComponentInChildren<TimerUI>();
		_cropGuide = GetComponentInChildren<CropGuide>();
		_farmDebugUI = GetComponentInChildren<FarmDebugUI>();
		ResourceManager.Instance.OnGoldChanged += _coinsUI.UpdateCoinText;
	}

	private void Start()
	{
		_coinsUI.UpdateCoinText(ResourceManager.Instance.Gold);
		_timerUI.SetClockhand(0.5f);
	}
}
