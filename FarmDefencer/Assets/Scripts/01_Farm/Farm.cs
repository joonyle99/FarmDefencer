using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

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
	/// �۹��� ������ȭ �õ��� �� ȣ��Ǵ� �̺�Ʈ�Դϴ�. 
	/// ������ȭ�� �۹��κ��� ��Ȯ ���ڿ� ���� �ൿ�� �ǹ��մϴ�.
	/// <br/>
	/// OnTryItemify&lt;productEntry, cropWorldPosition, afterItemify(isItemified)&gt;�� �����Ǹ�, 
	/// �̸� ó���ϴ� �ڵ鷯�� ���� �ݹ� afterItemify�� ���� ������ȭ�� �����ߴ��� ���θ� �Ű� ������ �����Ͽ� �ٽ� ȣ���ϸ� �˴ϴ�.
	/// �Ҵ緮�� �ʰ��ؼ� ��Ȯ�� �� ���� ������, �̸� �����ϱ� ���� ���� �ݹ� �����Դϴ�.
	/// <br/><br/>
	/// ��, OnTryItemify�� ó���ϴ� �������� ���� ������ �ִٸ� afterItemify(true), ���ٸ� afterItemify(false) �ϸ� �˴ϴ�.
	/// </summary>
	public UnityEvent<ProductEntry, Vector2Int, UnityAction<bool>> OnTryItemify;
	public GameObject FieldLockedDisplayPrefab;
	public GameObject CropLockedDisplayPrefab;
	public TileBase FlowedTile;

    private Dictionary<string, Field> _fields;
	private bool _isFarmPaused;

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
		if (_isFarmPaused)
		{
			return;
		}

        foreach (var (_, field) in _fields)
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

    public void SingleHoldingAction(Vector2 initialPosition, Vector2 deltaPosition, bool isEnd, float holdTime)
    {
		if (_isFarmPaused)
		{
			return;
		}


		foreach (var (_, field) in _fields)
		{
			if (!field.IsAvailable)
			{
				continue;
			}
			if (field.TryFindCropAt(initialPosition, out var crop))
			{
                crop.OnSingleHolding(deltaPosition, isEnd, holdTime);
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
		_isFarmPaused = deltaTime == 0.0f;
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

	public void SetFieldAvailability(string productUniqueId, bool value)
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

		if (CropLockedDisplayPrefab == null || !CropLockedDisplayPrefab.TryGetComponent<CropLockedDisplay>(out var _))
		{
			throw new System.ArgumentException("Farm�� CropLockedDisplayPrefab None ������Ʈ �Ǵ� CropLockedDisplay ������Ʈ�� ���� �ʴ� ������Ʈ�� �����մϴ�.");
		}

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
			fieldComponent.Init(
				FieldLockedDisplayPrefab,
				CropLockedDisplayPrefab,
				FlowedTile,
				(productEntry, cropPosition, afterItemifyCallback) => OnTryItemify.Invoke(productEntry, cropPosition, afterItemifyCallback));
            _fields.Add(fieldComponent.ProductEntry.UniqueId, fieldComponent);
        }
    }
}
