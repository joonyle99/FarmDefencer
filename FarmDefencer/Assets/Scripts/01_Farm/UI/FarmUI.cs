using UnityEngine;

public sealed class FarmUI : MonoBehaviour
{
	private CoinsUI _coinsUI;
	public CoinsUI CoinsUI => _coinsUI;
	private HarvestAnimationPlayer _harvestAnimationPlayer;
	public HarvestAnimationPlayer HarvestAnimationPlayer  => _harvestAnimationPlayer;
	private WateringCan _wateringCan;
	public WateringCan WateringCan => _wateringCan;
	private HarvestInventory _harvestInventory;
	public HarvestInventory HarvestInventory => _harvestInventory;

	public TimerUI TimerUI => _timerUI;
	private TimerUI _timerUI;

	public CropGuide CropGuide => _cropGuide;
	private CropGuide _cropGuide;

	public FarmDebugUI FarmDebugUI => _farmDebugUI;
	private FarmDebugUI _farmDebugUI;

	public void Init(FarmClock farmClock)
	{
		_wateringCan.Init(farmClock);
		_farmDebugUI.Init(farmClock);
	}

	private void Awake()
	{
		_coinsUI = GetComponentInChildren<CoinsUI>();
		_harvestAnimationPlayer = GetComponentInChildren<HarvestAnimationPlayer>();
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
