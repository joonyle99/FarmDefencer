using UnityEngine;
using JoonyleGameDevKit;
using System.Collections.Generic;

public interface IProduct
{
    public Factory OriginFactory { get; set; }
    public void SetOriginFactory(Factory originFactory);

    public GameObject GameObject { get; }
    public Transform Transform { get; }
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

    // ���丮���� �����ϱ� ���� ������Ʈ ��� (Ȯ�强�� ����ϱ� ����)
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
        InitializePool();
    }

    private void InitializePool()
    {
        _pool = new List<GameObject>(_poolCapacity);

        // ������Ʈ ���� ��, Ǯ�� �߰�
        for (int i = 0; i < _poolCapacity; ++i)
        {
            var newObj = Instantiate(_productPrefab, Vector3.zero, Quaternion.identity);

            newObj.name = _productName + " " + (i + 1).ToString();
            newObj.transform.SetParent(transform, false);
            newObj.SetActive(false);

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

        newObj.name = _productName + " " + nameTag + " (Extended)";
        newObj.transform.SetParent(transform, false);
        newObj.SetActive(false);

        AddProduct(newObj);

        _lastProduct = newObj;
    }

    public T GetProduct<T>() where T : IProduct
    {
        // ���� ������Ʈ�� ���� ��� Ǯ�� Ȯ���Ѵ�
        if (IsEmptyPool())
            ExtendPool();

        var newObj = RemoveLastProduct();
        newObj.SetActive(true);
        var product = newObj.GetComponent<T>();
        product.SetOriginFactory(this);

        return product;
    }
    public void ReturnProduct(IProduct product)
    {
        product.GameObject.SetActive(false);
        AddProduct(product.GameObject);
    }

    protected void AddProduct(GameObject obj)
    {
        Pool.Add(obj);
    }
    protected GameObject RemoveLastProduct()
    {
        if (IsEmptyPool())
        {
            return null;
        }

        var obj = Pool[Pool.Count - 1];
        Pool.RemoveAt(Pool.Count - 1);
        return obj;
    }

    public bool IsEmptyPool()
    {
        return Pool.Count == 0;
    }
}
