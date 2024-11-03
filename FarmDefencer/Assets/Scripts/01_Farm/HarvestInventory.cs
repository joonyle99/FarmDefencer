using System.Collections.Generic;
using UnityEngine;

public class HarvestInventory : MonoBehaviour
{
	public Camera Camera;
	public HarvestAnimationPlayer HarvestAnimationPlayer;
	public ProductDatabase ProductDatabase;
	private Dictionary<string, HarvestBox> _harvestBoxes;

	/// <summary>
	/// �ش� ���۹��� ���� ȹ�� �ִϸ��̼��� ����ϸ� ������ ������ŵ�ϴ�.
	/// </summary>
	/// <param name="productEntry"></param>
	/// <param name="count"></param>
	/// <param name="cropScreenPosition">�ִϸ��̼��� ���� ȭ�� ��ġ</param>
	public void AddProduct(ProductEntry productEntry, int count, Vector2 cropScreenPosition)
	{
		var harvestBox = _harvestBoxes[productEntry.UniqueId];
		var fromPosition = Camera.WorldToScreenPoint(cropScreenPosition);
		var toPosition = harvestBox.ScreenPosition;
		HarvestAnimationPlayer.PlayAnimation(productEntry, fromPosition, toPosition, () => AddProduct(productEntry, count));
	}

	/// <summary>
	/// �ش� ���۹��� ������ ������ŵ�ϴ�.
	/// </summary>
	/// <param name="productEntry"></param>
	/// <param name="count"></param>
	public void AddProduct(ProductEntry productEntry, int count)
	{
		var harvestBox = _harvestBoxes[productEntry.UniqueId];
		harvestBox.Count += count;
	}

	/// <summary>
	/// �ش� ���۹��� ������ ���ҽ�ŵ�ϴ�.
	/// �ش� ������ŭ �����ϰ� ���� ������ false�� ��ȯ�մϴ�.
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

	public void OnHarvest(ProductEntry productEntry, Vector2Int position)
	{
		AddProduct(productEntry, 1, position);
	}

	private void Awake()
	{
		_harvestBoxes = new Dictionary<string, HarvestBox>();
		foreach (var entry in ProductDatabase.Products)
		{
			var harvestBoxTransform = transform.Find($"BoxArea/HarvestBox_{entry.UniqueId}");
			if (harvestBoxTransform == null)
			{
				Debug.LogError($"HarvestInventory/BoxArea�� �ڽ��߿� HarvestBox_{entry.UniqueId}�� �ʿ��մϴ�.");
				continue;
			}
			
			if (!harvestBoxTransform.TryGetComponent<HarvestBox>(out var harvestBox))
			{
				Debug.LogError($"{harvestBoxTransform.gameObject.name}(��)�� HarvestBox ������Ʈ�� ���� �ʽ��ϴ�.");
				continue;
			}
			_harvestBoxes.Add(entry.UniqueId, harvestBox);
		}
	}
}
