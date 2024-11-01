using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Farm : MonoBehaviour
{
    public List<GameObject> FieldPrefabs;
    public FarmClock FarmClock;
    [Tooltip("ProductEntry�� �װ��� ��Ȯ�� ��ġ�� Vector2Int�� �ǹ��մϴ�.")]
    public UnityEvent<ProductEntry, Vector2Int> OnHarvest;

    private List<Field> _fields;

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

    private void Awake()
    {
        _fields = new List<Field>();

        foreach (var fieldPrefab in FieldPrefabs)
        {
            if (fieldPrefab == null
                || !fieldPrefab.TryGetComponent<Field>(out var _))
            {
                Debug.LogError("Farm�� FieldPrefabs�� None ������Ʈ �Ǵ� Field ������Ʈ�� ���� �ʴ� ������Ʈ�� �����մϴ�.");
                continue;
            }

            var fieldObject = Instantiate(fieldPrefab);
            var fieldComponent = fieldObject.GetComponent<Field>();
            fieldObject.transform.parent = transform;
			fieldObject.transform.localPosition = new Vector3(fieldComponent.FieldLocalPosition.x, fieldComponent.FieldLocalPosition.y, transform.position.z - 1.0f);
            fieldComponent.FarmClock = FarmClock;
            fieldComponent.OnHarvest = OnHarvest;
            _fields.Add(fieldComponent);
        }
    }
}
