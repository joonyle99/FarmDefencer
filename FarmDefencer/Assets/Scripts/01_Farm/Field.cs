using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Field : MonoBehaviour
{
    public GameObject CropPrefab;
    /// <summary>
    /// Farm 오브젝트 위치에 대한 상대 위치입니다.
    /// Field의 가장 왼쪽 아래 타일의 위치와 동일합니다.
    /// </summary>
    public Vector2Int FieldLocalPosition;
    public Vector2Int FieldSize;
    public TileBase FlowedTile;

    private Tilemap _tilemap;
    private List<Crop> _crops;

	/// <summary>
	/// 입력된 좌표에 해당되는 Crop을 검색해서 반환합니다.
	/// </summary>
	/// <param name="position"></param>
	/// <param name="crop"></param>
	/// <returns>position에 해당하는 Crop이 존재할 경우 crop에 값이 할당되며 true 반환, 이외의 경우 crop에 null이 할당되며 false 반환</returns>
	public bool TryFindCropAt(Vector2 position, [CanBeNull] out Crop crop)
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

    /// <summary>
    /// 입력된 좌표에 해당하며 T를 구현하는 Crop을 검색해서 반환합니다.
    /// 비 제네릭 메소드 TryFindCropAt()과 is/as를 통한 다운캐스팅을 모두 사용하는 방법에 대한 숏컷입니다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="position"></param>
    /// <param name="specializedCrop"></param>
    /// <returns></returns>
    public bool TryFindCropAt<T>(Vector2 position, [CanBeNull] out T specializedCrop) where T : class
    {
        if (!TryFindCropAt(position, out var crop))
        {
            specializedCrop = null;
            return false;
        }

        if (crop is T specialized)
        {
            specializedCrop = specialized;
            return true;
        }

        specializedCrop = null;
        return false;
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

        for (int yOffset = 0; yOffset < FieldSize.y; yOffset++)
        {
            for (int xOffset = 0; xOffset < FieldSize.x; xOffset++)
            {
                _tilemap.SetTile(new Vector3Int(xOffset, yOffset), FlowedTile);

                var cropObjectPosition = new Vector3(transform.position.x + xOffset, transform.position.y + yOffset, transform.position.z - 1.0f);
                var cropObject = Instantiate(CropPrefab);
                var cropComponent = cropObject.GetComponent<Crop>();
                cropObject.transform.parent = transform;
                cropObject.transform.position = cropObjectPosition;
                _crops.Add(cropComponent);
            }
        }
    }
}
