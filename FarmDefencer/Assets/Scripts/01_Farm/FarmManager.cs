using UnityEngine;

/// <summary>
/// 타이쿤의 최초 로드 동작을 정의하고, 타이쿤을 구성하는 여러 시스템들간의 중재자 역할을 하는 컴포넌트.
/// </summary>
public class FarmManager : MonoBehaviour
{
	[SerializeField] private CropSigns _cropSigns;
	[SerializeField] private FarmUI _farmUI;
	[SerializeField] private FarmClock _farmClock;
	[SerializeField] private Farm _farm;
	[SerializeField] private FarmInput _farmInput;
	[SerializeField] private ProductDatabase _productDatabase;

	private void Awake()
	{
		_farmClock.RegisterFarmUpdatableObject(_farm);

		_farmUI.Init(_farmClock);
		_farmUI.WateringCan.Water += _farm.WateringAction;

		_cropSigns.SignClicked += _farmUI.CropGuide.Toggle;
	}

	private void Start()
	{
		_farmInput.RegisterInputLayer(_farmUI.CropGuide);
		_farmInput.RegisterInputLayer(_farmUI.WateringCan);
		_farmInput.RegisterInputLayer(_cropSigns);
		_farmInput.RegisterInputLayer(_farm);

		_farm.Init(_farmUI.HarvestInventory.GetQuota, _farmUI.HarvestInventory.Gather);

		_farmUI.HarvestInventory.SetHarvestBoxAvailability("product_carrot", true);
		_farm.SetFieldAvailability("product_carrot", true);
		_farmUI.HarvestInventory.SetHarvestBoxAvailability("product_potato", true);
		_farm.SetFieldAvailability("product_potato", true);
		_farmUI.HarvestInventory.SetHarvestBoxAvailability("product_corn", true);
		_farm.SetFieldAvailability("product_corn", true);
		_farmUI.HarvestInventory.SetHarvestBoxAvailability("product_cabbage", true);
		_farm.SetFieldAvailability("product_cabbage", true);
		_farmUI.HarvestInventory.SetHarvestBoxAvailability("product_cucumber", true);
		_farm.SetFieldAvailability("product_cucumber", true);
		_farmUI.HarvestInventory.SetHarvestBoxAvailability("product_eggplant", true);
		_farm.SetFieldAvailability("product_eggplant", true);
		_farmUI.HarvestInventory.SetHarvestBoxAvailability("product_sweetpotato", true);
		_farm.SetFieldAvailability("product_sweetpotato", true);
		_farmUI.HarvestInventory.SetHarvestBoxAvailability("product_mushroom", true);
		_farm.SetFieldAvailability("product_mushroom", true);

		_farmUI.HarvestInventory.SetTodaysOrder(
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
		var ratio = _farmClock.LengthOfDaytime == 0.0f ? 0.0f : _farmClock.RemainingDaytime / _farmClock.LengthOfDaytime;
		_farmUI.TimerUI.SetClockhand(ratio);
	}
}
