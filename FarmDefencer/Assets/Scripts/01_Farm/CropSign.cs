using UnityEngine;

public class CropSign : MonoBehaviour
{
	public ProductEntry ProductEntry => _productEntry;
	[SerializeField] private ProductEntry _productEntry;
}
