using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 0번 자식: 배경
/// 1번 자식: 잠금 디스플레이
/// </summary>
public sealed class Field : MonoBehaviour, IFarmUpdatable, IFarmInputLayer
{
    public int InputPriority => 0; // 직접적으로 FarmInput에 등록되지 않고, Farm에 의존하므로 무시해도 됨
    
    [SerializeField] private GameObject cropPrefab;
    [SerializeField] private ProductEntry productEntry;
    
    private Action _onCropSignClicked;
    
    public ProductEntry ProductEntry => productEntry;
    [SerializeField] private Vector2Int fieldSize;
    public Vector2Int FieldSize => fieldSize;
    [SerializeField] private GameObject cropBackgroundPrefab;

    private List<SpriteRenderer> _cropBackgrounds;
    private CropSign _cropSign;
    private HashSet<Crop> _lockedCrops;

    private Crop[] Crops => _crops;
    private Crop[] _crops;
    private SpriteRenderer _backgroundRenderer; // 0번 자식 오브젝트에 할당
    private SpriteRenderer _fieldLockedRenderer; // 1번 자식 오브젝트에 할당
    
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

    public Crop TopLeftCrop => _crops[FieldSize.x * (FieldSize.y - 1)];

    public bool AllLocked => Array.TrueForAll(_crops, c => _lockedCrops.Contains(c));
    
    public bool TryLockCropAt(Vector2 cropPosition)
    {
        if (TryFindCropAt(cropPosition, out var crop))
        {
            if (!_lockedCrops.Contains(crop))
            {
                crop.ResetToInitialState();
                crop.gameObject.SetActive(false);
            }

            return _lockedCrops.Add(crop);
        }

        return false;
    }

    public bool IsCropLockable(Vector2 cropPosition) =>
        TryFindCropAt(cropPosition, out var crop) && !_lockedCrops.Contains(crop);

    public bool TryGetFirstLockableCropPosition(out Vector2 cropPosition)
    {
        var firstCrop = Array.Find(_crops, c => !_lockedCrops.Contains(c));
        cropPosition = firstCrop is null ? Vector2.zero : firstCrop.transform.position;
        return firstCrop is not null;
    }

    public bool AABB(Vector2 worldPosition)
    {
        // 우선 표지판 AABB
        if (SignAABB(worldPosition))
        {
            return true;
        }
        
        // 밭 AABB
        return worldPosition.x >= transform.position.x - 0.5f &&
            worldPosition.x <= transform.position.x + FieldSize.x - 0.5f&&
            worldPosition.y >= transform.position.y - 0.5f &&
            worldPosition.y <= transform.position.y + FieldSize.y - 0.5f;
    }
    
    public void UnlockCropAt(Vector2 cropPosition)
    {
        // Unlock동작은 잠금상태에 무관하게 동작
        if (TryFindCropAt(cropPosition, out var crop))
        {
            crop.gameObject.SetActive(true);
            _lockedCrops.Remove(crop);  
        }
    }

    public bool OnSingleTap(Vector2 worldPosition)
    {
        if (IsAvailable
            && TryFindCropAt(worldPosition, out var crop)
            && !_lockedCrops.Contains(crop))
        {
            crop.OnSingleTap(worldPosition);
            return true;
        }
        
        if (SignAABB(worldPosition))
        {
            _onCropSignClicked?.Invoke();
            return true;
        }

        return false;
    }

    public void OnWatering(Vector2 worldPosition)
    {
        if (IsAvailable
            && TryFindCropAt(worldPosition, out var crop)
            && !_lockedCrops.Contains(crop))
        {
            crop.OnWatering();
        }
    }

    public bool OnSingleHolding(Vector2 initialWorldPosition, Vector2 deltaWorldPosition, bool isEnd, float deltaHoldTime)
    {
        if (IsAvailable
            && TryFindCropAt(initialWorldPosition, out var crop)
            && !_lockedCrops.Contains(crop))
        {
            crop.OnSingleHolding(initialWorldPosition, deltaWorldPosition, isEnd, deltaHoldTime);
            return true;
        }

        return false;
    }
    
    public void Init(Func<ProductEntry, int> getQuotaFunction, Action<ProductEntry, Vector2, int> fillQuotaFunction, Action<ProductEntry> signClickedHandler)
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
        _onCropSignClicked = () => signClickedHandler(ProductEntry);
    }

    public void Reset() => Array.ForEach(_crops, crop => crop.ResetToInitialState());

    public void OnFarmUpdate(float deltaTime)
    {
        foreach (var crop in _crops)
        {
            crop.OnFarmUpdate(deltaTime);
        }
    }

	private void OnAvailabilityChanged()
    {
        var backgroundColor = _isAvailable ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1.0f);
        var fieldLockedColor = _isAvailable ? new Color(0.0f, 0.0f, 0.0f, 0.0f) : Color.white;
        
        Array.ForEach(_crops, c => c.gameObject.SetActive(_isAvailable));
        _cropBackgrounds.ForEach(c => c.color = backgroundColor);

        _backgroundRenderer.color = backgroundColor;
        _fieldLockedRenderer.color = fieldLockedColor;
	}

	private void Awake()
    {        
        _lockedCrops = new();
        _crops = new Crop[fieldSize.x * fieldSize.y];
        _cropBackgrounds = new();
        
        _backgroundRenderer = transform.Find("Background").GetComponent<SpriteRenderer>();
        _fieldLockedRenderer = transform.Find("FieldLockedDisplay").GetComponent<SpriteRenderer>();

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
                _cropBackgrounds.Add(cropBackgroundObject.GetComponent<SpriteRenderer>());
            }
        }

        _cropSign = GetComponentInChildren<CropSign>();

        OnAvailabilityChanged();
	}
    
    /// <summary>
    /// 입력된 좌표에 해당되는 Crop을 검색해서 반환합니다.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="crop"></param>
    /// <returns>position에 해당하는 Crop이 존재할 경우 crop에 값이 할당되며 true 반환, 이외의 경우 crop에 null이 할당되며 false 반환</returns>
    private bool TryFindCropAt(Vector2 position, out Crop crop)
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

    private bool SignAABB(Vector2 worldPosition) =>
        Mathf.Abs(_cropSign.transform.position.x - worldPosition.x) < CropSign.SignClickSize.x &&
        Mathf.Abs(_cropSign.transform.position.y - worldPosition.y) < CropSign.SignClickSize.y;
}
