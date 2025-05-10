using UnityEngine;

public sealed class CropSign : MonoBehaviour
{
	public static readonly Vector2 SignClickSize = new Vector2 { x = 1.0f, y = 1.0f };
	
	public ProductEntry ProductEntry => productEntry;
	[SerializeField] private ProductEntry productEntry;
}
