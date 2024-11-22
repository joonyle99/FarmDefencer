using FarmTest;
using UnityEngine;

/// <summary>
/// 타이쿤 씬 로드 직후의 동작을 정의하고, 여러 오브젝트 간의 콜백 구성 및 의존성 주입을 담당하도록 설계된 클래스입니다.
/// 잦은 이벤트 사용과 콜백 사용시 흐름 파악이 어렵기 때문에 최대한 이 클래스가 중간에서 관계를 구성하도록 합니다.
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
