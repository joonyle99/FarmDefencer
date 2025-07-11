using System;
using UnityEngine;

public sealed class FarmUI : MonoBehaviour
{
	private CoinsUI _coinsUI;
	private WateringCan _wateringCan;
	private HarvestInventory _harvestInventory;
	private TimerUI _timerUI;
	private CropGuide _cropGuide;

	public bool WateringCanAvailable
	{
		get => _wateringCan.gameObject.activeSelf;
		set => _wateringCan.gameObject.SetActive(value);
	}

	public void Init(FarmInput farmInput,
		ProductDatabase productDatabase,
		int currentMap,
		int currentStage,
		Action<Vector2> onWatering,
		Func<bool> isFarmPaused,
		Func<float> getRemainingDaytimeAlpha)
	{
		_wateringCan.Init(() => !isFarmPaused(), onWatering);
		farmInput.RegisterInputLayer(_wateringCan);
		farmInput.RegisterInputLayer(_cropGuide);
		
		_harvestInventory.Init(productDatabase);
		_timerUI.Init(currentMap, currentStage, getRemainingDaytimeAlpha);
	}

	public void PlayProductFillAnimation(ProductEntry entry, Vector2 cropWorldPosition, int count)
		=> _harvestInventory.PlayProductFillAnimation(entry, cropWorldPosition, count);

	public void PlayQuotaAssignAnimation(Func<ProductEntry, bool> isProductAvailable, Func<ProductEntry, int> getProductQuota) => _harvestInventory.PlayQuotaAssignAnimation(isProductAvailable, getProductQuota);

	public void ToggleCropGuide(ProductEntry entry) => _cropGuide.Toggle(entry);

	public void UpdateHarvestInventory(Func<ProductEntry, bool> isProductAvailable, Func<ProductEntry, int> getProductQuota, Func<ProductEntry> getHotProduct, Func<ProductEntry> getSpecialProduct)
		=> _harvestInventory.UpdateInventory(isProductAvailable, getProductQuota, getHotProduct, getSpecialProduct);

	public void PlayCoinAnimation() => _coinsUI.PlayAnimation();
	
	private void Awake()
	{
		_coinsUI = GetComponentInChildren<CoinsUI>();
		_wateringCan = GetComponentInChildren<WateringCan>();
		_harvestInventory = GetComponentInChildren<HarvestInventory>();
		_timerUI = GetComponentInChildren<TimerUI>();
		_cropGuide = GetComponentInChildren<CropGuide>();
		ResourceManager.Instance.OnGoldChanged += _coinsUI.SetCoin;
	}

	private void Start()
	{
		_coinsUI.SetCoin(ResourceManager.Instance.Gold);
	}
}
