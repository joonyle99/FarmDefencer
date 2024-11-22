using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HarvestInventory : MonoBehaviour
{
	public HarvestAnimationPlayer HarvestAnimationPlayer;
	public ProductDatabase ProductDatabase;
	private Dictionary<string, HarvestBox> _harvestBoxes;

	/// <summary>
	/// ������ �ֹ��� �����մϴ�.
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
				Debug.LogError($"HarvestInventory�� {productUniqueId}�� ��� HarvestBox�� �������� �ʽ��ϴ�.");
				continue;
			}

			harvestBox.Quota = count;
		}
	}

	/// <summary>
	/// �ش� ��Ʈ���� ���� �۹� ȹ�� �ִϸ��̼��� ����ϸ� ������ ������ŵ�ϴ�.
	/// ��ü�����δ� �ִϸ��̼� ���� �� FillQuota(productEntry, 1)�� ȣ���ϰ� �մϴ�.
	/// ���� �ش� ���۹��� �̹� �Ҵ緮�� �� ä�� ��� false�� ��ȯ�ϸ� �ƹ� �۾��� ���� �ʽ��ϴ�.
	/// </summary>
	/// <param name="productEntry"></param>
	/// <param name="cropScreenPosition">�ִϸ��̼��� ���� ȭ�� ��ġ</param>
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
	/// �ش� ���۹��� �Ҵ緮�� ���Ƿ� ä��ϴ�.
	/// ���������� ResourceManager�� Gold�� ������Ű�� �޼ҵ带 ȣ���մϴ�.
	/// <b>���� ȣ���ϴ� ���� ����� �������θ� ����ؾ� �մϴ�.</b> �������� ���� �帧�� TryBeginGather()�� �̿��ϼ���.
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
	/// �ش� ���۹��� ������ ���ҽ�ŵ�ϴ�.
	/// �ش� ������ŭ �����ϰ� ���� ������ false�� ��ȯ�մϴ�.
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
