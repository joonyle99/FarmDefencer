using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 자식으로 HarvestAnimationPlayer 오브젝트를 가져야 함.
/// </summary>
public class HarvestInventory : MonoBehaviour
{
	private HarvestAnimationPlayer _harvestAnimationPlayer;
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
	/// 요청한 개수와 남은 주문량 중 낮은 값만큼 수확 처리하는 메소드.
	/// 내부적으로 수확 애니메이션 재생 코루틴을 실행함.
	/// </summary>
	/// <remarks>
	/// count가 지금 주문량보다 많으면 에러. 반드시 GetQuota()로 사전 확인할 것.
	/// </remarks>
	/// <param name="productEntry"></param>
	/// <param name="cropWorldPosition"></param>
	/// <param name="count"></param>
	public void Gather(ProductEntry productEntry, Vector2 cropWorldPosition, int count)
	{
		var harvestBox = _harvestBoxes[productEntry.UniqueId];
		
		if (count > harvestBox.Quota)
		{
			throw new System.ArgumentOutOfRangeException($"ProductEntry {productEntry.UniqueId}의 남은 주문량 {harvestBox.Quota}보다 많은 개수인 {count}만큼 Gather() 시도했습니다.");
		}

		var profit = productEntry.Price * count;
		harvestBox.Quota -= count;
		ResourceManager.Instance.EarnGold(profit);
		SoundManager.PlaySfxStatic("SFX_T_coin");

		var cropScreenPosition = Camera.main.WorldToScreenPoint(cropWorldPosition);

		StartCoroutine(HarvestAnimationCoroutine(productEntry, cropScreenPosition, count));
	}

	public int GetQuota(ProductEntry productEntry) => _harvestBoxes[productEntry.UniqueId].Quota;

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
		_harvestAnimationPlayer = GetComponentInChildren<HarvestAnimationPlayer>();
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
			_harvestAnimationPlayer.PlayAnimation(productEntry, cropScreenPosition, toPosition, () => { });
			yield return new WaitForSeconds(0.1f);
		}
		yield return null;
	}
}
