using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HarvestInventory : MonoBehaviour
{
	public UnityEvent OnGoldEarned;
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
	/// 해당 엔트리에 대해 count만큼 획득 처리하며, 작물 획득 애니메이션을 재생합니다.
	/// 만약 해당 농작물이 이미 할당량을 다 채운 경우 false를 반환하며 아무 작업도 하지 않습니다.
	/// count가 남은 개수보다 많은 경우에도 일단은 0개로 만듭니다(예: 오늘의 수확이 2개 남은 상황에서 3개 작물 수확할 경우, 3개를 모두 수확하지만 2개만 획득 처리).
	/// </summary>
	/// <param name="productEntry"></param>
	/// <param name="cropScreenPosition">애니메이션의 시작 화면 위치</param>
	/// <param name="count"></param>
	public bool TryBeginGather(ProductEntry productEntry, Vector2 cropScreenPosition, int count)
	{
		var harvestBox = _harvestBoxes[productEntry.UniqueId];
		if (harvestBox.Quota == 0)
		{
			return false;
		}

		count = Mathf.Min(count, harvestBox.Quota);
		FillQuota(productEntry, count);
		StartCoroutine(HarvestAnimationCoroutine(productEntry, cropScreenPosition, count)); // 일단은 그대로 호출

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
		OnGoldEarned.Invoke();
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

	public bool GetHarvestBoxAvailability(string productUniqueId)
	{
		if (!_harvestBoxes.TryGetValue(productUniqueId, out var harvestBox))
		{
			Debug.LogWarning($"Farm.GetHarvestBoxAvailability()의 인자로 전달된 productUniqueId {productUniqueId}(은)는 HarvestInventory._harvestBoxes에 존재하지 않습니다.");
			return false;
		}

		return harvestBox.IsAvailable;
	}

	public void SetHarvestBoxAvailability(string productUniqueId, bool value)
	{
		if (!_harvestBoxes.TryGetValue(productUniqueId, out var harvestBox))
		{
			Debug.LogError($"Farm.SetHarvestBoxAvailability()의 인자로 전달된 productUniqueId {productUniqueId}(은)는 HarvestInventory._harvestBoxes에 존재하지 않습니다.");
			return;
		}

		harvestBox.IsAvailable = value;
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

	private System.Collections.IEnumerator HarvestAnimationCoroutine(ProductEntry productEntry, Vector2 cropScreenPosition, int count)
	{
		var harvestBox = _harvestBoxes[productEntry.UniqueId];
		var toPosition = harvestBox.ScreenPosition;

		for (var i = 0; i < count; i++)
		{
			HarvestAnimationPlayer.PlayAnimation(productEntry, cropScreenPosition, toPosition, () => { });
			yield return new WaitForSeconds(0.1f);
		}
		yield return null;
	}
}
