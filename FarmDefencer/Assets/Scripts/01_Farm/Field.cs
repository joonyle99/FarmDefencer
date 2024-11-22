using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class Field : MonoBehaviour, IFarmUpdatable
{
    /// <summary>
    /// ���� �۹��� ��Ʈ��, ���� ��ǥ���� Ÿ�� ��ǥ, �׸��� ������ȭ ��� ó�� �ݹ��� ������� �̺�Ʈ�Դϴ�.
    /// <seealso cref="Crop.OnTryItemify"/>�� �����ϼ���.
    /// </summary>
    public UnityEvent<ProductEntry, Vector2Int, UnityAction<bool>> OnTryItemify;
    public GameObject CropPrefab;
    /// <summary>
    /// Farm ������Ʈ ��ġ�� ���� ��� ��ġ�Դϴ�.
    /// Field�� ���� ���� �Ʒ� Ÿ���� ��ġ�� �����մϴ�.
    /// </summary>
    public Vector2Int FieldLocalPosition;
    public Vector2Int FieldSize;
    public TileBase FlowedTile;

    /// <summary>
    /// �� Field�� �ɱ� Crop�� ProductEntry�Դϴ�.
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

	/// <summary>
	/// �Էµ� ��ǥ�� �ش�Ǵ� Crop�� �˻��ؼ� ��ȯ�մϴ�.
	/// </summary>
	/// <param name="position"></param>
	/// <param name="crop"></param>
	/// <returns>position�� �ش��ϴ� Crop�� ������ ��� crop�� ���� �Ҵ�Ǹ� true ��ȯ, �̿��� ��� crop�� null�� �Ҵ�Ǹ� false ��ȯ</returns>
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

    private void OnAvailabilityChanged()
    {
        var color = _isAvailable ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1.0f);
		for (int yOffset = 0; yOffset < FieldSize.y; yOffset++)
		{
			for (int xOffset = 0; xOffset < FieldSize.x; xOffset++)
			{
                _tilemap.SetTileFlags(new Vector3Int(xOffset, yOffset), TileFlags.None);
				_tilemap.SetColor(new Vector3Int(xOffset, yOffset), color);
			}
		}
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

        _productEntry = CropPrefab.GetComponent<Crop>().ProductEntry;

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

		IsAvailable = false;
	}

	private void Start()
	{
		foreach (var crop in _crops)
        {
			crop.OnTryItemify.AddListener((callback) => OnTryItemify.Invoke(crop.ProductEntry, crop.Position, callback));
		}
	}
}
