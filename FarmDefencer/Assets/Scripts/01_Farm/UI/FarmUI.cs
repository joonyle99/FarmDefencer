using UnityEngine;

public class FarmUI : MonoBehaviour
{
	private CoinsUI _coinsUI;
	public CoinsUI CoinsUI => _coinsUI;
	private HarvestAnimationPlayer _harvestAnimationPlayer;
	public HarvestAnimationPlayer HarvestAnimationPlayer  => _harvestAnimationPlayer;
	private WateringCan _wateringCan;
	public WateringCan WateringCan => _wateringCan;
	private HarvestInventory _harvestInventory;
	public HarvestInventory HarvestInventory => _harvestInventory;

	private void Awake()
	{
		_coinsUI = GetComponentInChildren<CoinsUI>();
		_harvestAnimationPlayer = GetComponentInChildren<HarvestAnimationPlayer>();
		_wateringCan = GetComponentInChildren<WateringCan>();
		_harvestInventory = GetComponentInChildren<HarvestInventory>();

		_coinsUI.UpdateCoinText(ResourceManager.Instance.Gold);
		ResourceManager.Instance.OnGoldChanged += _coinsUI.UpdateCoinText;
	}
}
