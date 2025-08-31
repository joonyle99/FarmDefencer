using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public sealed class Farm : MonoBehaviour, IFarmUpdatable, IFarmInputLayer, IFarmSerializable
{
    private bool _isFarmPaused;
    private Field[] _fields;

    public int InputPriority => IFarmInputLayer.Priority_Farm;

    public void ApplyCropCommand(CropCommand cropCommand)
    {
        foreach (var field in _fields)
        {
            if (!field.IsAvailable)
            {
                continue;
            }
            
            field.ApplyCropCommand(cropCommand);
        }
    }

    public JObject Serialize()
    {
        var jsonFields = new JObject();

        foreach (var field in _fields)
        {
            jsonFields.Add(field.ProductEntry.ProductName, field.Serialize());
        }
        
        return new JObject(new JProperty("Fields", jsonFields));
    }

    public void Deserialize(JObject json)
    {
        if (json["Fields"] is not JObject jsonFields)
        {
            return;
        }

        foreach (var property in jsonFields.Properties())
        {
            if (property.Value.Type != JTokenType.Object)
            {
                Debug.LogError($"Farm/Fields에 Object가 아닌 값을 가지는 키 Product Name(Field Name) 존재: {property.Name}");
                continue;
            }
            
            var field = _fields.FirstOrDefault(f => f.ProductEntry.ProductName.Equals(property.Name));
            if (field is null)
            {
                Debug.LogError($"Farm/Fields에 알 수 없는 Product Name(Field Name) 존재: {property.Name}");
                continue;
            }
            
            field.Deserialize((JObject)property.Value);
        }
    }

    public bool TryGetLockableCropPositionFromProbability(IReadOnlyList<CropProbabilityData> cropProbabilityDatas,
        out Vector2 cropPosition, out ProductEntry productEntry)
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
            productEntry = null;
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
            productEntry = null;
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
                productEntry = targetField.ProductEntry;
                return true;
            }
        }

        // 랜덤 배정에 실패했으면 가장 처음 유효한 Crop의 위치를 반환
        var found = targetField.TryGetFirstLockableCropPosition(out cropPosition);
        productEntry = found ? targetField.ProductEntry : null;
        return found;
    }

    public bool TryLockCropAt(Vector2 cropPosition) =>_fields.Any(field => field.TryLockCropAt(cropPosition));

    public void UnlockCropAt(Vector2 cropPosition) => Array.ForEach(_fields, f => f.UnlockCropAt(cropPosition));

    public bool OnTap(Vector2 worldPosition) =>
        DoActionTo(field => field.OnTap(worldPosition), worldPosition);

    public bool OnHold(Vector2 initialWorldPosition, Vector2 deltaWorldPosition, bool isEnd,
        float deltaHoldTime)
        => DoActionTo(field => field.OnHold(initialWorldPosition, deltaWorldPosition, isEnd, deltaHoldTime),
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

    public void Init(
        Func<bool> isPestRunning,
        Func<string, bool> isFieldAvailable,
        Action<Vector2> onPlanted,
        Action<ProductEntry, Vector2, int> onSold,
        Action<ProductEntry> onSignClicked)
    {
        Array.ForEach(_fields,
            field =>
            {
                field.IsAvailable = isFieldAvailable(field.ProductEntry.ProductName);
                field.Init(isPestRunning, onPlanted,
                    (cropWorldPosition, count) => onSold(field.ProductEntry, cropWorldPosition, count), onSignClicked);
            });
    }

    private void Awake()
    {
        var fields = new List<Field>();

        for (var childIndex = 0; childIndex < transform.childCount; ++childIndex)
        {
            var childObject = transform.GetChild(childIndex);
            if (!childObject.TryGetComponent<Field>(out var fieldComponent))
            {
                continue;
            }

            fields.Add(fieldComponent);
        }
        
        _fields = fields.ToArray();
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