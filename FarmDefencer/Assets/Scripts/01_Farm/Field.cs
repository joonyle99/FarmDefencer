using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.Rendering.DebugUI;

public class Field : MonoBehaviour
{
    public GameObject CropPrefab;
    public Vector2Int FieldPosition;
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
        if (position.x < transform.position.x || position.x > transform.position.x + FieldSize.x
        || position.y < transform.position.y || position.y > transform.position.y + FieldSize.y)
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
