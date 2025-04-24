using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 타이쿤의 최초 로드 동작을 정의하고, 타이쿤을 구성하는 여러 시스템들간의 중재자 역할을 하는 컴포넌트.
/// </summary>
public sealed class FarmManager : MonoBehaviour
{
	[SerializeField] private CropSigns cropSigns;
	[SerializeField] private FarmUI farmUI;
	[SerializeField] private FarmClock farmClock;
	[SerializeField] private Farm farm;
	[SerializeField] private FarmInput farmInput;
	[SerializeField] private ProductDatabase productDatabase;
	[SerializeField] private PenaltyGiver penaltyGiver;
	[SerializeField] private QuotaContext quotaContext;

	private void Start()
	{
		cropSigns.SignClicked += farmUI.ToggleCropGuide;
		
		farmInput.RegisterInputLayer(cropSigns);
		farmInput.RegisterInputLayer(farm);
		farmInput.FullZoomOut();

		farmClock.RegisterFarmUpdatableObject(farm);
		farmClock.RegisterFarmUpdatableObject(penaltyGiver);
		farmClock.AddPauseCondition(() => penaltyGiver.IsAnimationPlaying);
		
		farm.Init(
			entry => quotaContext.TryGetQuota(entry.ProductName, out var quota) ? quota : 0,
			(entry, cropWorldPosition, quota) =>
			{
				quotaContext.FillQuota(entry.ProductName, quota);
				farmUI.PlayProductFillAnimation(entry, cropWorldPosition, quota, quotaContext);
				SoundManager.PlaySfxStatic("SFX_T_coin");
				ResourceManager.Instance.Gold += entry.Price * quota;
			});
		farmUI.Init(farmClock, farmInput, farm.WateringAction, productDatabase);
		penaltyGiver.Init(farm);

		quotaContext.QuotaContextUpdated += QuotaContextChangedHandler;
		quotaContext.AssignQuotas(MapManager.Instance.CurrentMap.MapId);

		var currentMap = MapManager.Instance.CurrentMap;
		var monsters = new List<string>();
		monsters.Add("Rabbit");
		monsters.Add("Rabbit");
		monsters.Add("Rabbit");
		monsters.Add("Rabbit");
		monsters.Add("Rabbit");
		monsters.Add("Rabbit");
		monsters.Add("Squirrel");
		monsters.Add("Squirrel");
		monsters.Add("Squirrel");
		monsters.Add("Squirrel");
		monsters.Add("Squirrel");
		monsters.Add("Cat");
		monsters.Add("Cat");
		monsters.Add("Cat");
		monsters.Add("Cat");
		monsters.Add("Cat");
		monsters.Add("Cat");
		monsters.Add("Capybara");
		monsters.Add("Capybara");
		monsters.Add("Capybara");
		monsters.Add("Capybara");
		monsters.Add("Capybara");
		monsters.Add("Capybara");
		monsters.Add("Elephant");
		monsters.Add("Elephant");
		monsters.Add("Elephant");
		monsters.Add("Elephant");
		monsters.Add("Elephant");
		monsters.Add("Elephant");
		penaltyGiver.SpawnMonsters(currentMap.MapId, monsters);
		
	}

	private void Update()
	{
		var ratio = farmClock.LengthOfDaytime == 0.0f ? 0.0f : farmClock.RemainingDaytime / farmClock.LengthOfDaytime;
		farmUI.SetTimerClockHand(ratio);
	}

	private void QuotaContextChangedHandler()
	{
		farmUI.UpdateHarvestInventory(quotaContext);
		farm.UpdateAvailability(quotaContext);

		if (quotaContext.IsAllQuotaFilled)
		{
			quotaContext.AssignQuotas(MapManager.Instance.CurrentMap.MapId);
		}
	}
}
