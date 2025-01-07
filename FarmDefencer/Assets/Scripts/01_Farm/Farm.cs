using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

/// <summary>
/// 밭과 작물들로 구성된 농장을 의미하는 클래스입니다.
/// FieldPrefabs를 설정해 농장을 구성합니다.
/// <br/>
/// 외부의 모든 객체들은 Field, Crop 등과 직접 소통하지 않도록(알 필요 없도록) 설계하였습니다.
/// 즉 Farm의 이벤트와 public 메소드들만 고려하여 다른 시스템을 설계하도록 최소한의 API만 노출하였습니다.
/// </summary>
public class Farm : MonoBehaviour, IFarmUpdatable
{
    public List<GameObject> FieldPrefabs;

	/// <summary>
	/// 작물을 아이템화 시도할 때 호출되는 이벤트입니다. 
	/// 아이템화란 작물로부터 수확 상자에 담기는 행동을 의미합니다.
	/// <br/>
	/// OnTryItemify&lt;productEntry, cropWorldPosition, afterItemify(isItemified)&gt;로 구성되며, 
	/// 이를 처리하는 핸들러는 인자 콜백 afterItemify에 대해 아이템화에 성공했는지 여부를 매개 변수로 전달하여 다시 호출하면 됩니다.
	/// 할당량을 초과해서 수확할 수 없기 때문에, 이를 검증하기 위한 이중 콜백 구조입니다.
	/// <br/><br/>
	/// 즉, OnTryItemify를 처리하는 측에서는 여유 공간이 있다면 afterItemify(true), 없다면 afterItemify(false) 하면 됩니다.
	/// </summary>
	public UnityEvent<ProductEntry, Vector2Int, UnityAction<bool>> OnTryItemify;
	public GameObject FieldLockedDisplayPrefab;
	public GameObject CropLockedDisplayPrefab;
	public TileBase FlowedTile;

    private Dictionary<string, Field> _fields;
	private bool _isFarmPaused;

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
			Debug.LogWarning($"Farm.GetFieldAvailability()의 인자로 전달된 productUniqueId {productUniqueId}(은)는 Farm._field에 존재하지 않습니다.");
			return false;
		}

		return field.IsAvailable;
	}

	public void SetFieldAvailability(string productUniqueId, bool value)
	{
		if (!_fields.TryGetValue(productUniqueId, out var field))
		{
			Debug.LogError($"Farm.SetAvailability()의 인자로 전달된 productUniqueId {productUniqueId}(은)는 Farm._field에 존재하지 않습니다.");
			return;
		}

		field.IsAvailable = value;
	}

	private void Awake()
    {
        _fields = new Dictionary<string, Field>();

		if (CropLockedDisplayPrefab == null || !CropLockedDisplayPrefab.TryGetComponent<CropLockedDisplay>(out var _))
		{
			throw new System.ArgumentException("Farm의 CropLockedDisplayPrefab None 오브젝트 또는 CropLockedDisplay 컴포넌트를 갖지 않는 오브젝트가 존재합니다.");
		}

        for (int index = 0; index<FieldPrefabs.Count; index++)
        {
            var fieldPrefab = FieldPrefabs[index];
            if (fieldPrefab == null
                || !fieldPrefab.TryGetComponent<Field>(out var _))
            {
                throw new System.ArgumentException("Farm의 FieldPrefabs에 None 오브젝트 또는 Field 컴포넌트를 갖지 않는 오브젝트가 존재합니다.");
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
