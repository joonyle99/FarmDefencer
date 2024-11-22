using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    /// <seealso cref="Field.OnTryItemify"/>를 참조하세요.
    /// </summary>
    public UnityEvent<ProductEntry, Vector2Int, UnityAction<bool>> OnTryItemify;

    private Field[] _fields;

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
        foreach (var field in _fields)
        {
			if (field.TryFindCropAt(position, out var crop))
			{
                crop.OnTap();
			}
		}
    }

    public void HoldingAction(Vector2 position, float holdTime)
    {
		foreach (var field in _fields)
		{
			if (field.TryFindCropAt(position, out var crop))
			{
                crop.OnHolding(holdTime);
			}
		}
	}

	public void WateringAction(Vector2 position)
	{
		foreach (var field in _fields)
		{
			if (field.TryFindCropAt(position, out var crop))
			{
				crop.OnWatering();
			}
		}
	}

	public void OnFarmUpdate(float deltaTime)
    {
        foreach (var field in _fields)
        {
            field.OnFarmUpdate(deltaTime);
        }
    }

	private void Awake()
    {
        _fields = new Field[FieldPrefabs.Count];

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
            fieldComponent.OnTryItemify.AddListener(OnTryItemify.Invoke);
            _fields[index] = fieldComponent;
        }
    }
}
