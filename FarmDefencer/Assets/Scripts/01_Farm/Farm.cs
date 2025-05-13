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

    public int InputPriority => IFarmInputLayer.Priority_Farm;

    public void UpdateAvailability(Func<ProductEntry, bool> isFieldAvailable)
    {
        foreach (var field in _fields)
        {
            field.IsAvailable = isFieldAvailable(field.ProductEntry);
        }
    }

    public bool TryGetLockableCropPositionFromProbability(IReadOnlyList<CropProbabilityData> cropProbabilityDatas,
        out Vector2 cropPosition)
    {
        var randomMax = 0.0f;
        foreach (var cropProbability in cropProbabilityDatas)
        {
            var field = _fields.FirstOrDefault(f =>
                f.ProductEntry.ProductName.Equals(cropProbability.TargetCrop.ProductName));
            if (field is null || !field.IsAvailable || field.AllLocked)
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
            if (field is null || !field.IsAvailable || field.AllLocked)
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

            if (targetField.IsCropLockable(new Vector2(cropX, cropY)))
            {
                cropPosition = new Vector2(cropX, cropY);
                return true;
            }
        }

        // 랜덤 배정에 실패했으면 가장 처음 유효한 Crop의 위치를 반환
        var found = targetField.TryGetFirstLockableCropPosition(out cropPosition);
        return found;
    }

    public bool TryLockCropAt(Vector2 cropPosition) =>_fields.Any(field => field.TryLockCropAt(cropPosition));

    public void UnlockCropAt(Vector2 cropPosition) => Array.ForEach(_fields, f => f.UnlockCropAt(cropPosition));

    public bool OnSingleTap(Vector2 worldPosition) =>
        DoActionTo(field => field.OnSingleTap(worldPosition), worldPosition);

    public bool OnSingleHolding(Vector2 initialWorldPosition, Vector2 deltaWorldPosition, bool isEnd,
        float deltaHoldTime)
        => DoActionTo(field => field.OnSingleHolding(initialWorldPosition, deltaWorldPosition, isEnd, deltaHoldTime),
            initialWorldPosition);

    public void WateringAction(Vector2 position) => DoActionTo(field => field.OnWatering(position), position);

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

    public void Init(Func<ProductEntry, int> getQuotaFunction, 
        Action<ProductEntry, Vector2, int> fillQuotaFunction,
        Action<ProductEntry> signClickedHandler)
    {
        Array.ForEach(_fields, field => field.Init(getQuotaFunction, fillQuotaFunction, signClickedHandler));
    }

    private void Awake()
    {
        _fields = new Field[transform.childCount];

        for (int childIndex = 0; childIndex < transform.childCount; ++childIndex)
        {
            var childObject = transform.GetChild(childIndex);
            var fieldComponent = childObject.GetComponent<Field>();

            _fields[childIndex] = fieldComponent;
        }
    }

    private bool DoActionTo(Action<Field> action, Vector2 worldPosition)
    {
        if (_isFarmPaused)
        {
            return false;
        }

        foreach (var field in _fields)
        {
            if (field.IsAvailable
                && field.AABB(worldPosition))
            {
                action(field);
                return true;
            }
        }

        return false;
    }
}