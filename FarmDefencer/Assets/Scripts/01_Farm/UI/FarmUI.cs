using System;
using UnityEngine;

public sealed class FarmUI : MonoBehaviour
{
	private CoinsUI _coinsUI;
	private WateringCan _wateringCan;
	private HarvestInventory _harvestInventory;
	private TimerUI _timerUI;
	private CropGuide _cropGuide;
	private FarmDebugUI _farmDebugUI;

	public void Init(FarmClock farmClock, FarmInput farmInput, Action<Vector2> onWatering, ProductDatabase productDatabase)
	{
		_wateringCan.Init(() => !farmClock.Stopped, onWatering);
		farmInput.RegisterInputLayer(_wateringCan);
		
		_farmDebugUI.Init(farmClock);
		
		farmInput.RegisterInputLayer(_cropGuide);
		
		_harvestInventory.Init(productDatabase);
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
