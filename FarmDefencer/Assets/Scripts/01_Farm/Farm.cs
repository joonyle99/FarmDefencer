using System.Collections.Generic;
using UnityEngine;

public class Farm : MonoBehaviour
{
    public List<GameObject> FieldPrefabs;

    private List<Field> _fields;

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

    /// <summary>
    /// <seealso cref="Field.TryFindCropAt{T}(Vector2, out T)"/>을 해당 위치를 포함하는 Field에 대해 호출합니다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="position"></param>
    /// <param name="specializedCrop"></param>
    /// <returns></returns>
    public bool TryFindCropAt<T>(Vector2 position, out T specializedCrop) where T : class
    {
        foreach (var field in _fields)
        {
            if (field.TryFindCropAt<T>(position, out specializedCrop))
            {
                return true;
            }
        }

        specializedCrop = null;
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
                Debug.LogError("Farm의 FieldPrefabs에 None 오브젝트 또는 Field 컴포넌트를 갖지 않는 오브젝트가 존재합니다.");
                continue;
            }

            var fieldObject = Instantiate(fieldPrefab);
            var fieldComponent = fieldObject.GetComponent<Field>();
            fieldObject.transform.parent = transform;
			fieldObject.transform.localPosition = new Vector3(fieldComponent.FieldLocalPosition.x, fieldComponent.FieldLocalPosition.y, transform.position.z - 1.0f);
            _fields.Add(fieldComponent);
        }
    }
}
