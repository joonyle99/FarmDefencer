using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 자식으로는 반드시 Field 오브젝트만 가지게 할 것.
/// </summary>
public class Farm : MonoBehaviour, IFarmUpdatable
{
	private Func<ProductEntry, Vector2, int, int> _harvestHandler;
	public Func<ProductEntry, Vector2, int, int> HarvestHandler
	{
		get
		{
			return _harvestHandler;
		}
		set
		{
			_harvestHandler = value;
			foreach (var field in _fields)
			{
				field.HarvestHandler = value;
			}
		}
	}

	private bool _isFarmPaused;
	private List<Field> _fields;

	/// <summary>
	/// 해당 월드 좌표의 Crop을 검색합니다.
	/// Crop과 외부 시스템이 직접 상호작용할 일은 없기 때문에, <b>디버그용으로만 사용할 것을 권장합니다. </b><br/><br/>
	/// <seealso cref="Field.TryFindCropAt(Vector2, out Crop)"/>을 해당 위치를 포함하는 Field에 대해 호출하는 동작입니다.
	/// </summary>
	/// <param name="position"></param>
	/// <param name="crop"></param>
	/// <returns></returns>
	public bool TryFindCropAt(Vector2 position, out Crop crop)
	{
		foreach (var field in _fields)
		{
			if (field.TryFindCropAt(position, out crop))
			{
				return true;
			}
		}

		crop = null;
		return false;
	}

	public void TapAction(Vector2 position)
    {
		if (_isFarmPaused)
		{
			return;
		}

		foreach (var field in _fields)
        {
			if (!field.IsAvailable)
			{
				continue;
			}
			if (field.TryFindCropAt(position, out var crop))
			{
                crop.OnSingleTap();
			}
		}
    }

    public void SingleHoldingAction(Vector2 initialPosition, Vector2 deltaPosition, bool isEnd, float deltaHoldTime)
    {
		if (_isFarmPaused)
		{
			return;
		}
		foreach (var field in _fields)
		{
			if (!field.IsAvailable)
			{
				continue;
			}
			if (field.TryFindCropAt(initialPosition, out var crop))
			{
				crop.OnSingleHolding(deltaPosition, isEnd, deltaHoldTime);
			}
		}
	}

	public void WateringAction(Vector2 position)
	{
		foreach (var field in _fields)
		{
			if (!field.IsAvailable)
			{
				continue;
			}
			if (field.TryFindCropAt(position, out var crop))
			{
				crop.OnWatering();
			}
		}
	}

	public void OnFarmUpdate(float deltaTime)
    {
		_isFarmPaused = deltaTime == 0.0f;
        foreach (var field in _fields)
        {
			if (!field.IsAvailable)
			{
				continue;
			}
			field.OnFarmUpdate(deltaTime);
        }
    }

	public bool GetFieldAvailability(string productUniqueId)
	{
		foreach (var field in _fields)
		{
			if (field.ProductEntry.UniqueId == productUniqueId)
			{
				return field.IsAvailable;
			}
		}

		Debug.LogError($"Farm이 {productUniqueId}에 해당하는 Field를 가지고 있지 않습니다.");
		return false;
	}

	public void SetFieldAvailability(string productUniqueId, bool value)
	{
		foreach (var field in _fields)
		{
			if (field.ProductEntry.UniqueId == productUniqueId)
			{
				field.IsAvailable = value;
				return;
			}
		}

		Debug.LogError($"Farm이 {productUniqueId}에 해당하는 Field를 가지고 있지 않습니다.");
	}

	private void Awake()
    {
		_fields = new List<Field>();

		for (int childIndex = 0; childIndex < transform.childCount; ++childIndex)
		{
			var childObject = transform.GetChild(childIndex);
			var fieldComponent = childObject.GetComponent<Field>();
			fieldComponent.HarvestHandler = HarvestHandler;

			_fields.Add(fieldComponent);
		}
    }
}
