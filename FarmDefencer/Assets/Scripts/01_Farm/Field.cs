using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 0번 자식: 배경
/// 1번 자식: 잠금 디스플레이
/// </summary>
public class Field : MonoBehaviour, IFarmUpdatable
{
	public GameObject CropPrefab;

    public ProductEntry ProductEntry;
    public Vector2Int FieldSize;

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
                    fillQuotaFunction(ProductEntry, crop.transform.position, count);
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
        if (localPosition.x < 0 || localPosition.x >= FieldSize.x
            || localPosition.y < 0 || localPosition.y >= FieldSize.y)
        {
            crop = null;
            return false;
        }

        crop = _crops[localPosition.y * FieldSize.x + localPosition.x];
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
        _crops = new Crop[FieldSize.x * FieldSize.y];
        _backgroundRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        _fieldLockedRenderer = transform.GetChild(1).GetComponent<SpriteRenderer>();

        for (int yOffset = 0; yOffset < FieldSize.y; ++yOffset)
        {
            for (int xOffset = 0; xOffset < FieldSize.x; ++xOffset)
            {
                var cropObject = Instantiate(CropPrefab, transform);

                var cropComponent = cropObject.GetComponent<Crop>();
                cropObject.transform.localPosition = new Vector3(xOffset, yOffset, 0.0f);

                _crops[yOffset * FieldSize.x + xOffset] = cropComponent;
            }
        }

        OnAvailabilityChanged();
	}
}
