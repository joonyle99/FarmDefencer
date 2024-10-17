using System.Collections.Generic;
using UnityEngine;

public class Farm : MonoBehaviour
{
    public List<GameObject> FieldPrefabs;

    private GameObject _fieldsObject; // field �ν��Ͻ����� �ڽ� ������Ʈ�� ������, farm�� �ڽ� ������Ʈ

    private void Awake()
    {
        _fieldsObject = transform.Find("Fields").gameObject;

        foreach (var fieldPrefab in FieldPrefabs)
        {
            if (fieldPrefab == null
                || !fieldPrefab.TryGetComponent<Field>(out var _))
            {
                Debug.LogError("Farm�� FieldPrefabs�� None ������Ʈ �Ǵ� Field ������Ʈ�� ���� �ʴ� ������Ʈ�� �����մϴ�.");
                continue;
            }

            var fieldInstance = Instantiate(fieldPrefab, _fieldsObject.transform);
            var fieldComponent = fieldInstance.GetComponent<Field>();
            fieldInstance.transform.localPosition = (Vector3Int)fieldComponent.FieldPosition;
        }
    }
}
