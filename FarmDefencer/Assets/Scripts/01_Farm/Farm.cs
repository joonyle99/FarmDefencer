using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 자식으로는 반드시 Field 오브젝트만 가지게 할 것.
/// </summary>
public sealed class Farm : MonoBehaviour, IFarmUpdatable, IFarmInputLayer
{
    private bool _isFarmPaused;
    private Field[] _fields;
    private HashSet<Crop> _lockedCrops;

    public bool TryGetLockableCropPositionFromProbability(IReadOnlyList<CropProbabilityData> cropProbabilityDatas,
        out Vector2 cropPosition)
    {
        var randomMax = 0.0f;
        foreach (var cropProbability in cropProbabilityDatas)
        {
            var field = _fields.FirstOrDefault(f =>
                f.ProductEntry.ProductName.Equals(cropProbability.TargetCrop.ProductName));
            if (field is null || !field.IsAvailable || field.Crops.All(c => _lockedCrops.Contains(c)))
            {
                continue;
            }

            randomMax += cropProbability.Probability;
        }

        if (randomMax == 0.0f)
        {
            cropPosition = Vector2.zero;
            return false;
        }

        var randomValue = Random.Range(0.0f, randomMax);
        Field targetField = null;
        foreach (var cropProbability in cropProbabilityDatas)
        {
            var field = _fields.FirstOrDefault(f =>
                f.ProductEntry.ProductName.Equals(cropProbability.TargetCrop.ProductName));
            if (field is null || !field.IsAvailable || field.Crops.All(c => _lockedCrops.Contains(c)))
            {
                continue;
            }

            randomValue -= cropProbability.Probability;
            if (randomValue <= 0.0f)
            {
                targetField = field;
                break;
            }
        }

        if (targetField is null)
        {
            cropPosition = Vector2.zero;
            return false;
        }

        // tryCount만큼 랜덤 배정을 시도
        for (int tryCount = 0; tryCount < 10; ++tryCount)
        {
            var flattenedPosition = Random.Range(0, targetField.FieldSize.x * targetField.FieldSize.y);
            var cropX = targetField.transform.position.x + flattenedPosition % targetField.FieldSize.x;
            var cropY = targetField.transform.position.y + flattenedPosition / targetField.FieldSize.x;

            if (targetField.TryFindCropAt(new Vector2(cropX, cropY), out var crop) && !_lockedCrops.Contains(crop))
            {
                cropPosition = new Vector2(cropX, cropY);
                return true;
            }
        }

        // 랜덤 배정에 실패했으면 가장 처음 유효한 Crop의 위치를 반환
        var firstCrop = targetField.Crops.FirstOrDefault(c => !_lockedCrops.Contains(c));
        cropPosition = firstCrop is not null
            ? firstCrop.transform.position
            : Vector2.zero;
        return firstCrop is not null;
    }

    public bool TryLockCropAt(Vector2 cropPosition)
    {
        foreach (var field in _fields)
        {
            if (field.TryFindCropAt(cropPosition, out var crop))
            {
                if (!_lockedCrops.Contains(crop))
                {
                    crop.ResetToInitialState();
                    crop.gameObject.SetActive(false);
                }

                return _lockedCrops.Add(crop);
            }
        }

        return false;
    }

    public void UnlockCropAt(Vector2 cropPosition)
    {
        // Unlock동작은 잠금상태에 무관하게 동작
        foreach (var field in _fields)
        {
            if (field.TryFindCropAt(cropPosition, out var crop))
            {
                crop.gameObject.SetActive(true);
                _lockedCrops.Remove(crop);  
            }
        }
    }

    public bool OnSingleTap(Vector2 worldPosition) =>
        DoCropActionTo(crop => crop.OnSingleTap(worldPosition), worldPosition);

    public bool OnSingleHolding(Vector2 initialWorldPosition, Vector2 deltaWorldPosition, bool isEnd,
        float deltaHoldTime)
        => DoCropActionTo(crop => crop.OnSingleHolding(initialWorldPosition, deltaWorldPosition, isEnd, deltaHoldTime),
            initialWorldPosition);

    public void WateringAction(Vector2 position) => DoCropActionTo(crop => crop.OnWatering(), position);

    public void OnFarmUpdate(float deltaTime)
    {
        _isFarmPaused = deltaTime == 0.0f;
        Array.ForEach(_fields, field =>
        {
            if (field.IsAvailable)
            {
                field.OnFarmUpdate(deltaTime);
            }
        });
    }

    public bool GetFieldAvailability(string productUniqueId)
    {
        var field = _fields.FirstOrDefault(field => field.ProductEntry.ProductName == productUniqueId);
        if (field == null)
        {
            Debug.LogError($"Farm이 {productUniqueId}에 해당하는 Field를 가지고 있지 않습니다.");
            return false;
        }

        return field.IsAvailable;
    }

    public void SetFieldAvailability(string productUniqueId, bool value)
    {
        var field = _fields.FirstOrDefault(field => field.ProductEntry.ProductName == productUniqueId);
        if (field == null)
        {
            Debug.LogError($"Farm이 {productUniqueId}에 해당하는 Field를 가지고 있지 않습니다.");
            return;
        }

        field.IsAvailable = value;
    }

    public void Init(Func<ProductEntry, int> getQuotaFunction, Action<ProductEntry, Vector2, int> fillQuotaFunction)
    {
        Array.ForEach(_fields, field => field.Init(getQuotaFunction, fillQuotaFunction));
    }

    private void Awake()
    {
        _fields = new Field[transform.childCount];
        _lockedCrops = new();

        for (int childIndex = 0; childIndex < transform.childCount; ++childIndex)
        {
            var childObject = transform.GetChild(childIndex);
            var fieldComponent = childObject.GetComponent<Field>();

            _fields[childIndex] = fieldComponent;
        }
    }

    private bool DoCropActionTo(Action<Crop> action, Vector2 cropWorldPosition)
    {
        if (_isFarmPaused)
        {
            return false;
        }

        foreach (var field in _fields)
        {
            if (field.IsAvailable
                && field.TryFindCropAt(cropWorldPosition, out var crop)
                && !_lockedCrops.Contains(crop))
            {
                action(crop);
                return true;
            }
        }

        return false;
    }
}