using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class Field : MonoBehaviour, IFarmUpdatable
{
    public GameObject CropPrefab;
    /// <summary>
    /// Farm 오브젝트 위치에 대한 상대 위치입니다.
    /// Field의 가장 왼쪽 아래 타일의 위치와 동일합니다.
    /// </summary>
    public Vector2Int FieldLocalPosition;
    public Vector2Int FieldSize;

    /// <summary>
    /// 이 Field에 심길 Crop의 ProductEntry입니다.
    /// </summary>
    public ProductEntry ProductEntry => _productEntry;
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
    private ProductEntry _productEntry;
    private Tilemap _tilemap;
    private List<Crop> _crops;
    private GameObject _fieldLockedDisplayObject;

	/// <summary>
	/// 입력된 좌표에 해당되는 Crop을 검색해서 반환합니다.
	/// </summary>
	/// <param name="position"></param>
	/// <param name="crop"></param>
	/// <returns>position에 해당하는 Crop이 존재할 경우 crop에 값이 할당되며 true 반환, 이외의 경우 crop에 null이 할당되며 false 반환</returns>
	public bool TryFindCropAt(Vector2 position, out Crop crop)
	{
        if (position.x < transform.position.x - 0.5f || position.x > transform.position.x + FieldSize.x - 0.5f
        || position.y < transform.position.y - 0.5f || position.y > transform.position.y + FieldSize.y - 0.5f)
        {
            crop = null;
            return false;
        }

        foreach (var cropItem in _crops)
        {
            if (cropItem.IsLocatedAt(position))
            {
                crop = cropItem;
                return true;
            }
        }

        crop = null;
        return false;
	}

    public void OnFarmUpdate(float deltaTime)
    {
        foreach (var crop in _crops)
        {
            crop.OnFarmUpdate(deltaTime);
        }
    }

	public void Init(
        GameObject fieldLockedDisplayPrefab,
        GameObject cropLockedDisplayPrefab,
        TileBase flowedTile,
        UnityAction<ProductEntry, Vector2Int, int, UnityAction<bool>> onTryItemifyAction)
	{
        _fieldLockedDisplayObject = Instantiate(fieldLockedDisplayPrefab);
        _fieldLockedDisplayObject.SetActive(false);
        _fieldLockedDisplayObject.transform.SetParent(transform, false);
        _fieldLockedDisplayObject.transform.localPosition = new Vector2(FieldSize.x / 2.0f, FieldSize.y / 2.0f);

        IsAvailable = false;

		for (int yOffset = 0; yOffset < FieldSize.y; yOffset++)
		{
			for (int xOffset = 0; xOffset < FieldSize.x; xOffset++)
			{
				_tilemap.SetTile(new Vector3Int(xOffset, yOffset), flowedTile);
				var cropObjectPosition = new Vector3(transform.position.x + xOffset, transform.position.y + yOffset, transform.position.z - 1.0f);
				var cropObject = Instantiate(CropPrefab);
				var cropComponent = cropObject.GetComponent<Crop>();
				cropObject.transform.parent = transform;
				cropObject.transform.position = cropObjectPosition;

				cropComponent.Init(cropLockedDisplayPrefab, (count, afterItemifyCallback) => onTryItemifyAction(ProductEntry, cropComponent.Position, count, afterItemifyCallback));
				_crops.Add(cropComponent);
			}
		}
	}

	private void OnAvailabilityChanged()
    {
        var color = _isAvailable ? Color.white : new Color(0.4f, 0.7f, 1.0f, 1.0f);
		for (int yOffset = 0; yOffset < FieldSize.y; yOffset++)
		{
			for (int xOffset = 0; xOffset < FieldSize.x; xOffset++)
			{
                _tilemap.SetTileFlags(new Vector3Int(xOffset, yOffset), TileFlags.None);
				_tilemap.SetColor(new Vector3Int(xOffset, yOffset), color);
			}
		}

		_fieldLockedDisplayObject.SetActive(!_isAvailable);
	}

	private void Awake()
    {
        _tilemap = GetComponentInChildren<Tilemap>();
        _crops = new List<Crop>();

        if (CropPrefab == null
            || !CropPrefab.TryGetComponent<Crop>(out var _))
        {
            throw new MissingComponentException("Field에 지정된 CropPrefab이 null이거나 Crop 컴포넌트를 가지지 않습니다.");
        }

        _productEntry = CropPrefab.GetComponent<Crop>().ProductEntry;
	}
}
