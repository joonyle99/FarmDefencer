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
	}

	private void Start()
	{
		SetAvailability("product_carrot", true);
		SetAvailability("product_potato", true);
		SetAvailability("product_corn", true);
		SetAvailability("product_cabbage", false);
		SetAvailability("product_cucumber", false);
		SetAvailability("product_eggplant", false);
		SetAvailability("product_sweetpotato", false);
		SetAvailability("product_mushroom", false);

		FarmUI.HarvestInventory.SetTodaysOrder(new System.Collections.Generic.List<(string, int)>{ ("product_carrot", 5), ("product_potato", 5), ("product_corn", 5) });
	}
}
