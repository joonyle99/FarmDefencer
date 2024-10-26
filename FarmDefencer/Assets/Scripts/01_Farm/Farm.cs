using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

public class Farm : MonoBehaviour
{
    public List<GameObject> FieldPrefabs;

    private List<Field> _fields;

    /// <summary>
    /// 입력된 좌표에 해당되는 Crop을 검색해서 반환합니다.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="crop"></param>
    /// <returns>worldPosition에 해당하는 Crop이 존재할 경우 crop에 값이 할당되며 true 반환, 이외의 경우 crop에 null이 할당되며 false 반환</returns>
    public bool TryFindCropAt(Vector2 position, [CanBeNull] out Crop crop)
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
