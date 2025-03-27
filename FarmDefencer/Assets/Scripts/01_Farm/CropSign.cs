using UnityEngine;

public sealed class CropSign : MonoBehaviour
{
	public ProductEntry ProductEntry => productEntry;
	[SerializeField] private ProductEntry productEntry;
}
