using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Field : MonoBehaviour
{
    public GameObject CropPrefab;
    /// <summary>
    /// Farm ������Ʈ ��ġ�� ���� ��� ��ġ�Դϴ�.
    /// Field�� ���� ���� �Ʒ� Ÿ���� ��ġ�� �����մϴ�.
    /// </summary>
    public Vector2Int FieldLocalPosition;
    public Vector2Int FieldSize;
    public TileBase FlowedTile;

    private Tilemap _tilemap;
    private List<Crop> _crops;

	/// <summary>
	/// �Էµ� ��ǥ�� �ش�Ǵ� Crop�� �˻��ؼ� ��ȯ�մϴ�.
	/// </summary>
	/// <param name="position"></param>
	/// <param name="crop"></param>
	/// <returns>position�� �ش��ϴ� Crop�� ������ ��� crop�� ���� �Ҵ�Ǹ� true ��ȯ, �̿��� ��� crop�� null�� �Ҵ�Ǹ� false ��ȯ</returns>
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
    /// �Էµ� ��ǥ�� �ش��ϸ� T�� �����ϴ� Crop�� �˻��ؼ� ��ȯ�մϴ�.
    /// �� ���׸� �޼ҵ� TryFindCropAt()�� is/as�� ���� �ٿ�ĳ������ ��� ����ϴ� ����� ���� �����Դϴ�.
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
            throw new MissingComponentException("Field�� ������ CropPrefab�� null�̰ų� Crop ������Ʈ�� ������ �ʽ��ϴ�.");
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
