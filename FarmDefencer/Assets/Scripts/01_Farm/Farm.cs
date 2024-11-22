using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ��� �۹���� ������ ������ �ǹ��ϴ� Ŭ�����Դϴ�.
/// FieldPrefabs�� ������ ������ �����մϴ�.
/// <br/>
/// �ܺ��� ��� ��ü���� Field, Crop ��� ���� �������� �ʵ���(�� �ʿ� ������) �����Ͽ����ϴ�.
/// �� Farm�� �̺�Ʈ�� public �޼ҵ�鸸 ����Ͽ� �ٸ� �ý����� �����ϵ��� �ּ����� API�� �����Ͽ����ϴ�.
/// </summary>
public class Farm : MonoBehaviour, IFarmUpdatable
{
    public List<GameObject> FieldPrefabs;
    
    /// <summary>
    /// <seealso cref="Field.OnTryItemify"/>�� �����ϼ���.
    /// </summary>
    public UnityEvent<ProductEntry, Vector2Int, UnityAction<bool>> OnTryItemify;

    private Dictionary<string, Field> _fields;

	/// <summary>
	/// �ش� ���� ��ǥ�� Crop�� �˻��մϴ�.
	/// Crop�� �ܺ� �ý����� ���� ��ȣ�ۿ��� ���� ���� ������, <b>����׿����θ� ����� ���� �����մϴ�. </b><br/><br/>
	/// <seealso cref="Field.TryFindCropAt(Vector2, out Crop)"/>�� �ش� ��ġ�� �����ϴ� Field�� ���� ȣ���ϴ� �����Դϴ�.
	/// </summary>
	/// <param name="position"></param>
	/// <param name="crop"></param>
	/// <returns></returns>
	public bool TryFindCropAt(Vector2 position, out Crop crop)
	{
		foreach (var (_, field) in _fields)
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
        foreach (var (_, field) in _fields)
        {
			if (!field.IsAvailable)
			{
				continue;
			}
			if (field.TryFindCropAt(position, out var crop))
			{
                crop.OnTap();
			}
		}
    }

    public void HoldingAction(Vector2 position, float holdTime)
    {
		foreach (var (_, field) in _fields)
		{
			if (!field.IsAvailable)
			{
				continue;
			}
			if (field.TryFindCropAt(position, out var crop))
			{
                crop.OnHolding(holdTime);
			}
		}
	}

	public void WateringAction(Vector2 position)
	{
		foreach (var (_, field) in _fields)
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
        foreach (var (_, field) in _fields)
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
		if (!_fields.TryGetValue(productUniqueId, out var field))
		{
			Debug.LogWarning($"Farm.GetFieldAvailability()�� ���ڷ� ���޵� productUniqueId {productUniqueId}(��)�� Farm._field�� �������� �ʽ��ϴ�.");
			return false;
		}

		return field.IsAvailable;
	}

	public void SetAvailability(string productUniqueId, bool value)
	{
		if (!_fields.TryGetValue(productUniqueId, out var field))
		{
			Debug.LogError($"Farm.SetAvailability()�� ���ڷ� ���޵� productUniqueId {productUniqueId}(��)�� Farm._field�� �������� �ʽ��ϴ�.");
			return;
		}

		field.IsAvailable = value;
	}

	private void Awake()
    {
        _fields = new Dictionary<string, Field>();

        for (int index = 0; index<FieldPrefabs.Count; index++)
        {
            var fieldPrefab = FieldPrefabs[index];
            if (fieldPrefab == null
                || !fieldPrefab.TryGetComponent<Field>(out var _))
            {
                throw new System.ArgumentException("Farm�� FieldPrefabs�� None ������Ʈ �Ǵ� Field ������Ʈ�� ���� �ʴ� ������Ʈ�� �����մϴ�.");
            }

            var fieldObject = Instantiate(fieldPrefab);
            var fieldComponent = fieldObject.GetComponent<Field>();
            fieldObject.transform.parent = transform;
			fieldObject.transform.localPosition = new Vector3(fieldComponent.FieldLocalPosition.x, fieldComponent.FieldLocalPosition.y, transform.position.z - 1.0f);
            fieldComponent.OnTryItemify.AddListener(OnTryItemify.Invoke);
            _fields.Add(fieldComponent.ProductEntry.UniqueId, fieldComponent);
        }
    }
}
