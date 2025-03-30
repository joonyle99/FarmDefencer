using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 0번 자식: 배경
/// 1번 자식: 잠금 디스플레이
/// </summary>
public sealed class Field : MonoBehaviour, IFarmUpdatable
{
	[SerializeField] private GameObject cropPrefab;
    [SerializeField] private ProductEntry productEntry;
    public ProductEntry ProductEntry => productEntry;
    [SerializeField] private Vector2Int fieldSize;
    public Vector2Int FieldSize => fieldSize;
    [SerializeField] private GameObject cropBackgroundPrefab;

    public bool IsAvailable
    {
        get
        {
            return _isAvailable;
        }
        set
        {
            _isAvailable = value;
            OnAvailabilityChanged();
        }
    }
    private bool _isAvailable;

    public Crop[] Crops => _crops;
    private Crop[] _crops;
    private SpriteRenderer _backgroundRenderer; // 0번 자식 오브젝트에 할당
    private SpriteRenderer _fieldLockedRenderer; // 1번 자식 오브젝트에 할당

    public void Init(Func<ProductEntry, int> getQuotaFunction, Action<ProductEntry, Vector2, int> fillQuotaFunction)
    {
        Array.ForEach(
            _crops,
            crop => crop.Init(() => getQuotaFunction(ProductEntry),
            (count) =>
            {
                if (count > 0)
                {
                    fillQuotaFunction(productEntry, crop.transform.position, count);
                }
            }));
	}

	/// <summary>
	/// 입력된 좌표에 해당되는 Crop을 검색해서 반환합니다.
	/// </summary>
	/// <param name="position"></param>
	/// <param name="crop"></param>
	/// <returns>position에 해당하는 Crop이 존재할 경우 crop에 값이 할당되며 true 반환, 이외의 경우 crop에 null이 할당되며 false 반환</returns>
	public bool TryFindCropAt(Vector2 position, out Crop crop)
	{
        var localPosition = new Vector2Int(Mathf.RoundToInt(position.x - transform.position.x), Mathf.RoundToInt(position.y - transform.position.y));
        if (localPosition.x < 0 || localPosition.x >= fieldSize.x
            || localPosition.y < 0 || localPosition.y >= fieldSize.y)
        {
            crop = null;
            return false;
        }

        crop = _crops[localPosition.y * fieldSize.x + localPosition.x];
		return true;
	}

    public void OnFarmUpdate(float deltaTime)
    {
        foreach (var crop in _crops)
        {
            crop.OnFarmUpdate(deltaTime);
        }
    }

	private void OnAvailabilityChanged()
    {
        var backgroundColor = _isAvailable ? Color.white : new Color(0.0f, 0.0f, 0.0f, 0.0f);
        var fieldLockedColor = _isAvailable ? new Color(0.0f, 0.0f, 0.0f, 0.0f) : Color.white;

        _backgroundRenderer.color = backgroundColor;
        _fieldLockedRenderer.color = fieldLockedColor;
	}

	private void Awake()
    {
        _crops = new Crop[fieldSize.x * fieldSize.y];
        _backgroundRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        _fieldLockedRenderer = transform.GetChild(1).GetComponent<SpriteRenderer>();

        var cropBackgroundParentObject = new GameObject();
        cropBackgroundParentObject.transform.parent = transform;
        cropBackgroundParentObject.name = "CropBackgrounds";
        cropBackgroundParentObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        
        for (int yOffset = 0; yOffset < fieldSize.y; ++yOffset)
        {
            for (int xOffset = 0; xOffset < fieldSize.x; ++xOffset)
            {
                var cropObject = Instantiate(cropPrefab, transform);

                var cropComponent = cropObject.GetComponent<Crop>();
                cropObject.transform.localPosition = new Vector3(xOffset, yOffset, 0.0f);

                _crops[yOffset * fieldSize.x + xOffset] = cropComponent;

                var cropBackgroundObject = Instantiate(cropBackgroundPrefab, cropBackgroundParentObject.transform);
                cropBackgroundObject.transform.localPosition = new Vector3(xOffset, yOffset, 0.0f);
            }
        }

        OnAvailabilityChanged();
	}
}
