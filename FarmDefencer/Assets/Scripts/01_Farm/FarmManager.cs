using FarmTest;
using UnityEngine;

/// <summary>
/// Ÿ���� �� �ε� ������ ������ �����ϰ�, ���� ������Ʈ ���� �ݹ� ���� �� ������ ������ ����ϵ��� ����� Ŭ�����Դϴ�.
/// ���� �̺�Ʈ ���� �ݹ� ���� �帧 �ľ��� ��Ʊ� ������ �ִ��� �� Ŭ������ �߰����� ���踦 �����ϵ��� �մϴ�.
/// </summary>
public class FarmManager : MonoBehaviour
{
	public FarmUI FarmUI;
    public FarmClock FarmClock;
    public Farm Farm;
	public FarmTestPlayer FarmTestPlayer;
	public ProductDatabase ProductDatabase;

	public void SetAvailability(string productUniqueId, bool value)
	{
		Farm.SetFieldAvailability(productUniqueId, value);
		FarmUI.HarvestInventory.SetHarvestBoxAvailability(productUniqueId, value);
	}

	public bool GetAvailability(string productUniqueId) => FarmUI.HarvestInventory.GetHarvestBoxAvailability(productUniqueId);

	private void Awake()
	{
		FarmClock.RegisterFarmUpdatableObject(Farm);
		Farm.OnTryItemify.AddListener(
			(productEntry, cropWorldPosition, afterItemifyCallback) =>
			{
				var cropScreenPosition = FarmTestPlayer.Camera.WorldToScreenPoint(new Vector3(cropWorldPosition.x, cropWorldPosition.y, 0)); 
				var isItemified = FarmUI.HarvestInventory.TryBeginGather(productEntry, cropScreenPosition);
				afterItemifyCallback(isItemified);
			});
		FarmUI.Init(FarmClock);
	}

	private void Start()
	{
		SetAvailability("product_carrot", true);
		SetAvailability("product_potato", true);
		SetAvailability("product_corn", true);
		SetAvailability("product_cabbage", true);
		SetAvailability("product_cucumber", true);
		SetAvailability("product_eggplant", true);
		SetAvailability("product_sweetpotato", true);
		SetAvailability("product_mushroom", true);

		FarmUI.HarvestInventory.SetTodaysOrder(
			new System.Collections.Generic.List<(string, int)>
			{
				("product_carrot", 5),
				("product_potato", 5), 
				("product_corn", 5), 
				("product_cabbage", 5), 
				("product_cucumber", 5), 
				("product_eggplant", 5), 
				("product_sweetpotato", 5), 
				("product_mushroom", 5),
			});
	}

	private void Update()
	{
		var ratio = FarmClock.LengthOfDaytime == 0.0f ? 0.0f : FarmClock.RemainingDaytime / FarmClock.LengthOfDaytime;
		FarmUI.TimerUI.SetClockhand(ratio);
	}
}
