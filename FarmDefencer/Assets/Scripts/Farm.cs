using System.Collections.Generic;
using UnityEngine;

public class Farm : MonoBehaviour
{
    public List<GameObject> FieldPrefabs;

    private GameObject _fieldsObject; // field 인스턴스들을 자식 오브젝트로 가지는, farm의 자식 오브젝트

    private void Awake()
    {
        _fieldsObject = transform.Find("Fields").gameObject;

        foreach (var fieldPrefab in FieldPrefabs)
        {
            if (fieldPrefab == null
                || !fieldPrefab.TryGetComponent<Field>(out var _))
            {
                Debug.LogError("Farm의 FieldPrefabs에 None 오브젝트 또는 Field 컴포넌트를 갖지 않는 오브젝트가 존재합니다.");
                continue;
            }

            var fieldInstance = Instantiate(fieldPrefab, _fieldsObject.transform);
            var fieldComponent = fieldInstance.GetComponent<Field>();
            fieldInstance.transform.localPosition = (Vector3Int)fieldComponent.FieldPosition;
        }
    }
}
