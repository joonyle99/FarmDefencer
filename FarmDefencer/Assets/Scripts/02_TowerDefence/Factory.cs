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
/// 오브젝트 풀링 최적화 기법을 사용하여 가비지 컬렉션을 최소화합니다.
/// </remarks>
public class Factory : MonoBehaviour
{
    [Header("━━━━━━━━ Factory ━━━━━━━━")]
    [Space]

    // 팩토리에서 생성하기 위한 오브젝트 목록 (확장성을 고려하기 위함)
    [SerializeField] private List<GameObject> _productPrefabList = new List<GameObject>();
    [SerializeField] private GameObject _productPrefab;
    [SerializeField] private string _productName = "Product";
    [SerializeField] private int _poolCapacity = 100;

    [Space]

    [SerializeField] private List<GameObject> _pool = new List<GameObject>();
    public List<GameObject> Pool => _pool;

    private GameObject _lastProduct;

    private void Awake()
    {
        //InitializePool();
    }
    private void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        _pool = new List<GameObject>(_poolCapacity);

        // 5 / 100 : 20 -> 40 -> 60 -> 80 -> 100
        var unit = _poolCapacity / _productPrefabList.Count;
        Debug.Log("unit: " + unit);

        // 오브젝트 생성 후, 풀에 추가
        foreach (var prefab in _productPrefabList)
        {
            // 프리팹을 종류별로 일정량씩 생성
            for (int i = 0; i < unit; ++i)
            {
                var newObj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                newObj.name = _productName + " " + prefab.name + " " + (i + 1).ToString();
                newObj.transform.SetParent(transform, false);
                newObj.SetActive(false);
                newObj.tag = prefab.name;

                AddProduct(newObj);

                _lastProduct = newObj;
            }
        }
        //for (int i = 0; i < _poolCapacity; ++i)
        //{
        //    var newObj = Instantiate(_productPrefab, Vector3.zero, Quaternion.identity);

        //    newObj.name = _productName + " " + (i + 1).ToString();
        //    newObj.transform.SetParent(transform, false);
        //    newObj.SetActive(false);

        //    AddProduct(newObj);

        //    _lastProduct = newObj;
        //}
    }
    private void ExtendPool()
    {
        // 기존 풀 용량의 절반만큼 확장
        var halfOfCapacity = _poolCapacity / 2;
        _poolCapacity += halfOfCapacity;

        for (int i = 1; i <= halfOfCapacity; i++)
        {
            // 마지막 오브젝트의 이름에서 숫자를 추출해 네임 태그 생성
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

    public T GetProduct<T>(System.Type type) where T : IProduct
    {
        // 꺼낼 오브젝트가 없는 경우 풀을 확장한다
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
