using UnityEngine;
using JoonyleGameDevKit;
using System.Collections.Generic;

public interface IProduct
{
    public Factory OriginFactory { get; set; }
    public void SetOriginFactory(Factory originFactory);
}

/// <summary>
/// 
/// </summary>
/// <remarks>
/// ������Ʈ Ǯ�� ����ȭ ����� ����Ͽ� ������ �÷����� �ּ�ȭ�մϴ�.
/// </remarks>
public class Factory : MonoBehaviour
{
    [Header("���������������� Factory ����������������")]
    [Space]

    // ���丮���� �����ϱ� ���� ������Ʈ ���
    // [SerializeField] private List<GameObject> _productPrefabList = new List<GameObject>();

    [SerializeField] private GameObject _productPrefab;
    [SerializeField] private string _productName = "Product";
    [SerializeField] private int _poolCapacity = 100;

    [Space]

    [SerializeField] private List<GameObject> _pool = new List<GameObject>();
    public List<GameObject> Pool => _pool;

    private GameObject _lastProduct;

    private void Awake()
    {
        CreatePool();
    }

    private void CreatePool()
    {
        // ������Ʈ ���� ��, Ǯ�� �߰�
        for (int i = 0; i < _poolCapacity; ++i)
        {
            var newObj = Instantiate(_productPrefab, Vector3.zero, Quaternion.identity);

            newObj.gameObject.name = _productName + " " + (i + 1).ToString();
            newObj.gameObject.SetActive(false);

            AddProduct(newObj);

            _lastProduct = newObj;
        }
    }
    private void ExtendPool()
    {
        // ���� Ǯ �뷮�� ���ݸ�ŭ Ȯ��
        var halfOfCapacity = _poolCapacity / 2;
        _poolCapacity += halfOfCapacity;

        for (int i = 1; i <= halfOfCapacity; i++)
        {
            // ������ ������Ʈ�� �̸����� ���ڸ� ������ ���� �±� ����
            var lastNumber = _lastProduct.name.ExtractNumber();
            var nameTag = (lastNumber + 1).ToString();

            CreateNewProduct(nameTag);
        }
    }
    private void CreateNewProduct(string nameTag)
    {
        var newObj = Instantiate(_productPrefab, Vector3.zero, Quaternion.identity);

        newObj.gameObject.name = _productName + " " + nameTag + " (Extended)";
        newObj.gameObject.SetActive(false);

        AddProduct(newObj);

        _lastProduct = newObj;
    }

    public T GetProduct<T>() where T : Component
    {
        // ���� ������Ʈ�� ���� ��� Ǯ�� Ȯ���Ѵ�
        if (IsEmptyPool())
            ExtendPool();

        var newObj = RemoveLast();
        newObj.SetActive(true);
        return newObj.GetComponent<T>();
    }
    public void ReturnProduct(Component component)
    {
        var oldObj = component.gameObject;
        AddProduct(oldObj);
        oldObj.SetActive(false);
    }

    protected virtual void AddProduct(GameObject obj)
    {
        Pool.Add(obj);
    }
    protected virtual GameObject RemoveLast()
    {
        var obj = Pool[^1];
        Pool.RemoveAt(Pool.Count - 1);
        return obj;
    }

    public virtual bool IsEmptyPool()
    {
        return Pool.Count == 0;
    }
}
