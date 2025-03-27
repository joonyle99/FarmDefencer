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

	private void Awake()
	{
		farmClock.RegisterFarmUpdatableObject(farm);

		farmUI.Init(farmClock);
		farmUI.WateringCan.Water += farm.WateringAction;

		cropSigns.SignClicked += farmUI.CropGuide.Toggle;
	}

	private void Start()
	{
		farmInput.RegisterInputLayer(farmUI.CropGuide);
		farmInput.RegisterInputLayer(farmUI.WateringCan);
		farmInput.RegisterInputLayer(cropSigns);
		farmInput.RegisterInputLayer(farm);

		farm.Init(farmUI.HarvestInventory.GetQuota, farmUI.HarvestInventory.Gather);

		farmUI.HarvestInventory.SetHarvestBoxAvailability("product_carrot", true);
		farm.SetFieldAvailability("product_carrot", true);
		farmUI.HarvestInventory.SetHarvestBoxAvailability("product_potato", true);
		farm.SetFieldAvailability("product_potato", true);
		farmUI.HarvestInventory.SetHarvestBoxAvailability("product_corn", true);
		farm.SetFieldAvailability("product_corn", true);
		farmUI.HarvestInventory.SetHarvestBoxAvailability("product_cabbage", true);
		farm.SetFieldAvailability("product_cabbage", true);
		farmUI.HarvestInventory.SetHarvestBoxAvailability("product_cucumber", true);
		farm.SetFieldAvailability("product_cucumber", true);
		farmUI.HarvestInventory.SetHarvestBoxAvailability("product_eggplant", true);
		farm.SetFieldAvailability("product_eggplant", true);
		farmUI.HarvestInventory.SetHarvestBoxAvailability("product_sweetpotato", true);
		farm.SetFieldAvailability("product_sweetpotato", true);
		farmUI.HarvestInventory.SetHarvestBoxAvailability("product_mushroom", true);
		farm.SetFieldAvailability("product_mushroom", true);

		farmUI.HarvestInventory.SetTodaysOrder(
			new System.Collections.Generic.List<(string, int)>
			{
				("product_carrot", 99),
				("product_potato", 99), 
				("product_corn", 99), 
				("product_cabbage", 99), 
				("product_cucumber", 99), 
				("product_eggplant", 99), 
				("product_sweetpotato", 99), 
				("product_mushroom", 99),
			});
	}

	private void Update()
	{
		var ratio = farmClock.LengthOfDaytime == 0.0f ? 0.0f : farmClock.RemainingDaytime / farmClock.LengthOfDaytime;
		farmUI.TimerUI.SetClockhand(ratio);
	}
}
