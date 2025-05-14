using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 자식으로 HarvestAnimationPlayer 오브젝트를 가져야 함.
/// </summary>
public sealed class HarvestInventory : MonoBehaviour
{
	private ProductDatabase _productDatabase;
	private HarvestAnimationPlayer _harvestAnimationPlayer;
	private Dictionary<ProductEntry, HarvestBox> _harvestBoxes;

	public void UpdateInventory(QuotaContext context)
	{
		foreach (var productEntry in _productDatabase.Products)
		{
			var isAvailable = context.IsProductAvailable(productEntry);
			_harvestBoxes[productEntry].IsAvailable = isAvailable;
			if (!isAvailable)
			{
				_harvestBoxes[productEntry].Quota = 0;
				continue;
			}

			if (!context.TryGetQuota(productEntry.ProductName, out var quota))
			{
				Debug.LogError($"QuotaContext에서 {productEntry.ProductName}에 해당하는 주문량 정보를 가져오지 못했습니다.");
				_harvestBoxes[productEntry].Quota = 0;
				continue;
			}

			_harvestBoxes[productEntry].Quota = quota;
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
	public void PlayProductFillAnimation(ProductEntry productEntry, Vector2 cropWorldPosition, int count)
	{
		var cropScreenPosition = Camera.main.WorldToScreenPoint(cropWorldPosition);
		StartCoroutine(HarvestAnimationCoroutine(productEntry, cropScreenPosition, count));
	}

	public void Init(ProductDatabase database)
	{
		_productDatabase = database;
		foreach (var entry in _productDatabase.Products)
		{
			var harvestBoxTransform = transform.Find($"BoxArea/HarvestBox_{entry.ProductName}");
			if (harvestBoxTransform is null)
			{
				Debug.LogError($"HarvestInventory/BoxArea의 자식중에 HarvestBox_{entry.ProductName}가 필요합니다.");
				continue;
			}
			
			if (!harvestBoxTransform.TryGetComponent<HarvestBox>(out var harvestBox))
			{
				Debug.LogError($"{harvestBoxTransform.gameObject.name}(은)는 HarvestBox 컴포넌트를 갖지 않습니다.");
				continue;
			}
			_harvestBoxes.Add(entry, harvestBox);
		}
	}
	
	private void Awake()
	{
		_harvestAnimationPlayer = GetComponentInChildren<HarvestAnimationPlayer>();
		_harvestBoxes = new();
	}

	private System.Collections.IEnumerator HarvestAnimationCoroutine(ProductEntry productEntry, Vector2 cropScreenPosition, int count)
	{
		var harvestBox = _harvestBoxes[productEntry];
		var toPosition = harvestBox.ScreenPosition;

		for (var i = 0; i < count; i++)
		{
			_harvestAnimationPlayer.PlayAnimation(productEntry, cropScreenPosition, toPosition, () => { });
			yield return new WaitForSeconds(0.1f);
		}
		yield return null;
	}
}
