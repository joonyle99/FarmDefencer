using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HarvestInventory : MonoBehaviour
{
	public HarvestAnimationPlayer HarvestAnimationPlayer;
	public ProductDatabase ProductDatabase;
	private Dictionary<string, HarvestBox> _harvestBoxes;

	/// <summary>
	/// 오늘의 주문을 설정합니다.
	/// </summary>
	/// <param name="quotas"></param>
	public void SetTodaysOrder(List<(string, int)> quotas)
	{
		foreach (var pair in quotas)
		{
			var productUniqueId = pair.Item1;
			var count = pair.Item2;

			if (!_harvestBoxes.TryGetValue(productUniqueId, out var harvestBox))
			{
				Debug.LogError($"HarvestInventory에 {productUniqueId}를 담는 HarvestBox가 존재하지 않습니다.");
				continue;
			}

			harvestBox.Quota = count;
		}
	}

	/// <summary>
	/// 해당 엔트리에 대해 작물 획득 애니메이션을 재생하며 개수를 증가시킵니다.
	/// 구체적으로는 애니메이션 종료 시 FillQuota(productEntry, 1)을 호출하게 합니다.
	/// 만약 해당 농작물이 이미 할당량을 다 채운 경우 false를 반환하며 아무 작업도 하지 않습니다.
	/// </summary>
	/// <param name="productEntry"></param>
	/// <param name="cropScreenPosition">애니메이션의 시작 화면 위치</param>
	public bool TryBeginGather(ProductEntry productEntry, Vector2 cropScreenPosition)
	{
		var harvestBox = _harvestBoxes[productEntry.UniqueId];
		if (harvestBox.Quota == 0)
		{
			return false;
		}

		var toPosition = harvestBox.ScreenPosition;
		HarvestAnimationPlayer.PlayAnimation(productEntry, cropScreenPosition, toPosition, () => FillQuota(productEntry, 1));

		return true;
	}

	/// <summary>
	/// 해당 농작물의 할당량을 임의로 채웁니다.
	/// 내부적으로 ResourceManager의 Gold를 증가시키는 메소드를 호출합니다.
	/// <b>직접 호출하는 것은 디버그 목적으로만 사용해야 합니다.</b> 정상적인 게임 흐름은 TryBeginGather()를 이용하세요.
	/// </summary>
	/// <param name="productEntry"></param>
	/// <param name="count"></param>
	public void FillQuota(ProductEntry productEntry, int count)
	{
		var harvestBox = _harvestBoxes[productEntry.UniqueId];
		var price = productEntry.Price * count;
		harvestBox.Quota -= count;
		ResourceManager.Instance.EarnGold(price);
	}

	/// <summary>
	/// 해당 농작물의 개수를 감소시킵니다.
	/// 해당 개수만큼 보유하고 있지 않으면 false를 반환합니다.
	/// </summary>
	/// <param name="productEntry"></param>
	/// <param name="count"></param>
	public bool TryMinusProduct(ProductEntry productEntry, int count)
	{
		var harvestBox = _harvestBoxes[productEntry.UniqueId];
		if (harvestBox.Quota < count)
		{
			return false;
		}

		harvestBox.Quota -= count;
		return true;
	}


	private void Awake()
	{
		_harvestBoxes = new Dictionary<string, HarvestBox>();
		foreach (var entry in ProductDatabase.Products)
		{
			var harvestBoxTransform = transform.Find($"BoxArea/HarvestBox_{entry.UniqueId}");
			if (harvestBoxTransform == null)
			{
				Debug.LogError($"HarvestInventory/BoxArea의 자식중에 HarvestBox_{entry.UniqueId}가 필요합니다.");
				continue;
			}
			
			if (!harvestBoxTransform.TryGetComponent<HarvestBox>(out var harvestBox))
			{
				Debug.LogError($"{harvestBoxTransform.gameObject.name}(은)는 HarvestBox 컴포넌트를 갖지 않습니다.");
				continue;
			}
			_harvestBoxes.Add(entry.UniqueId, harvestBox);
		}
	}
}
