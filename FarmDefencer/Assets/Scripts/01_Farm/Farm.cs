using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Farm : MonoBehaviour, IFarmUpdatable
{
    public List<GameObject> FieldPrefabs;
    
    /// <summary>
    /// <seealso cref="Field.OnTryItemify"/>를 참조하세요.
    /// </summary>
    public UnityEvent<ProductEntry, Vector2Int, UnityAction<bool>> OnTryItemify;

    private Field[] _fields;

    /// <summary>
    /// <seealso cref="Field.TryFindCropAt(Vector2, out Crop)"/>을 해당 위치를 포함하는 Field에 대해 호출합니다.
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
