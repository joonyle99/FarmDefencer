using FarmTest;
using UnityEngine;

/// <summary>
/// 타이쿤 씬 로드 직후의 동작을 정의하고, 여러 오브젝트 간의 콜백 구성 및 의존성 주입을 담당하도록 설계된 클래스입니다.
/// 잦은 이벤트 사용과 콜백 사용시 흐름 파악이 어렵기 때문에 최대한 이 클래스가 중간에서 관계를 구성하도록 합니다.
/// </summary>
public class FarmManager : MonoBehaviour
{
	public FarmUI FarmUI;
    public FarmClock FarmClock;
    public Farm Farm;
	public FarmTestPlayer FarmTestPlayer;
	public ProductDatabase ProductDatabase;
	public FarmSoundManager FarmSoundManager;

	public void SetAvailability(string productUniqueId, bool value)
	{
		Farm.SetFieldAvailability(productUniqueId, value);
		FarmUI.HarvestInventory.SetHarvestBoxAvailability(productUniqueId, value);
	}

	public bool GetAvailability(string productUniqueId) => FarmUI.HarvestInventory.GetHarvestBoxAvailability(productUniqueId);

	private void Awake()
	{
		FarmClock.RegisterFarmUpdatableObject(Farm);

		FarmUI.Init(FarmClock);
		FarmUI.WateringCan.OnWatering.AddListener(Farm.WateringAction);
	}

	private void Start()
	{
		Farm.HarvestHandler = FarmUI.HarvestInventory.Gather;

		SetAvailability("product_carrot", true);
		SetAvailability("product_potato", true);
		SetAvailability("product_corn", true);
		SetAvailability("product_cabbage", true);
		//SetAvailability("product_cucumber", true);
		//SetAvailability("product_eggplant", true);
		//SetAvailability("product_sweetpotato", true);
		//SetAvailability("product_mushroom", true);

		FarmUI.HarvestInventory.SetTodaysOrder(
			new System.Collections.Generic.List<(string, int)>
			{
				("product_carrot", 99),
				("product_potato", 99), 
				("product_corn", 99), 
				("product_cabbage", 99), 
				//("product_cucumber", 99), 
				//("product_eggplant", 99), 
				//("product_sweetpotato", 99), 
				//("product_mushroom", 99),
			});
	}

	private void Update()
	{
		var ratio = FarmClock.LengthOfDaytime == 0.0f ? 0.0f : FarmClock.RemainingDaytime / FarmClock.LengthOfDaytime;
		FarmUI.TimerUI.SetClockhand(ratio);
	}
}
