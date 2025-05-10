using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 타이쿤의 최초 로드 동작을 정의하고, 타이쿤을 구성하는 여러 시스템들간의 중재자 역할을 하는 컴포넌트.
/// </summary>
public sealed class FarmManager : MonoBehaviour
{
	[SerializeField] private FarmUI farmUI;
	[SerializeField] private FarmClock farmClock;
	[SerializeField] private Farm farm;
	[SerializeField] private FarmInput farmInput;
	[SerializeField] private ProductDatabase productDatabase;
	[SerializeField] private PenaltyGiver penaltyGiver;
	[SerializeField] private HarvestTutorialGiver harvestTutorialGiver;
	[SerializeField] private QuotaContext quotaContext;

	private void Start()
	{
		farmInput.RegisterInputLayer(farm);
		farmInput.RegisterInputLayer(harvestTutorialGiver);

		farmClock.RegisterFarmUpdatableObject(farm);
		farmClock.RegisterFarmUpdatableObject(penaltyGiver);
		farmClock.AddPauseCondition(() => penaltyGiver.IsAnimationPlaying);
		farmClock.AddPauseCondition(() => harvestTutorialGiver.IsPlayingTutorial);
		
		farm.Init(
			entry => quotaContext.TryGetQuota(entry.ProductName, out var quota) ? quota : 0,
			(entry, cropWorldPosition, quota) =>
			{
				quotaContext.FillQuota(entry.ProductName, quota);
				farmUI.PlayProductFillAnimation(entry, cropWorldPosition, quota, quotaContext);
				SoundManager.PlaySfxStatic("SFX_T_coin");
				ResourceManager.Instance.Gold += entry.Price * quota;
			},
			farmUI.ToggleCropGuide);
		
		farmUI.Init(farmClock, farmInput, farm.WateringAction, productDatabase);
		penaltyGiver.Init(farm);
		
		quotaContext.QuotaContextUpdated += QuotaContextChangedHandler;
		quotaContext.AssignQuotas(MapManager.Instance.CurrentMap.MapId);
		
		// harvestTutorialGiver.AddTutorial("product_carrot");
		//harvestTutorialGiver.AddTutorial("product_potato");
		//harvestTutorialGiver.AddTutorial("product_corn");
		//harvestTutorialGiver.AddTutorial("product_cabbage");
		//harvestTutorialGiver.AddTutorial("product_cucumber");
		//harvestTutorialGiver.AddTutorial("product_eggplant");
		harvestTutorialGiver.AddTutorial("product_sweetpotato");
		harvestTutorialGiver.AddTutorial("product_mushroom");
}

	private void Update()
	{
		var ratio = farmClock.LengthOfDaytime == 0.0f ? 0.0f : farmClock.RemainingDaytime / farmClock.LengthOfDaytime;
		farmUI.SetTimerClockHand(ratio);

		if (harvestTutorialGiver.IsPlayingTutorial)
		{
			harvestTutorialGiver.gameObject.SetActive(!penaltyGiver.IsAnimationPlaying);
			farmInput.InputPriorityCut = harvestTutorialGiver.InputPriority;
		}
		else
		{
			farmInput.InputPriorityCut = 0;
		}

		farmUI.WateringCanAvailable = !harvestTutorialGiver.gameObject.activeSelf;
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
