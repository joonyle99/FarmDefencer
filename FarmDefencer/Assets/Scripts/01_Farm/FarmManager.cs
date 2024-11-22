using FarmTest;
using UnityEngine;

/// <summary>
/// Ÿ���� �� �ε� ������ ������ �����ϰ�, ���� ������Ʈ ���� �ݹ� ���� �� ������ ������ ����ϵ��� ����� Ŭ�����Դϴ�.
/// ���� �̺�Ʈ ���� �ݹ� ���� �帧 �ľ��� ��Ʊ� ������ �ִ��� �� Ŭ������ �߰����� ���踦 �����ϵ��� �մϴ�.
/// </summary>
public class FarmManager : MonoBehaviour
{
	public HarvestInventory HarvestInventory;
    public FarmClock FarmClock;
    public Farm Farm;
	public FarmTestPlayer FarmTestPlayer;
	public ProductDatabase ProductDatabase;

	private void Awake()
	{
		FarmClock.RegisterFarmUpdatableObject(Farm);
		Farm.OnTryItemify.AddListener(
			(productEntry, cropWorldPosition, afterItemifyCallback) =>
			{
				var cropScreenPosition = FarmTestPlayer.Camera.WorldToScreenPoint(new Vector3(cropWorldPosition.x, cropWorldPosition.y, 0)); 
				var isItemified = HarvestInventory.TryBeginGather(productEntry, cropScreenPosition);
				afterItemifyCallback(isItemified);
			});
	}

	private void Start()
	{
		HarvestInventory.SetTodaysOrder(new System.Collections.Generic.List<(string, int)>{ ("product_carrot", 5), ("product_potato", 5), ("product_corn", 5) });
	}
}
