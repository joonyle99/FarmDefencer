using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HarvestInventory : MonoBehaviour
{
	public HarvestAnimationPlayer HarvestAnimationPlayer;
	public ProductDatabase ProductDatabase;
	private Dictionary<string, HarvestBox> _harvestBoxes;

	/// <summary>
	/// 해당 농작물에 대한 획득 애니메이션을 재생합니다.
	/// 애니메이션 종료 시 callback을 호출합니다.
	/// </summary>
	/// <param name="productEntry"></param>
	/// <param name="count"></param>
	/// <param name="cropScreenPosition">애니메이션의 시작 화면 위치</param>
	public void PlayHarvestAnimation(ProductEntry productEntry, int count, Vector2 cropScreenPosition, UnityAction callback)
	{
		var harvestBox = _harvestBoxes[productEntry.UniqueId];
		var toPosition = harvestBox.ScreenPosition;
		HarvestAnimationPlayer.PlayAnimation(productEntry, cropScreenPosition, toPosition, callback);
	}

	/// <summary>
	/// 해당 농작물의 개수를 증가시킵니다.
	/// 내부적으로 ResourceManager의 Gold를 증가시키는 메소드를 호출합니다.
	/// </summary>
	/// <param name="productEntry"></param>
	/// <param name="count"></param>
	public void AddProduct(ProductEntry productEntry, int count)
	{
		var harvestBox = _harvestBoxes[productEntry.UniqueId];
		var price = productEntry.Price * count;
		harvestBox.Count += count;
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
		if (harvestBox.Count < count)
		{
			return false;
		}

		harvestBox.Count -= count;
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
