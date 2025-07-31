using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class FarmUI : MonoBehaviour
{
	private CoinsUI _coinsUI;
	private HarvestInventory _harvestInventory;
	private CropGuide _cropGuide;
	private Button _battleButton;
	private Button _settingButton;

	public bool IsCropGuideShowing => _cropGuide.gameObject.activeSelf;

	private Func<bool> _canGoDefence;

	public void Init(FarmInput farmInput,
		ProductDatabase productDatabase,
		Action onBattleButtonClicked,
		Action onQuitRequested,
		Action<ProductEntry, float, Vector2, Vector2> playScreenHarvestAnimation,
		Func<bool> canGoDefence)
	{
		farmInput.RegisterInputLayer(_cropGuide);
		
		_battleButton.onClick.AddListener(() => onBattleButtonClicked());
		// TODO 실제 설정창 구현하기
		_settingButton.onClick.AddListener(() => onQuitRequested());
		_harvestInventory.Init(productDatabase, playScreenHarvestAnimation);

		_canGoDefence = canGoDefence;
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
		_harvestInventory = GetComponentInChildren<HarvestInventory>();
		_cropGuide = GetComponentInChildren<CropGuide>();
		_battleButton = transform.Find("BattleButton").GetComponent<Button>();
		_settingButton = transform.Find("SettingButton").GetComponent<Button>();
		ResourceManager.Instance.OnGoldChanged += _coinsUI.SetCoin;
	}

	private void Start()
	{
		_coinsUI.SetCoin(ResourceManager.Instance.Gold);
	}

	private void Update()
	{
		_battleButton.interactable = _canGoDefence();
	}
}
