using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Farm : MonoBehaviour, IFarmUpdatable
{
    public List<GameObject> FieldPrefabs;
    
    /// <summary>
    /// <seealso cref="Field.OnTryItemify"/>�� �����ϼ���.
    /// </summary>
    public UnityEvent<ProductEntry, Vector2Int, UnityAction<bool>> OnTryItemify;

    private Field[] _fields;

    /// <summary>
    /// <seealso cref="Field.TryFindCropAt(Vector2, out Crop)"/>�� �ش� ��ġ�� �����ϴ� Field�� ���� ȣ���մϴ�.
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
                throw new System.ArgumentException("Farm�� FieldPrefabs�� None ������Ʈ �Ǵ� Field ������Ʈ�� ���� �ʴ� ������Ʈ�� �����մϴ�.");
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
