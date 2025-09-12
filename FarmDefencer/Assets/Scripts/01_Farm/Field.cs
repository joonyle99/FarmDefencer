using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

/// <summary>
/// 0번 자식: 배경
/// 1번 자식: 잠금 디스플레이
/// </summary>
public sealed class Field : MonoBehaviour, IFarmUpdatable, IFarmInputLayer, IFarmSerializable
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

    private Crop[] _crops;
    private SpriteRenderer _backgroundRenderer; // 0번 자식 오브젝트에 할당
    private SpriteRenderer _fieldLockedRenderer; // 1번 자식 오브젝트에 할당

    public Vector2 CropSignWorldPosition => _cropSign.transform.position;

    public bool IsAvailable
    {
        get => _isAvailable;
        set
        {
            _isAvailable = value;
            OnAvailabilityChanged();
        }
    }

    private bool _isAvailable;
    private Func<bool> _isPestRunning;
    private bool _isHolding;
    private Crop _holdingCrop;

    public Crop TopLeftCrop => _crops[FieldSize.x * (FieldSize.y - 1)];

    public bool AllLocked => Array.TrueForAll(_crops, c => _lockedCrops.Contains(c));

    public void ApplyCropCommand(CropCommand cropCommand)
    {
        foreach (var crop in _crops)
        {
            crop.ApplyCommand(cropCommand);
        }
    }

    public JObject Serialize()
    {
        var jsonCrops = new JObject();
        for (int i = 0; i < _crops.Length; ++i)
        {
            jsonCrops.Add(i.ToString(), _crops[i].Serialize());
        }

        return new JObject(new JProperty("Crops", jsonCrops));
    }

    public void Deserialize(JObject json)
    {
        if (json["Crops"] is not JObject)
        {
            return;
        }

        foreach (var property in ((JObject)json["Crops"]).Properties())
        {
            if (!int.TryParse(property.Name, out var cropIndex) || cropIndex < 0 || cropIndex >= _crops.Length)
            {
                Debug.LogError($"Field {ProductEntry.ProductName} 에 잘못된 Crop 인덱스 존재: {property.Name}");
                continue;
            }

            if (property.Value.Type != JTokenType.Object)
            {
                Debug.LogError($"Field {ProductEntry.ProductName} 의 {cropIndex} 가 Object를 값으로 갖지 않습니다.");
                continue;
            }

            var crop = _crops[cropIndex];
            crop.Deserialize((JObject)property.Value);
        }
    }

    public bool TryLockCropAt(Vector2 cropPosition)
    {
        if (TryFindCropAt(cropPosition, out var crop))
        {
            if (!_lockedCrops.Contains(crop))
            {
                crop.CropDisplay.ManualMode = true;
                crop.ResetToInitialState();
                crop.gameObject.SetActive(false);
            }

            return _lockedCrops.Add(crop);
        }

        return false;
    }

    public void UpdateCropGaugeManually(Vector2 cropPosition, float ratio)
    {
        if (!TryFindCropAt(cropPosition, out var crop) ||
            !crop.CropDisplay.ManualMode)
        {
            return;
        }

        crop.CropDisplay.UpdateGauge(ratio);
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
               worldPosition.x <= transform.position.x + FieldSize.x - 0.5f &&
               worldPosition.y >= transform.position.y - 0.5f &&
               worldPosition.y <= transform.position.y + FieldSize.y - 0.5f;
    }

    public void UnlockCropAt(Vector2 cropPosition)
    {
        // Unlock동작은 잠금상태에 무관하게 동작
        if (TryFindCropAt(cropPosition, out var crop))
        {
            crop.CropDisplay.ManualMode = false;
            crop.gameObject.SetActive(true);
            _lockedCrops.Remove(crop);
        }
    }

    public bool OnTap(Vector2 worldPosition)
    {
        if (IsAvailable
            && TryFindCropAt(worldPosition, out var crop)
            && !_lockedCrops.Contains(crop))
        {
            crop.OnTap(worldPosition);
            return true;
        }

        if (SignAABB(worldPosition) && !_isPestRunning())
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

    public bool OnHold(Vector2 initialWorldPosition, Vector2 deltaWorldPosition, bool isEnd,
        float deltaHoldTime)
    {
        if (isEnd)
        {
            _holdingCrop?.OnHold(initialWorldPosition, deltaWorldPosition, true, deltaHoldTime);
            _isHolding = false;
            _holdingCrop = null;
            return true;
        }

        if (!IsAvailable)
        {
            return false;
        }

        if (!_isHolding) // 첫 홀드 프레임. 홀드인지 스윕인지 판별해야 함.
        {
            if (TryFindCropAt(initialWorldPosition, out var crop)
                && !_lockedCrops.Contains(crop))
            {
                if (crop.OnHold(initialWorldPosition, deltaWorldPosition, false, deltaHoldTime))
                {
                    _holdingCrop = crop;
                }
            }
            else
            {
                _holdingCrop = null;
            }

            _isHolding = true;
            return true;
        }

        // 지속 홀드 프레임.
        if (_holdingCrop is not null)
        {
            _holdingCrop.OnHold(initialWorldPosition, deltaWorldPosition, false, deltaHoldTime);
        }
        else
        {
            if (TryFindCropAt(initialWorldPosition + deltaWorldPosition, out var crop)
                && !_lockedCrops.Contains(crop))
            {
                crop.OnHold(initialWorldPosition, deltaWorldPosition, true, deltaHoldTime);
            }
        }

        return true;
    }

    public void Init(
        Func<bool> isPestRunning,
        Func<ProductEntry, (int, int)> getPrice,
        Action<Vector2> onPlanted,
        Action<Vector2, int> onSold,
        Action<ProductEntry> signClickedHandler,
        CropDisplay cropDisplayObjectToClone)
    {
        _isPestRunning = isPestRunning;
        Array.ForEach(
            _crops,
            crop => crop.Init(
                () => onPlanted(crop.transform.position),
                count => onSold(crop.transform.position, count),
                cropDisplayObjectToClone));
        _onCropSignClicked = () => signClickedHandler(ProductEntry);
        _cropSign.Init(ProductEntry, () => getPrice(ProductEntry));
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

        _cropSign = transform.Find("CropSign").GetComponent<CropSign>();

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
        var localPosition = new Vector2Int(Mathf.RoundToInt(position.x - transform.position.x),
            Mathf.RoundToInt(position.y - transform.position.y));
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